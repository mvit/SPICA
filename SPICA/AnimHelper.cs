using SPICA.Formats.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPICA
{
    public class AnimHelper
    {
        private const string CachePath = "SPICACache.bin";
        private static Dictionary<uint, string> Cache = new Dictionary<uint, string>();

        public static void Open(string path = CachePath)
        {
            Cache = new Dictionary<uint, string>();

            if (File.Exists(path)) 
            {
                BinaryReader br = new BinaryReader(File.OpenRead(path));
                var count = br.ReadUInt64();
                for (ulong i = 0; i < count; i++)
                {
                    var hash = br.ReadUInt32();
                    string name = br.ReadString();

                    if (!Cache.ContainsKey(hash)) Cache.Add(hash, name);
                }
            }
        }

        public static string GetNameFromHash(uint hash)
        {
            if (!Cache.ContainsKey(hash)) return hash.ToString();
            return Cache[hash];
        }
    }
}
