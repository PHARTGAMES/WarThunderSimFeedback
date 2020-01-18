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
using System;
using System.Runtime.InteropServices;

namespace WTTelemetry
{

    /// <summary>
    /// The data packet for sending over udp + some named properties 
    /// for human friendly mapping and stateless calculations
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    public struct WTAPI
    {

        public int PacketId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] AccG;

        public float Yaw;
        public float Pitch;
        public float Roll;

        public void Initialize()
        {
            AccG = new float[3];
        }

        public float Heave
        {
            get
            {
                return AccG[1];
            }
        }

        public float Sway
        {
            get
            {
                return AccG[0];
            }
        }

        public float Surge
        {
            get
            {
                return AccG[2];
            }
        }

        /// <summary>
        /// Placeholder for the stateful property.
        /// This will propergate the Name list for available Telemetry Keys.
        /// </summary>
        public float Rumble
        {
            get { return 0; }
        }


        public byte[] ToByteArray()
        {
            WTAPI packet = this;
            int num = Marshal.SizeOf<WTAPI>(packet);
            byte[] array = new byte[num];
            IntPtr intPtr = Marshal.AllocHGlobal(num);
            Marshal.StructureToPtr<WTAPI>(packet, intPtr, false);
            Marshal.Copy(intPtr, array, 0, num);
            Marshal.FreeHGlobal(intPtr);
            return array;
        }
    }


}
