using SPICA.Flatbuffers.TR;
using SPICA.Flatbuffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPICA.FlatBuffers.TR;
using SPICA.Formats.CtrH3D;
using System.Numerics;
using SPICA.Math3D;

namespace SPICA.Formats.GFLX.TR
{
    public struct TRLXNode
    {
        public string name;
        public int parentID;
        public int boneID;
        public Vector3 scale;
        public Vector3 rotate;
        public Vector3 translate;
        public int type;
    }
    
    public struct TRLXBone
    {
        public Matrix3x4 inverse_transform;
    }

    public class TRLXSKL
    {
        public List<TRLXNode> nodes;
        public List<TRLXBone> bones;
        public int offset;

        public TRLXSKL(H3D Scene, int ModelIndex) {
            
            nodes = new List<TRLXNode>();
            bones = new List<TRLXBone>();

            CtrH3D.Model.H3DModel model = Scene.Models[ModelIndex];

            H3DDict<CtrH3D.Model.H3DBone> skeleton = model.Skeleton;

            foreach(CtrH3D.Model.H3DBone bone in skeleton)
            {
                if (bone.USUMFlags == 2)
                {
                    nodes.Add(new TRLXNode() { name = bone.Name, parentID = bone.ParentIndex, boneID = -1, rotate = bone.Rotation, scale = bone.Scale, translate = bone.Translation, type = 0 });
                }
                else
                {
                    nodes.Add(new TRLXNode() { name = bone.Name, parentID = bone.ParentIndex, boneID = bones.Count, rotate = bone.Rotation, scale = bone.Scale, translate = bone.Translation, type = 0 });
                    bones.Add(new TRLXBone() { inverse_transform = bone.InverseTransform });
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
            TRSKL fb = ToFlatbuffer();
            return FlatBufferConverter.SerializeFrom(fb);
        }

        public TRSKL ToFlatbuffer()
        {
            TRSKL skl = new TRSKL();

            skl.TransformNodes = new TransformNode[nodes.Count];

            for (int i = 0; i < nodes.Count; i++)
            {
                TRLXNode node = nodes[i];
                skl.TransformNodes[i] = new TransformNode()
                {
                    Name = node.name,
                    ParentIndex = node.parentID,
                    BoneIndex = node.boneID,
                    RotatePivot = new FlatBuffers.Common.Vector3(0.0f),
                    ScalePivot = new FlatBuffers.Common.Vector3(0.0f),
                    Transform = new TransformMatrix()
                    {
                        Scale = new FlatBuffers.Common.Vector3(node.scale),
                        Rotate = new FlatBuffers.Common.Vector3(node.rotate),
                        Translate = new FlatBuffers.Common.Vector3(node.translate),
                    },
                    NodeType = node.type,
                    LocatorNode = "",
                };
            }

            skl.BoneNodes = new BoneNode[bones.Count];

            for (int i = 0; i < bones.Count; i++)
            {
                TRLXBone bone = bones[i];
                skl.BoneNodes[i] = new BoneNode()
                {
                    Flag_00 = 1,
                    Flag_01 = 1,
                    BoneMatrix = new BoneMatrix()
                    {
                        X = new FlatBuffers.Common.Vector3(bone.inverse_transform.M11, bone.inverse_transform.M12, bone.inverse_transform.M13),
                        Y = new FlatBuffers.Common.Vector3(bone.inverse_transform.M21, bone.inverse_transform.M22, bone.inverse_transform.M23),
                        Z = new FlatBuffers.Common.Vector3(bone.inverse_transform.M31, bone.inverse_transform.M32, bone.inverse_transform.M33),
                        W = new FlatBuffers.Common.Vector3(bone.inverse_transform.M41, bone.inverse_transform.M42, bone.inverse_transform.M43),
                    }
                };
            }
            return skl;
        }
    }
}
