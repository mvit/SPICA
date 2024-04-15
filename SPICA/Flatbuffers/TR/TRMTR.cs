using FlatSharp.Attributes;

namespace SPICA.Flatbuffers.TR
{
    [FlatBufferTable]
    public class TRMTR
    {
        [FlatBufferItem(00)] public string Name { get; set; }

    }
    [FlatBufferTable]
    public class MaterialList
    {
        [FlatBufferItem(00)] public int Field_00 { get; set; }
        [FlatBufferItem(01)] public TRMTR[] Materials { get; set; }
    }
}