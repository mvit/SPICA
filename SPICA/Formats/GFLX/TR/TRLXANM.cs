using SPICA.Formats.Common;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Animation;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Flatbuffers;
using SPICA.Flatbuffers.TR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using FlatSharp;

namespace SPICA.Formats.GFLX
{
    public class TRLXANMTrack<T>
    {
        public List<UInt16> times = new List<UInt16>();
        public List<T> values = new List<T>();
    }

    public class TRLXANMNode
    {
        public string name;
        public TRLXANMTrack<Vector3> scale = new TRLXANMTrack<Vector3>();
        public TRLXANMTrack<Quaternion> rotation = new TRLXANMTrack<Quaternion>();
        public TRLXANMTrack<Vector3> translation = new TRLXANMTrack<Vector3>();
        public Vector3 initScale;
        public Quaternion initRotation;
        public Vector3 initTranslation;
    }

    public class TRLXANM
    {
        public int FrameCount { get; set; }
        public int FrameRate { get; set; }
        public int IsLooped { get; set; }
        public List<TRLXANMNode> Channels { get; set; } = new List<TRLXANMNode>();

        public TRLXANM(H3D Scene, int ModelIndex, int AnimIndex = -1)
        {
            FrameRate = 30;

            H3DAnimation Animation = Scene.SkeletalAnimations[AnimIndex];
            H3DDict<H3DBone> Skeleton = Scene.Models[ModelIndex].Skeleton;

            FrameCount = (int)Animation.FramesCount;
            IsLooped = (int)(Animation.AnimationFlags & H3DAnimationFlags.IsLooping);

            foreach (H3DBone bone in Skeleton)
            {
                Channels.Add(new TRLXANMNode()
                {
                    name = bone.Name,
                    initRotation = Quaternion.CreateFromYawPitchRoll(bone.Rotation.Y, bone.Rotation.X, bone.Rotation.Z),
                    initTranslation = bone.Translation,
                    initScale = bone.Scale,
                });
            }

            foreach (H3DAnimationElement element in Animation.Elements)
            {
                TRLXANMNode node = Channels.Find(x => x.name == element.Name);
                if (element.Content is H3DAnimQuatTransform quatTransform)
                {
                    for (int frame = 0; frame < Animation.FramesCount; frame++)
                    {
                        if (quatTransform.HasTranslation)
                        {
                            if (node.translation.values.Count() == 0 || quatTransform.GetTranslationValue(frame) != node.translation.values.Last())
                            {
                                node.translation.times.Add((ushort)frame);
                                node.translation.values.Add(quatTransform.GetTranslationValue(frame));
                            }
                        }
                        if (quatTransform.HasScale)
                        {
                            if (node.scale.values.Count() == 0 || quatTransform.GetScaleValue(frame) != node.scale.values.Last())
                            {
                                node.scale.times.Add((ushort)frame);
                                node.scale.values.Add(quatTransform.GetScaleValue(frame));
                            }
                        }
                        if (quatTransform.HasRotation)
                        {
                            if (node.rotation.values.Count() == 0 || quatTransform.GetRotationValue(frame) != node.rotation.values.Last())
                            {
                                node.rotation.times.Add((ushort)frame);
                                node.rotation.values.Add(quatTransform.GetRotationValue(frame));
                            }
                        }
                    }
                }
                else if (element.Content is H3DAnimTransform transform)
                {
                    for (int frame = 0; frame < Animation.FramesCount; frame++)
                    {
                        if (transform.TranslationExists)
                        {
                            node.translation.times.Add((ushort)frame);
                            node.translation.values.Add(new Vector3(
                                transform.TranslationX.GetFrameValue(frame),
                                transform.TranslationY.GetFrameValue(frame),
                                transform.TranslationZ.GetFrameValue(frame)));
                        }
                        if (transform.ScaleExists)
                        {
                            node.scale.times.Add((ushort)frame);
                            node.scale.values.Add(new Vector3(
                                transform.ScaleX.GetFrameValue(frame),
                                transform.ScaleY.GetFrameValue(frame),
                                transform.ScaleZ.GetFrameValue(frame)));
                        }
                        if (transform.RotationExists)
                        {
                            node.rotation.times.Add((ushort)frame);
                            node.rotation.values.Add(Quaternion.CreateFromRotationMatrix(
                                Matrix4x4.CreateRotationX(transform.RotationX.GetFrameValue(frame)) * 
                                Matrix4x4.CreateRotationY(transform.RotationY.GetFrameValue(frame)) * 
                                Matrix4x4.CreateRotationZ(transform.RotationZ.GetFrameValue(frame))
                            ));
                        }
                    }
                }
            }

        }

        public void Save(string fileName)
        {
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(fileName));
            bw.Write(Serialize());
            bw.Close();
        }

        public byte[] Serialize()
        {
            TRANM fb = ToFlatbuffer();
            return FlatBufferConverter.SerializeFrom(fb);
        }

        public TRANM ToFlatbuffer()
        {
            TRANM fb = new TRANM();

            fb.Info = new TRANMInfo()
            {
                FrameCount = (uint)FrameCount,
                FrameRate = (uint)FrameRate,
                IsLooped = (uint)IsLooped,
            };

            TRANMSkeletalAnimation animation = new TRANMSkeletalAnimation();
            animation.Tracks = new List<TRANMSkeletalTrack>();
            foreach (TRLXANMNode node in Channels)
            {
                TRANMSkeletalTrack track = new TRANMSkeletalTrack()
                {
                    BoneName = node.name,
                };
                //SCALE
                if (node.scale.values.Count == 0)
                {
                    TRANMFixedVectorTrack ScaleTrack = new TRANMFixedVectorTrack();
                    Vector3 scale = node.initScale;
                    ScaleTrack.Value = new TRVector3 { X = scale.X, Y = scale.Y, Z = scale.Z };
                    track.ScaleChannel = new FlatBufferUnion<TRANMFixedVectorTrack, TRANMFramedVectorTrack, TRANMKeyed16VectorTrack, TRANMKeyed8VectorTrack>(ScaleTrack);
                }
                else if (node.scale.values.Count == 1)
                {
                    TRANMFixedVectorTrack ScaleTrack = new TRANMFixedVectorTrack();
                    Vector3 scale = node.scale.values[0];
                    ScaleTrack.Value = new TRVector3 { X = scale.X, Y = scale.Y, Z = scale.Z };
                    track.ScaleChannel = new FlatBufferUnion<TRANMFixedVectorTrack, TRANMFramedVectorTrack, TRANMKeyed16VectorTrack, TRANMKeyed8VectorTrack>(ScaleTrack);
                }
                else if (node.scale.times.Count == FrameCount)
                {
                    TRANMFramedVectorTrack ScaleTrack = new TRANMFramedVectorTrack()
                    {
                        Values = new List<TRVector3>()
                    };

                    foreach (Vector3 scale in node.scale.values)
                    {
                        ScaleTrack.Values.Add(new TRVector3 { X = scale.X, Y = scale.Y, Z = scale.Z });
                    }
                    track.ScaleChannel = new FlatBufferUnion<TRANMFixedVectorTrack, TRANMFramedVectorTrack, TRANMKeyed16VectorTrack, TRANMKeyed8VectorTrack>(ScaleTrack);
                }
                else
                {
                    TRANMKeyed16VectorTrack ScaleTrack = new TRANMKeyed16VectorTrack()
                    {
                        Keys = new List<UInt16>(),
                        Values = new List<TRVector3>()
                    };

                    for (int i = 0; i < node.scale.times.Count; i++)
                    {
                        Vector3 scale = node.scale.values[i];
                        UInt16 time = node.scale.times[i];
                        ScaleTrack.Values.Add(new TRVector3 { X = scale.X, Y = scale.Y, Z = scale.Z });
                        ScaleTrack.Keys.Add(time);
                    }
                    track.ScaleChannel = new FlatBufferUnion<TRANMFixedVectorTrack, TRANMFramedVectorTrack, TRANMKeyed16VectorTrack, TRANMKeyed8VectorTrack>(ScaleTrack);
                }
                //TRANSLATION
                //ALSO, WHEN DOING GFLXANM, MAKE SURE YOU'RE NOT SCALING THIS
                if (node.translation.values.Count == 0)
                {
                    TRANMFixedVectorTrack TranslationTrack = new TRANMFixedVectorTrack();
                    Vector3 translate = node.initTranslation; //SCALING
                    TranslationTrack.Value = new TRVector3 { X = translate.X, Y = translate.Y, Z = translate.Z };
                    track.TranslateChannel = new FlatBufferUnion<TRANMFixedVectorTrack, TRANMFramedVectorTrack, TRANMKeyed16VectorTrack, TRANMKeyed8VectorTrack>(TranslationTrack);
                }

                else if (node.translation.values.Count == 1)
                {
                    TRANMFixedVectorTrack TranslationTrack = new TRANMFixedVectorTrack();
                    Vector3 translate = node.translation.values[0]; //SCALING;
                    TranslationTrack.Value = new TRVector3 { X = translate.X, Y = translate.Y, Z = translate.Z };
                    track.TranslateChannel = new FlatBufferUnion<TRANMFixedVectorTrack, TRANMFramedVectorTrack, TRANMKeyed16VectorTrack, TRANMKeyed8VectorTrack>(TranslationTrack);
                }

                else if (node.translation.times.Count == FrameCount)
                {
                    TRANMFramedVectorTrack TranslationTrack = new TRANMFramedVectorTrack()
                    {
                        Values = new List<TRVector3>()
                    };

                    foreach (Vector3 translate in node.translation.values)
                    {
                        var scaled_translate = translate;
                        TranslationTrack.Values.Add(new TRVector3 { X = scaled_translate.X, Y = scaled_translate.Y, Z = scaled_translate.Z });
                    }
                    track.TranslateChannel = new FlatBufferUnion<TRANMFixedVectorTrack, TRANMFramedVectorTrack, TRANMKeyed16VectorTrack, TRANMKeyed8VectorTrack>(TranslationTrack);
                }
                else
                {
                    TRANMKeyed16VectorTrack TranslationTrack = new TRANMKeyed16VectorTrack()
                    {
                        Keys = new List<UInt16>(),
                        Values = new List<TRVector3>()
                    };

                    for (int i = 0; i < node.translation.times.Count; i++)
                    {
                        Vector3 translate = node.translation.values[i];
                        UInt16 time = node.translation.times[i];
                        TranslationTrack.Values.Add(new TRVector3 { X = translate.X, Y = translate.Y, Z = translate.Z });
                        TranslationTrack.Keys.Add(time);
                    }
                    track.TranslateChannel = new FlatBufferUnion<TRANMFixedVectorTrack, TRANMFramedVectorTrack, TRANMKeyed16VectorTrack, TRANMKeyed8VectorTrack>(TranslationTrack);
                }
                //ROTATION
                if (node.rotation.values.Count == 0)
                {
                    var RotationTrack = new TRANMFixedRotationTrack();
                    Quaternion rotation = node.initRotation;
                    GFPackedQuaternion pack = GFQuaternionPacker.PackQuaternion(rotation);
                    RotationTrack.Value = new TRPackedQuaternion { X = pack.x, Y = pack.y, Z = pack.z };
                    track.RotationChannel = new FlatBufferUnion<TRANMFixedRotationTrack, TRANMFramedRotationTrack, TRANMKeyed16RotationTrack, TRANMKeyed8RotationTrack>(RotationTrack);
                }
                else if (node.rotation.values.Count == 1)
                {
                    var RotationTrack = new TRANMFixedRotationTrack();
                    Quaternion rotation = node.rotation.values[0];
                    GFPackedQuaternion pack = GFQuaternionPacker.PackQuaternion(rotation);
                    RotationTrack.Value = new TRPackedQuaternion { X = pack.x, Y = pack.y, Z = pack.z };
                    track.RotationChannel = new FlatBufferUnion<TRANMFixedRotationTrack, TRANMFramedRotationTrack, TRANMKeyed16RotationTrack, TRANMKeyed8RotationTrack>(RotationTrack);
                }
                else if (node.rotation.times.Count == FrameCount)
                {
                    var RotationTrack = new TRANMFramedRotationTrack()
                    {
                        Values = new List<TRPackedQuaternion>()
                    }; 
                    foreach (Quaternion rotation in node.rotation.values)
                    {
                        GFPackedQuaternion pack = GFQuaternionPacker.PackQuaternion(rotation);
                        RotationTrack.Values.Add(new TRPackedQuaternion { X = pack.x, Y = pack.y, Z = pack.z });
                    }
                    track.RotationChannel = new FlatBufferUnion<TRANMFixedRotationTrack, TRANMFramedRotationTrack, TRANMKeyed16RotationTrack, TRANMKeyed8RotationTrack>(RotationTrack);
                }
                else
                {
                    var RotationTrack = new TRANMKeyed16RotationTrack()
                    {
                        Keys = new List<UInt16>(),
                        Values = new List<TRPackedQuaternion>()
                    };
                    for (int i = 0; i < node.rotation.values.Count; i++)
                    {
                        Quaternion rotation = node.rotation.values[i];
                        UInt16 time = node.rotation.times[i];
                        GFPackedQuaternion pack = GFQuaternionPacker.PackQuaternion(rotation);
                        RotationTrack.Keys.Add(time);
                        RotationTrack.Values.Add(new TRPackedQuaternion { X = pack.x, Y = pack.y, Z = pack.z });
                    }
                    track.RotationChannel = new FlatBufferUnion<TRANMFixedRotationTrack, TRANMFramedRotationTrack, TRANMKeyed16RotationTrack, TRANMKeyed8RotationTrack>(RotationTrack);
                }

                animation.Tracks.Add(track);

            }
            fb.SkeletalAnimation = animation;
            return fb;
        }
    }
}
