using FlatSharp;
using FlatSharp.Attributes;
using System;
using System.Collections.Generic;

namespace SPICA.Flatbuffers.TR
{
    [FlatBufferStruct]
    public class TRTransform
    {
        [FlatBufferItem(0)]
        public TRVector3 Scale { get; set; }
        [FlatBufferItem(1)]
        public TRVector4 Rotate { get; set; }
        [FlatBufferItem(2)]
        public TRVector3 Translate { get; set; }
    }

    [FlatBufferStruct]
    public class TRPackedQuaternion
    {
        [FlatBufferItem(0)]
        public UInt16 X { get; set; }
        [FlatBufferItem(1)]
        public UInt16 Y { get; set; }
        [FlatBufferItem(2)]
        public UInt16 Z { get; set; }
    }

    [FlatBufferStruct]
    public class TRVector3
    {
        [FlatBufferItem(0)]
        public float X { get; set; }
        [FlatBufferItem(1)]
        public float Y { get; set; }
        [FlatBufferItem(2)]
        public float Z { get; set; }
    }

    [FlatBufferStruct]
    public class TRVector4
    {
        [FlatBufferItem(0)]
        public float X { get; set; }
        [FlatBufferItem(1)]
        public float Y { get; set; }
        [FlatBufferItem(2)]
        public float Z { get; set; }
        [FlatBufferItem(3)]
        public float W { get; set; }
    }

    [FlatBufferTable]
    public class TRANMFixedVectorTrack
    {
        [FlatBufferItem(0)]
        public TRVector3 Value { get; set; }
    }

    [FlatBufferTable]
    public class TRANMFramedVectorTrack
    {
        [FlatBufferItem(0)]
        public IList<TRVector3> Values { get; set; }
    }

    [FlatBufferTable]
    public class TRANMKeyed16VectorTrack
    {
        [FlatBufferItem(0)]
        public IList<UInt16> Keys { get; set; }
        [FlatBufferItem(1)]
        public IList<TRVector3> Values { get; set; }
    }

    [FlatBufferTable]
    public class TRANMKeyed8VectorTrack
    {
        [FlatBufferItem(0)]
        public IList<Byte> Keys { get; set; }
        [FlatBufferItem(1)]
        public IList<TRVector3> Values { get; set; }
    }
    [FlatBufferTable]
    public class TRANMFixedRotationTrack
    {
        [FlatBufferItem(0)]
        public TRPackedQuaternion Value { get; set; }
    }

    [FlatBufferTable]
    public class TRANMFramedRotationTrack
    {
        [FlatBufferItem(0)]
        public IList<TRPackedQuaternion> Values { get; set; }
    }

    [FlatBufferTable]
    public class TRANMKeyed16RotationTrack
    {
        [FlatBufferItem(0)]
        public IList<UInt16> Keys { get; set; }
        [FlatBufferItem(1)]
        public IList<TRPackedQuaternion> Values { get; set; }
    }

    [FlatBufferTable]
    public class TRANMKeyed8RotationTrack
    {
        [FlatBufferItem(0)]
        public IList<Byte> Keys { get; set; }
        [FlatBufferItem(1)]
        public IList<TRPackedQuaternion> Values { get; set; }
    }
    [FlatBufferTable]
    public class TRANMSkeletalTrack
    {
        [FlatBufferItem(0)]
        public String BoneName { get; set; }
        [FlatBufferItem(1)]
        public FlatBufferUnion<TRANMFixedVectorTrack, TRANMFramedVectorTrack, TRANMKeyed16VectorTrack, TRANMKeyed8VectorTrack> ScaleChannel {get; set;}
        [FlatBufferItem(3)]
        public FlatBufferUnion<TRANMFixedRotationTrack, TRANMFramedRotationTrack, TRANMKeyed16RotationTrack, TRANMKeyed8RotationTrack> RotationChannel { get; set; }
        [FlatBufferItem(5)]
        public FlatBufferUnion<TRANMFixedVectorTrack, TRANMFramedVectorTrack, TRANMKeyed16VectorTrack, TRANMKeyed8VectorTrack> TranslateChannel { get; set; }

    }

    [FlatBufferTable]
    public class TRANMInfo
    {
        [FlatBufferItem(0)]
        public UInt32 IsLooped { get; set; }
        [FlatBufferItem(1)]
        public UInt32 FrameCount { get; set; }
        [FlatBufferItem(2)]
        public UInt32 FrameRate { get; set; }
    }

    [FlatBufferTable]
    public class TRANMBoneInit
    {
        [FlatBufferItem(0)]
        public UInt32 IsInit { get; set; }
        [FlatBufferItem(1)]
        public TRTransform BoneTransform { get; set; }
    }

    [FlatBufferTable]
    public class TRANMSkeletalAnimation
    {
        [FlatBufferItem(0)]
        public IList<TRANMSkeletalTrack> Tracks { get; set; }
        [FlatBufferItem(1)]
        public TRANMBoneInit Init { get; set; }
    }

    [FlatBufferTable]
    public class TRANM
    {
        [FlatBufferItem(0)]
        public TRANMInfo Info { get; set; }
        [FlatBufferItem(1)]
        public TRANMSkeletalAnimation SkeletalAnimation { get; set; }
    }
}
