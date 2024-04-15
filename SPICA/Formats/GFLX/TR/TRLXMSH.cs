using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.CtrH3D.Model.Mesh;
using SPICA.PICA.Commands;
using SPICA.PICA.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPICA.Formats.GFLX.TR
{
    public struct TRLXMaterial
    {
        public uint count;
        public uint offset;
        public string name;
    }

    public enum TRLXAttribute
    {
        NONE = 0,
        POSITION,
        NORMAL,
        TANGENT,
        BINORMAL,
        COLOR,
        TEX_COORD,
        BLEND_INDICES,
        BLEND_WEIGHTS,
    }

    public enum TRLXVertexType
    {
        NONE = 0,
        R8_G8_B8_A8_UNSIGNED_NORMALIZED = 20,
        W8_X8_Y8_Z8_UNSIGNED = 22,
        W16_X16_Y16_Z16_SIGNED_NORMALIZED = 39,
        W16_X16_Y16_Z16_FLOAT = 43,
        X32_Y32_FLOAT = 48,
        X32_Y32_Z32_FLOAT = 51,
        W32_X32_Y32_Z32_FLOAT = 54,
    }

    public struct TRLXVertexAccesor
    {
        TRLXAttribute attribute;
        TRLXVertexType type;
        uint size;
    }

    public struct TRLXAccessor
    {

    }

    public class TRLXShape
    {
        public byte[] buffer;
        public uint[] indices;
        public List<TRLXMaterial> materials;
    }
    public class TRLXMSH
    {

        public TRLXMSH(H3DMesh mesh)
        {
            PICAVertex[] vertices = mesh.GetVertices();

            for (int SMIndex = 0; SMIndex < mesh.SubMeshes.Count; SMIndex++)
            {
                TRLXShape shape = new TRLXShape();
                H3DSubMesh submesh = mesh.SubMeshes[SMIndex];
                string shapeName = "dummy_mesh_shape";
                string meshName = "dummy_mesh";

                shape.indices = Array.ConvertAll(submesh.Indices, val => (uint)val);
                PICAVertex[] submeshVerts = vertices.Skip(submesh.Indices.Min()).Take(submesh.Indices.Max() - submesh.Indices.Min()).ToArray();
                
                foreach(PICAAttribute attribute in mesh.Attributes)
                {

                }
            }

        }
    }
}
