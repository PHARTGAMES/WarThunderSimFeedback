//MIT License
//
//Copyright(c) 2019 PHARTGAMES
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
//
using SimFeedback.log;
using SimFeedback.telemetry;
using System;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using System.Numerics;

namespace WTTelemetry
{
    /// <summary>
    /// War Thunder Telemetry Provider
    /// </summary>
    public sealed class WarThunderTelemetryProvider : AbstractTelemetryProvider
    {
        private bool isStopped = true;                                  // flag to control the polling thread
        private Thread t;
        private string stateURL = "http://127.0.0.1:8111/state";
        private string indicatorsURL = "http://127.0.0.1:8111/indicators";

        private Vector3 lastVelocity = new Vector3(0, 0, 0);
        WTAPI telemetryData = new WTAPI();

        /// <summary>
        /// Default constructor.
        /// Every TelemetryProvider needs a default constructor for dynamic loading.
        /// Make sure to call the underlying abstract class in the constructor.
        /// </summary>
        public WarThunderTelemetryProvider() : base()
        {
            Author = "PEZZALUCIFER";
            Version = "v1.0";
            BannerImage = @"img\banner_wt.png"; // Image shown on top of the profiles tab
            IconImage = @"img\warthunder.jpg";          // Icon used in the tree view for the profile
            TelemetryUpdateFrequency = 100;     // the update frequency in samples per second
            telemetryData.Initialize();
        }

        /// <summary>
        /// Name of this TelemetryProvider.
        /// Used for dynamic loading and linking to the profile configuration.
        /// </summary>
        public override string Name { get { return "warthunder"; } }

        public override void Init(ILogger logger)
        {
            base.Init(logger);
            Log("Initializing WarThunderTelemetryProvider");
        }

        /// <summary>
        /// A list of all telemetry names of this provider.
        /// </summary>
        /// <returns>List of all telemetry names</returns>
        public override string[] GetValueList()
        {
            return GetValueListByReflection(typeof(WTAPI));
        }

        /// <summary>
        /// Start the polling thread
        /// </summary>
        public override void Start()
        {
            if (isStopped)
            {
                LogDebug("Starting WarThunderTelemetryProvider");
                isStopped = false;
                t = new Thread(Run);
                t.Start();
            }
        }

        /// <summary>
        /// Stop the polling thread
        /// </summary>
        public override void Stop()
        {
            LogDebug("Stopping WarThunderTelemetryProvider");
            isStopped = true;
            if (t != null) t.Join();
        }

        /// <summary>
        /// The thread funktion to poll the telemetry data and send TelemetryUpdated events.
        /// </summary>
        private void Run()
        {
            WTAPI lastTelemetryData = new WTAPI();
            Session session = new Session();
            WebClient wc = new WebClient();
            Stopwatch sw = new Stopwatch();
            Stopwatch rollSmoothSW = new Stopwatch();
            sw.Start();
            rollSmoothSW.Start();

            while (!isStopped)
            {
                try
                {
                    string json = wc.DownloadString(indicatorsURL);
                    Dictionary<string, object> indicators = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
//                    json = wc.DownloadString(stateURL);
//                    Dictionary<string, object> state = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                    float deg2rad = (float)Math.PI / 180.0f;
                    bool valid = (bool)indicators["valid"];
                    if (valid)
                    {

                        float pitch = (float)(double)indicators["aviahorizon_pitch"];
                        float roll = (float)(double)indicators["aviahorizon_roll"];
                        float yaw = (float)(double)indicators["compass"] * deg2rad;
                        float speed = (float)(double)indicators["speed"];


                        telemetryData.Pitch = pitch;


                        float absRoll = Math.Abs(roll);

                        if(absRoll > 130.0f)
                        {
                            rollSmoothSW.Restart();
                        }

                        float smoothTime = 300.0f;
                        if (rollSmoothSW.ElapsedMilliseconds > smoothTime)
                        {
                            telemetryData.Roll = LoopAngle(roll, 90.0f);
                        }
                        else
                        {
                            float minLerp = 0.0001f;
                            float maxLerp = 1.0f;
                            float lerp = minLerp + ((rollSmoothSW.ElapsedMilliseconds / smoothTime) * (maxLerp-minLerp));

                            lerp = 0.02f;
                            telemetryData.Roll = Lerp(telemetryData.Roll, LoopAngle(roll, 90.0f), lerp);
                        }


                        pitch *= deg2rad;
                        roll *= deg2rad;

                        Vector3 fwd = new Vector3(
                            (float)Math.Cos(yaw) * (float)Math.Cos(pitch),
                            (float)Math.Sin(yaw) * (float)Math.Cos(pitch),
                            (float)Math.Sin(pitch));

                        fwd = Vector3.Normalize(fwd);
                        float temp = fwd.Y;
                        fwd.Y = fwd.Z;
                        fwd.Z = temp;

                        Vector3 velocity = fwd * speed;

                        velocity = Vector3.Lerp(lastVelocity, velocity, 0.5f);

                        Vector3 acceleration = velocity - lastVelocity;
                        lastVelocity = velocity;

                        Quaternion orientationQ = Quaternion.CreateFromAxisAngle(fwd, roll);

                        Matrix4x4 orientation = Matrix4x4.CreateFromQuaternion(orientationQ);

                        Matrix4x4 orientationInv = new Matrix4x4();

                        Matrix4x4.Invert(orientation, out orientationInv);

                        Vector3 localAcceleration = Vector3.Transform(acceleration, orientationInv);

                        telemetryData.AccG[0] = acceleration.X;
                        telemetryData.AccG[1] = acceleration.Y;
                        telemetryData.AccG[2] = acceleration.Z;

                        // otherwise we are connected
                        IsConnected = true;

                        IsRunning = true;

                        sw.Restart();

                        TelemetryEventArgs args = new TelemetryEventArgs(
                            new WTTelemetryInfo(telemetryData, lastTelemetryData));
                        RaiseEvent(OnTelemetryUpdate, args);

                        lastTelemetryData = telemetryData;
                    }
                    else if (sw.ElapsedMilliseconds > 500)
                    {
                        IsRunning = false;
                    }

                }
                catch (Exception e)
                {
                    LogError("WarThunderTelemetryProvider Exception while processing data", e);
                    IsConnected = false;
                    IsRunning = false;
                    Thread.Sleep(1000);
                }
            }

            IsConnected = false;
            IsRunning = false;
        }

        //Reverses angles greater than minMag to a range between minMag and 0
        private float LoopAngle(float angle, float minMag)
        {

            float absAngle = Math.Abs(angle);

            if (absAngle <= minMag)
            {
                return angle;
            }

            float direction = angle / absAngle;

            //(180.0f * 1) - 135 = 45
            //(180.0f *-1) - -135 = -45
            float loopedAngle = (180.0f * direction) - angle;

            return loopedAngle;
        }

        float Lerp(float a, float b, float f)
        {
            return a + f * (b - a);
        }
    }
}
