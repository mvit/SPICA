using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatSharp.Attributes;
using SPICA.FlatBuffers.Common;

namespace SPICA.Flatbuffers.GFB
{
    [FlatBufferTable]
    public class GFBMDL
    {
        [FlatBufferItem(0)]
        public UInt32 Version { get; set; }
        [FlatBufferItem(1)]
        public BoundingBox BoundingBox { get; set; }
    }
}
