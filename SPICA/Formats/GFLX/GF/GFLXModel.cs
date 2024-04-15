using SPICA.Flatbuffers;
using SPICA.Flatbuffers.GFB;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.CtrH3D.Model.Material;
using SPICA.Formats.CtrH3D.Model.Mesh;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPICA.Formats.GFLX
{
    public struct GFLXBone
    {
        public GFLXBone(H3DBone bone)
        {
        }
    }

    public struct GFLXMesh
    {
        public GFLXMesh(H3DMesh mesh)
        {
            
        }
    }

    public class GFLXShape
    {

    }

    public struct GFLXBoundingBox
    {

    }

    public struct GFLXMaterial
    {
        public string Name;
        public string ShaderName;
        public int SortPriority;
        public bool DepthWrite;
        public bool DepthTest;
        public int LightSetNum;
        public int BlendMode;
        public int CullMode;
        public int VertexShaderIndex;
        public int GeometryShaderIndex;
        public int FragmentShaderIndex;
    }

    public class GFLXModel
    {
        public List<String> textureFiles = new List<String>();
        public List<String> vertexShaders = new List<String>();
        public List<String> geometryShaders = new List<String>();
        public List<String> fragmentShaders = new List<String>();
        public List<GFLXMaterial> materials = new List<GFLXMaterial>();
        public List<GFLXMesh> meshes = new List<GFLXMesh>();
        public List<GFLXBone> skeleton = new List<GFLXBone>();
        public GFLXModel()
        {

        }

        public GFLXModel(H3D scene, int modelIndex)
        {
            H3DModel model = scene.Models[modelIndex];
            
            foreach(H3DMaterial material in model.Materials)
            {
                textureFiles.Add(material.Texture0Name);
                textureFiles.Add(material.Texture1Name);
                textureFiles.Add(material.Texture2Name);
                vertexShaders.Add(material.Name);
                fragmentShaders.Add(material.Name);

                
            }

            foreach(H3DMesh mesh in model.Meshes)
            {
                meshes.Add(new GFLXMesh(mesh));
            }
            
            foreach(H3DBone bone in model.Skeleton)
            {
                skeleton.Add(new GFLXBone(bone));
            }

        }

        public GFLXModel(GFBMDL flatbuffer)
        {

        }

        public void Save(string fileName)
        {
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(fileName));
            bw.Write(Serialize());
            bw.Close();
        }

        public byte[] Serialize()
        {
            return FlatBufferConverter.SerializeFrom(ToFlatbuffer());
        }

        public GFBMDL ToFlatbuffer()
        {
            var fb = new GFBMDL();
            return fb;
        }
    }
}
