using FlatSharp.Attributes;
using System.Numerics;

namespace SPICA.FlatBuffers.Common
{
    [FlatBufferStruct]
    public class Vector3
    {
        [FlatBufferItem(00)] public float X { get; set; } = 0.0f;
        [FlatBufferItem(01)] public float Y { get; set; } = 0.0f;
        [FlatBufferItem(02)] public float Z { get; set; } = 0.0f;

        public Vector3() { X = 0.0f; Y = 0.0f; Z = 0.0f; }

        public Vector3(float f = 0.0f) { X = f; Y=f; Z = f; }

        public Vector3(float X, float Y, float Z)
        {
            this.X = X; this.Y = Y; this.Z = Z;
        }

        public Vector3(System.Numerics.Vector3 v)
        {
            this.X = v.X; this.Y = v.Y; this.Z = v.Z;
        }
    }

    [FlatBufferStruct]
    public class Vector4
    {
        [FlatBufferItem(00)] public float W { get; set; } = 0.0f;
        [FlatBufferItem(01)] public float X { get; set; } = 0.0f;
        [FlatBufferItem(02)] public float Y { get; set; } = 0.0f;
        [FlatBufferItem(03)] public float Z { get; set; } = 0.0f;
    }

    [FlatBufferStruct]
    public class Sphere
    {
        [FlatBufferItem(0)] public float X { get; set; } = 0.0f;
        [FlatBufferItem(1)] public float Y { get; set; } = 0.0f;
        [FlatBufferItem(2)] public float Z { get; set; } = 0.0f;
        [FlatBufferItem(3)] public float Radius { get; set; } = 0.0f;
    }

    [FlatBufferTable]
    public class BoundingBox
    {
        [FlatBufferItem(00)] public Vector3 MinBound { get; set; }
        [FlatBufferItem(01)] public Vector3 MaxBound { get; set; }
    }

    [FlatBufferStruct]
    public class Quaternion
    {
        [FlatBufferItem(00)] public int W { get; set; }
        [FlatBufferItem(01)] public int X { get; set; }
        [FlatBufferItem(02)] public int Y { get; set; }
        [FlatBufferItem(03)] public int Z { get; set; }

    }

    [FlatBufferStruct]
    public class PackedQuaternion
    {
        [FlatBufferItem(0)] public ushort X { get; set; }
        [FlatBufferItem(1)] public ushort Y { get; set; }
        [FlatBufferItem(2)] public ushort Z { get; set; }
    }
}
