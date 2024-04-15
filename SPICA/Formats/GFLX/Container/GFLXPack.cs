using SPICA.Compression;
using SPICA.Formats.Common;
using SPICA.Formats.CtrH3D;
using SPICA.Formats.CtrH3D.Model;
using SPICA.Formats.CtrH3D.Model.Material;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace SPICA.Formats.GFLX
{
    public class GFLXFile
    {
        public string name;
        public string path;
        public byte[] data { get; set; }
    }
    public class GFLXFolder
    {
        public string name;
        public List<GFLXFile> files;

        public void AddFile(GFLXFile file)
        {
            files.Add(file);
        }

        public void AddFile(string name, byte[] data)
        {
            files.Add(new GFLXFile
            {
                name = name,
                path = Path.Combine(this.name, name),
                data = data
            });
        }
    }

    public class GFLXPack
    {
        public const string ROMFSPATH = "bin/pokemon/";
        public List<GFLXFolder> Folders { get; set; } = new List<GFLXFolder>();

        public GFLXPack()
        {

        }

        public GFLXPack(H3D Scene)
        {
            GFLXFolder modelFolder = new GFLXFolder() { name = "mdl" };
            //First add the models
            for (int i = 0; i < Scene.Models.Count; i++)
            {
                //Add Model
                modelFolder.AddFile(
                    new GFLXFile() {
                        name = Scene.Models[i].Name,
                        path = Path.Combine(ROMFSPATH, modelFolder.name, Scene.Models[i].Name),
                        data = new GFLXModel(Scene, i).Serialize(),
                    });
                //Add BNSHs and Textures
                foreach(H3DMaterial material in Scene.Models[i].Materials)
                {
                    modelFolder.AddFile(
                        new GFLXFile()
                    {
                        name = material.Name + ".bnsh_vsh",
                        path = Path.Combine(ROMFSPATH, modelFolder.name, Scene.Models[i].Name)
                    });

                }
            }
            GFLXFolder animFolder = new GFLXFolder() { name = "anm" };
            //Then add the animations
            for (int i = 0; i < Scene.SkeletalAnimations.Count; i++)
            {
                animFolder.AddFile(
                    new GFLXFile()
                    {
                        name = Scene.SkeletalAnimations[i].Name,
                        path = Path.Combine(ROMFSPATH, animFolder.name, Scene.SkeletalAnimations[i].Name),
                        data = new TRLXANM(Scene, i).Serialize()
                    });
                    
            }
        }

        public int GetFileCount()
        {
            int count = 0;
            foreach (GFLXFolder folder in Folders)
            {
                count += folder.files.Count;
            }
            return count;
        }

        public void Save(string fileName)
        {
            BinaryWriter bw = new BinaryWriter(File.OpenWrite(fileName));
            GFLXPackConverter.PackTo(bw, this);
        }

    }

    public class GFLXPackOld
    {
        public string Magic;
        public UInt32 FileCnt;
        UInt64 InfoOff;

        List<byte[]> Files;
        List<string> Names;

        public GFLXPackOld(BinaryReader br) {
            //
        }

        public GFLXPackOld(string path)
        {
            Files = new List<byte[]>();
            Names = new List<string>();
            using (BinaryReader br = new BinaryReader(File.Open(path, FileMode.Open)))
            {
                //Read container header
                Magic = br.ReadChars(8).ToString();
                br.ReadUInt64();
                FileCnt = br.ReadUInt32();
                br.ReadUInt32();
                InfoOff = br.ReadUInt64();
                br.ReadUInt64();
                br.ReadUInt64();
                br.ReadUInt64();

                for (int i = 0; i < FileCnt; i++)
                {
                    br.BaseStream.Position = (long)InfoOff + (i * 0x18);
                    //Read file header
                    br.ReadUInt32();
                    UInt32 size = br.ReadUInt32();
                    UInt32 zsize = br.ReadUInt32();
                    br.ReadUInt32(); //dummy
                    UInt64 offset = br.ReadUInt64();
                    br.BaseStream.Position = (long)offset;
                    byte[] compData = br.ReadBytes((int)zsize);
                    byte[] decompData = LZ4.Decompress(compData, (int)size);
                    string ext = string.Empty;
                    switch (BitConverter.ToUInt32(decompData, 0))
                    {
                        case 0x58544E42:
                            ext = ".btnx";
                            break;
                        case 0x48534E42:
                            ext = ".bnsh";
                            break;
                        case 0x20:
                            ext = ".gfbmdl";
                            break;
                        default:
                            ext = ".bin";
                            break;
                    }
                    Names.Add(offset.ToString("X8") + ext);
                    Files.Add(decompData);
                }
            }
        }

        ~GFLXPackOld()
        {
            //
        }

        public void Save(string filename)
        {

        }

        public byte[] GetFile(int ind)
        {
            if (ind >= FileCnt) return null;
            return Files[ind];
        }

        public string GetName(int ind) {
            if (ind >= FileCnt) return string.Empty;
            return Names[ind];
        }

        public H3D ToH3D()
        {
            H3D h3d = new H3D();
            return h3d;
        }
    }
}
