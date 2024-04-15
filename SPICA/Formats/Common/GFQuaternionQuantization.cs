using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SPICA.Formats.Common
{
    public struct GFPackedQuaternion
    {
        public UInt16 x;
        public UInt16 y;
        public UInt16 z;
    }
    public static class GFQuaternionPacker
    {
        public const float PI_DIVISOR = (float)(Math.PI / UInt16.MaxValue);
        public const float PI_ADDEND = (float)(Math.PI / 4.0);

        public static float ExpandFloat(ulong i)
        {
            return i * PI_DIVISOR - PI_ADDEND;
        }

        public static ulong QuantizeFloat(float i)
        {
            short result = Convert.ToInt16((i + PI_ADDEND) / PI_DIVISOR);
            return Convert.ToUInt64(result & 0x7FFF);
        }

        public static Quaternion UnpackQuaternion(GFPackedQuaternion pq)
        {
            UInt64 pack = (ulong)((pq.z << 32) & (pq.y << 16) & (pq.x));
            int missingComponent = (int)(pack & 3);
            bool isNegative = (pack & 4) == 0;

            float tx = ExpandFloat((pack >> 3) & 0x7FFF);
            float ty = ExpandFloat((pack >> (15 + 3)) & 0x7FFF);
            float tz = ExpandFloat((pack >> (30 + 3)) & 0x7FFF);
            float tw = 1.0f - (tx * tx + ty * ty + tz * tz);

            if (tw < 0.0f) {
                tw = 0.0f;
            }

            tw = (float) Math.Sqrt(tw);

            Quaternion result = new Quaternion(tx, ty, tz, tw);

            result = Shuffle(result, missingComponent);

            if (isNegative)
            {
                result *= -1.0f;
            }

            return result;
        }

        public static GFPackedQuaternion PackQuaternion(Quaternion q)
        {
            q = Quaternion.Normalize(q);
            //Console.WriteLine($"Quaternion: {q.X} {q.Y} {q.Z}, {q.W}");

            List<float> qc = new List<float>{ q.X, q.Y, q.Z, q.W };
            
            float maxVal = qc.Max();
            float minVal = qc.Min();
            uint isNegative = 0;

            if (Math.Abs(minVal) > maxVal)
            {
                maxVal = minVal;
                isNegative = 1;
            }

            uint maxIndex = Convert.ToUInt16(qc.IndexOf(maxVal));
            
            ulong tx = 0;
            ulong ty = 0;
            ulong tz = 0;

            if (isNegative == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    qc[i] *= -1.0f;
                }
            }

            switch (maxIndex)
            {
                case 0: 
                    tx = QuantizeFloat(qc[1]);
                    ty = QuantizeFloat(qc[2]);
                    tz = QuantizeFloat(qc[3]);
                    break;
                case 1: 
                    tx = QuantizeFloat(qc[0]);
                    ty = QuantizeFloat(qc[2]);
                    tz = QuantizeFloat(qc[3]);
                    break;
                case 2: 
                    tx = QuantizeFloat(qc[0]);
                    ty = QuantizeFloat(qc[1]);
                    tz = QuantizeFloat(qc[3]);
                    break;
                case 3: 
                    tx = QuantizeFloat(qc[0]);
                    ty = QuantizeFloat(qc[1]);
                    tz = QuantizeFloat(qc[2]);
                    break;
            }

            ulong pack = ((tz << 30) | (ty << 15) | (tx));
            pack = (pack << 3) | ((isNegative << 2) | maxIndex);
            
            //Console.WriteLine($"Pack: {pack}");
            
            GFPackedQuaternion packed = new GFPackedQuaternion()
            {
                x = Convert.ToUInt16(pack & UInt16.MaxValue),
                y = Convert.ToUInt16((pack >> 16) & UInt16.MaxValue),
                z = Convert.ToUInt16((pack >> 32) & UInt16.MaxValue)

            };

            //Console.WriteLine($"Mapping: {maxIndex}, Values: {packed.x}, {packed.y}, {packed.z}");

            return packed;
        }


        public static Quaternion Shuffle(Quaternion q, int missingComponent)
        {
            switch(missingComponent)
            {
                case 0:
                    return new Quaternion(q.W, q.Y, q.Z, q.X);
                case 1:
                    return new Quaternion(q.Y, q.W, q.Z, q.X);
                case 2:
                    return new Quaternion(q.Y, q.Z, q.W, q.X);
                case 3:
                default:
                    return new Quaternion(q.X, q.Y, q.Z, q.W);
            }

        }
    }
}
