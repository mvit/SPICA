using SPICA.Compression;
using SPICA.Formats.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SPICA.Formats.GFLX
{
    public static class GFLXPackConverter
    {
        public enum GFCompressionType : UInt16
        {
            NONE = 0,
            ZLIB,
            LZ4,
            OODLE
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct GFPakHeader
        {
            public const int SIZE = 0x18;
            public string magic;
            public UInt32 Version;
            public UInt32 Relocated;
            public UInt32 FileNumber;
            public UInt32 FolderNumber;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct GFPakFolderHeader
        {
            public const int SIZE = 0x16;
            public UInt64 Hash;
            public UInt32 ContentNumber;
            public UInt32 Reserved;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct GFPakFolderIndex
        {
            public const int SIZE = 0x16;
            public UInt64 Hash;
            public UInt32 Index;
            public UInt32 Reserved;
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct GFPakFileHeader
        {
            public const int SIZE = 0x18;
            public UInt16 Level;
            public GFCompressionType CompressionType;
            public UInt32 BufferSize;
            public UInt32 FileSize;
            public UInt32 Reserved;
            public UInt64 FilePointer;
        }

        public static GFLXPack UnpackFrom(BinaryReader br)
        {
            GFPakHeader header = br.ReadBytes(GFPakHeader.SIZE).ToStruct<GFPakHeader>();

            Int64 embeddedFileOff = br.ReadInt64();
            Int64 embeddedFileHashOff = br.ReadInt64();

            List<Int64> folderOffsets = new List<Int64>();
            for (int i = 0; i < header.FolderNumber; i++)
            {
                Int64 folderOffset = br.ReadInt64();
                folderOffsets.Add(folderOffset);
            }

            //TODO: HASH MATCH
            List<UInt64> fileHashes = new List<UInt64>();

            for (int i = 0; i < header.FileNumber; i++)
            {
                UInt64 fileHash = br.ReadUInt64();
                fileHashes.Add(fileHash);
            }

            br.BaseStream.Position = embeddedFileHashOff;
            List<GFPakFileHeader> embeddedFiles = new List<GFPakFileHeader>();
            for (int i = 0; i < header.FileNumber; i++)
            {
                GFPakFileHeader file = br.ReadBytes(GFPakFileHeader.SIZE).ToStruct<GFPakFileHeader>();
                embeddedFiles.Add(file);
            }

            br.BaseStream.Position = embeddedFileOff;
            List<byte[]> files = new List<byte[]>();
            for (int i = 0; i < embeddedFiles.Count; i++)
            {
                GFPakFileHeader file = embeddedFiles[i];
                br.BaseStream.Position = (long)file.FilePointer;
                byte[] fileBytes = br.ReadBytes((int)file.FileSize);

                switch (file.CompressionType)
                {
                    case GFCompressionType.LZ4:
                        byte[] decompressed = LZ4.Decompress(fileBytes, (int)file.BufferSize);
                        files.Add(decompressed);
                        break;
                    case GFCompressionType.NONE:
                    default:
                        files.Add(fileBytes);
                        break;
                }
                files.Add(fileBytes);
            }

            br.BaseStream.Position = folderOffsets[0];
            List<GFLXFolder> folders = new List<GFLXFolder>();
            for (int i = 0; i < header.FolderNumber; i++)
            {
                br.BaseStream.Position = folderOffsets[i];
                GFPakFolderHeader tFolder = br.ReadBytes(GFPakFolderHeader.SIZE).ToStruct<GFPakFolderHeader>();
                //TODO: HASH MATCH
                GFLXFolder folder = new GFLXFolder() { name = tFolder.Hash.ToString() };
                for (int j = 0; j < tFolder.ContentNumber; j++)
                {
                    GFPakFolderIndex content = br.ReadBytes(GFPakFolderIndex.SIZE).ToStruct<GFPakFolderIndex>();
                    //TODO: HASH MATCH
                    GFLXFile file = new GFLXFile()
                    {
                        name = fileHashes[(int)content.Index].ToString(),
                        path = content.Hash.ToString(),
                        data = files[(int)content.Index],
                    };
                    folder.AddFile(file);
                }
                folders.Add(folder);
            }

            return new GFLXPack() { Folders = folders };

        }
        public static void PackTo(BinaryWriter bw, GFLXPack pack)
        {
            GFPakHeader header = new GFPakHeader()
            {
                magic = "GFLXPACK",
                Version = 0x1000,
                Relocated = 0,
                FolderNumber = (uint)pack.Folders.Count,
                FileNumber = (uint)pack.GetFileCount(),
            };

            bw.Write(header.ToBytes());

            //We'll need to come back here later
            long filePosPtr = bw.BaseStream.Position;
            //Offset to File Pointer
            bw.Write((UInt64)0);
            
            //Offset to Absolute Hashes
            long fileHashPtr = bw.BaseStream.Position;
            bw.Write((UInt64)0);

            //Offset to Folder Pointers
            long folderPosPtr = bw.BaseStream.Position;
            for (int i = 0; i < pack.Folders.Count; i++)
            {
                bw.Write((UInt64)0);
            }

            //Now we go back and add the fileHashPtr
            var pos = bw.BaseStream.Position;
            bw.BaseStream.Position = fileHashPtr;
            bw.Write(pos);
            bw.BaseStream.Position = pos;

            //Write the Absolute Path Hashes
            foreach (GFLXFolder folder in pack.Folders)
            {
                foreach (GFLXFile file in folder.files)
                {
                    bw.Write(GFFNV.CreateHash(file.path));
                }
            }

            //Write the Folder Info
            int fileCount = 0;
            for (int i = 0; i < pack.Folders.Count; i++)
            {
                GFLXFolder folder = pack.Folders[i];
                
                //We need to go back and add the folder position
                pos = bw.BaseStream.Position;
                bw.BaseStream.Position = folderPosPtr + GFPakFolderHeader.SIZE * i;
                bw.Write(pos);
                bw.BaseStream.Position = pos;
                
                bw.Write(
                    new GFPakFolderHeader()
                    {
                        Hash = GFFNV.CreateHash(folder.name),
                        ContentNumber = (uint)folder.files.Count,
                        Reserved = 0xCC,
                    }.ToBytes());
                foreach (GFLXFile file in folder.files)
                {
                    GFPakFolderIndex folderIndex = new GFPakFolderIndex()
                    {
                        //The relative hash name goes here
                        Hash = GFFNV.CreateHash(file.name),
                        Index = (uint)fileCount,
                        Reserved = 0xCC,
                    };
                    fileCount++;
                }
            }

            List<long> filePosPtrs = new List<long>();
            List<byte[]> fileData = new List<byte[]>();
            //Write the file info
            foreach(GFLXFolder folder in pack.Folders)
            {
                foreach(GFLXFile file in folder.files)
                {
                    bw.Write(
                        new GFPakFileHeader() { 
                            Level = 9, 
                            CompressionType = GFCompressionType.NONE, 
                            FileSize = (uint)file.data.Length, 
                            BufferSize = (uint)file.data.Length, 
                            Reserved = 0xCC,
                            FilePointer = 0, 
                        }.ToBytes());
                    fileData.Add(file.data);
                    filePosPtrs.Add(bw.BaseStream.Position - 0x8);
                }
            }

            //Go back and write the File Pointer position
            pos = bw.BaseStream.Position;
            bw.BaseStream.Position = filePosPtr;
            bw.Write(pos);
            bw.BaseStream.Position = pos;

            //Now we write the files
            for (int i = 0; i < fileCount; i++)
            {
                //First we update the file info
                pos = bw.BaseStream.Position;
                bw.BaseStream.Position = filePosPtrs[i];
                bw.Write(pos);
                bw.BaseStream.Position = pos;
                //Now we write the filedata
                bw.Write(fileData[i]);
            }

        }
    }
}
