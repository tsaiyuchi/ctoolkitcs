using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CToolkitCs.v1_3.Compress
{
    public class CtkCompressUtil
    {

        public static void ExtractEntries(ZipArchive archive, string zipPath, string destPath)
        {

            var query = from row in archive.Entries
                        where row.FullName.StartsWith(zipPath)
                        select row;

            foreach (var entry in query)
            {
                if (string.IsNullOrEmpty(entry.Name)) continue;
                var path = entry.FullName.Replace(zipPath, "");
                path = Path.Combine(destPath, path);
                var fi = new FileInfo(path);
                if (!fi.Directory.Exists) fi.Directory.Create();
                entry.ExtractToFile(fi.FullName, true);
            }
        }

        public static void ExtractEntry(ZipArchive archive, string zipFile, string destFolder)
        {

            var query = from row in archive.Entries
                        where row.FullName.StartsWith(zipFile)
                        select row;

            var entry = query.FirstOrDefault();
            if (entry == null) return;
            if (string.IsNullOrEmpty(entry.Name)) return;

            var path = Path.Combine(destFolder, entry.Name);
            var fi = new FileInfo(path);
            if (!fi.Directory.Exists) fi.Directory.Create();
            entry.ExtractToFile(fi.FullName, true);


        }

        /// <summary> 將字串(e.q. Json)以UTF8編碼後，用GZip壓縮成byte[]，用於存入Db Blob欄位 </summary>
        public static byte[] GZipCompressString(string text, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var rawBytes = encoding.GetBytes(text);

            using (var memStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memStream, CompressionMode.Compress, true))
                {
                    gzipStream.Write(rawBytes, 0, rawBytes.Length);
                }
                return memStream.ToArray();
            }
        }

        /// <summary> 將GZip壓縮過的byte[]解壓回原始字串(e.q. Json) </summary>
        public static string GZipDecompressString(byte[] gzipBytes, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;

            using (var memStream = new MemoryStream(gzipBytes))
            using (var gzipStream = new GZipStream(memStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(gzipStream, encoding))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
