﻿using SPICA.Formats.Common;
using SPICA.Formats.CtrH3D.Animation;

using System;
using System.Collections.Generic;
using System.IO;

namespace SPICA.Formats.GFL.Motion
{
    class GFMotion
    {
        public ushort FramesCount;

        public readonly List<GFMotBoneTransform> Bones;

        public int Index;

        public GFMotion()
        {
            Bones = new List<GFMotBoneTransform>();
        }

        public GFMotion(BinaryReader Reader, List<GFMotBone> Skeleton, int Index) : this()
        {
            this.Index = Index;

            ushort OctalsCount = Reader.ReadUInt16();

            FramesCount = Reader.ReadUInt16();

            uint[] Octals = new uint[OctalsCount];

            int  KeyFramesCount = 0;
            uint CurrentOctal   = 0;

            for (int i = 0; i < OctalsCount; i++)
            {
                if ((i & 7) == 0) CurrentOctal = Reader.ReadUInt24();

                Octals[i] = CurrentOctal & 7;

                CurrentOctal >>= 3;

                if (Octals[i] > 5) KeyFramesCount++;
            }

            bool Frame16 = FramesCount > 0xff;

            if (Frame16 && (Reader.BaseStream.Position & 1) != 0) Reader.ReadByte();

            int[][] KeyFrames = new int[KeyFramesCount][];

            for (int i = 0; i < KeyFrames.Length; i++)
            {
                int Count;

                Count = Frame16
                    ? Reader.ReadUInt16()
                    : Reader.ReadByte();

                KeyFrames[i] = new int[Count + 2];

                KeyFrames[i][Count + 1] = FramesCount;

                for (int j = 1; j <= Count; j++)
                {
                    KeyFrames[i][j] = Frame16
                        ? Reader.ReadUInt16()
                        : Reader.ReadByte();
                }
            }

            GFUtils.Align(Reader);

            GFMotBoneTransform CurrentBone = null;

            int CurrentKFL =  0;
            int OctalIndex =  2;
            int ElemIndex  =  0;
            int OldIndex   = -1;

            while (OctalIndex < OctalsCount)
            {
                CurrentOctal = Octals[OctalIndex++];

                if (CurrentOctal != 1)
                {
                    int BoneIndex = ElemIndex / 9;

                    if (BoneIndex != OldIndex)
                    {
                        CurrentBone = new GFMotBoneTransform { Name = Skeleton[BoneIndex].Name };

                        Bones.Add(CurrentBone);

                        OldIndex = BoneIndex;
                    }
                }

                if (CurrentOctal != 1)
                {
                    //Actual Key Frame format
                    List<GFMotKeyFrame> KFs = null;

                    switch (ElemIndex % 9)
                    {
                        case 0: KFs = CurrentBone.TranslationX; break;
                        case 1: KFs = CurrentBone.TranslationY; break;
                        case 2: KFs = CurrentBone.TranslationZ; break;

                        case 3: KFs = CurrentBone.RotationX;    break;
                        case 4: KFs = CurrentBone.RotationY;    break;
                        case 5: KFs = CurrentBone.RotationZ;    break;

                        case 6: KFs = CurrentBone.ScaleX;       break;
                        case 7: KFs = CurrentBone.ScaleY;       break;
                        case 8: KFs = CurrentBone.ScaleZ;       break;
                    }

                    switch (CurrentOctal)
                    {
                        case 0: KFs.Add(new GFMotKeyFrame(0, 0)); break; //Constant Zero (0 deg)
                        case 2: KFs.Add(new GFMotKeyFrame(0, (float)Math.PI *  0.5f)); break; //Constant +Half PI (90 deg)
                        case 3: KFs.Add(new GFMotKeyFrame(0, (float)Math.PI *  1.0f)); break; //Constant +PI (180 deg)
                        case 4: KFs.Add(new GFMotKeyFrame(0, (float)Math.PI * -0.5f)); break; //Constant -Half PI (-90/270 deg)
                        case 5: KFs.Add(new GFMotKeyFrame(0, Reader.ReadSingle())); break; //Constant value (stored as Float)

                        case 6: //Linear Key Frames list
                            foreach (int Frame in KeyFrames[CurrentKFL++])
                            {
                                KFs.Add(new GFMotKeyFrame(Frame, Reader.ReadSingle()));
                            }
                            break;

                        case 7: //Hermite Key Frames list
                            foreach (int Frame in KeyFrames[CurrentKFL++])
                            {
                                KFs.Add(new GFMotKeyFrame(
                                    Frame,
                                    Reader.ReadSingle(),
                                    Reader.ReadSingle()));
                            }
                            break;
                    }

                    ElemIndex++;
                }
                else
                {
                    //Skip S/R/T Vector
                    ElemIndex += 3;
                }
            }
        }

        public H3DAnimation ToH3DSkeletalAnimation()
        {
            H3DAnimation Output = new H3DAnimation();

            Output.Name        = "GFMotion";
            Output.FramesCount = FramesCount;

            foreach (GFMotBoneTransform Bone in Bones)
            {
                H3DAnimTransform Transform = new H3DAnimTransform();

                SetKeyFrameGroup(Bone.TranslationX, Transform.TranslationX, 0);
                SetKeyFrameGroup(Bone.TranslationY, Transform.TranslationY, 1);
                SetKeyFrameGroup(Bone.TranslationZ, Transform.TranslationZ, 2);

                SetKeyFrameGroup(Bone.RotationX,    Transform.RotationX,    3);
                SetKeyFrameGroup(Bone.RotationY,    Transform.RotationY,    4);
                SetKeyFrameGroup(Bone.RotationZ,    Transform.RotationZ,    5);

                SetKeyFrameGroup(Bone.ScaleX,       Transform.ScaleX,       6);
                SetKeyFrameGroup(Bone.ScaleY,       Transform.ScaleY,       7);
                SetKeyFrameGroup(Bone.ScaleZ,       Transform.ScaleZ,       8);

                Output.Elements.Add(new H3DAnimationElement
                {
                    Name          = Bone.Name,
                    Content       = Transform,
                    TargetType    = H3DAnimTargetType.Bone,
                    PrimitiveType = H3DAnimPrimitiveType.Transform
                });
            }

            return Output;
        }

        //No idea why the Slope needs to be multiplied by this scale
        //This was discovered pretty much by trial and error, and may be wrong too
        private const float SlopeScale = 1 / 30f;

        private void SetKeyFrameGroup(List<GFMotKeyFrame> Source, H3DFloatKeyFrameGroup Target, int CurveIndex)
        {
            Target.Curve.StartFrame  = 0;
            Target.Curve.EndFrame    = FramesCount;
            Target.Curve.CurveIndex  = (ushort)CurveIndex;
            Target.InterpolationType = H3DInterpolationType.Hermite;

            foreach (GFMotKeyFrame KF in Source)
            {
                Target.KeyFrames.Add(new KeyFrame(
                    KF.Frame,
                    KF.Value,
                    KF.Slope * SlopeScale));
            }
        }
    }
}
