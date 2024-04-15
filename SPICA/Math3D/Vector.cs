using System;
using System.IO;
using System.Numerics;

namespace SPICA.Math3D
{
    static class VectorExtensions
    {
        public static Vector2 ReadVector2(this BinaryReader Reader)
        {
            return new Vector2(
                Reader.ReadSingle(),
                Reader.ReadSingle());
        }

        public static Vector3 ReadVector3(this BinaryReader Reader)
        {
            return new Vector3(
                Reader.ReadSingle(),
                Reader.ReadSingle(),
                Reader.ReadSingle());
        }

        public static Vector4 ReadVector4(this BinaryReader Reader)
        {
            return new Vector4(
                Reader.ReadSingle(),
                Reader.ReadSingle(),
                Reader.ReadSingle(),
                Reader.ReadSingle());
        }

        public static Quaternion ReadQuaternion(this BinaryReader Reader)
        {
            return new Quaternion(
                Reader.ReadSingle(),
                Reader.ReadSingle(),
                Reader.ReadSingle(),
                Reader.ReadSingle());
        }

        public static void Write(this BinaryWriter Writer, Vector2 v)
        {
            Writer.Write(v.X);
            Writer.Write(v.Y);
        }

        public static void Write(this BinaryWriter Writer, Vector3 v)
        {
            Writer.Write(v.X);
            Writer.Write(v.Y);
            Writer.Write(v.Z);
        }

        public static void Write(this BinaryWriter Writer, Vector4 v)
        {
            Writer.Write(v.X);
            Writer.Write(v.Y);
            Writer.Write(v.Z);
            Writer.Write(v.W);
        }

        public static void Write(this BinaryWriter Writer, Quaternion q)
        {
            Writer.Write(q.X);
            Writer.Write(q.Y);
            Writer.Write(q.Z);
            Writer.Write(q.W);
        }

        public static Quaternion CreateRotationBetweenVectors(Vector3 a, Vector3 b)
        {
            float qw = Vector3.Dot(a, b) + (float)Math.Sqrt(a.LengthSquared() * b.LengthSquared());

            Quaternion Rotation = new Quaternion(Vector3.Cross(a, b), qw);

            return Quaternion.Normalize(Rotation);
        }

        public static Vector3 ToEuler(this Quaternion q)
        {
            return new Vector3(
                (float)Math.Atan2(2 * (q.X * q.W + q.Y * q.Z), 1 - 2 * (q.X * q.X + q.Y * q.Y)),
                -(float)Math.Asin(2 * (q.X * q.Z - q.W * q.Y)),
                (float)Math.Atan2(2 * (q.X * q.Y + q.Z * q.W), 1 - 2 * (q.Y * q.Y + q.Z * q.Z)));
        }

        public static Quaternion ToQuaternion(this Vector3 v)
        {

            float cy = (float)Math.Cos(v.Z * 0.5);
            float sy = (float)Math.Sin(v.Z * 0.5);
            float cp = (float)Math.Cos(v.Y * 0.5);
            float sp = (float)Math.Sin(v.Y * 0.5);
            float cr = (float)Math.Cos(v.X * 0.5);
            float sr = (float)Math.Sin(v.X * 0.5);

            return new Quaternion
            {
                W = (cr * cp * cy + sr * sp * sy),
                X = (sr * cp * cy - cr * sp * sy),
                Y = (cr * sp * cy + sr * cp * sy),
                Z = (cr * cp * sy - sr * sp * cy)
            };

        }

    }
}
