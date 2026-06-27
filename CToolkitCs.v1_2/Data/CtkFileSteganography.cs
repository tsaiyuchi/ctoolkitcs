using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CToolkitCs.v1_2.Data
{

    public class CtkFileSteganography
    {
        private const int BUFFER_SIZE = 80 * 1024; // 80KB buffer
        private static readonly byte[] MAGIC_MARKER = Encoding.UTF8.GetBytes("<<Dataset Information>>");

        /// <summary>
        /// 隱藏檔案到圖片中 (使用 Stream,自動加入標記)
        /// </summary>
        /// <param name="imageFile">原始圖片路徑</param>
        /// <param name="hiddenFile">要隱藏的檔案路徑</param>
        /// <param name="outputFile">輸出的圖片路徑</param>
        public static void HideFile(string imageFile, string hiddenFile, string outputFile)
        {
            long hiddenSize = new FileInfo(hiddenFile).Length;

            using (FileStream outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE))
            {
                // 1. 先複製圖片
                using (FileStream imageStream = new FileStream(imageFile, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE))
                {
                    imageStream.CopyTo(outputStream, BUFFER_SIZE);
                }

                // 2. 寫入魔術標記
                outputStream.Write(MAGIC_MARKER, 0, MAGIC_MARKER.Length);

                // 3. 寫入隱藏檔案大小 (8 bytes - long)
                byte[] sizeBytes = BitConverter.GetBytes(hiddenSize);
                outputStream.Write(sizeBytes, 0, sizeBytes.Length);

                // 4. 附加隱藏檔案內容
                using (FileStream hiddenStream = new FileStream(hiddenFile, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE))
                {
                    hiddenStream.CopyTo(outputStream, BUFFER_SIZE);
                }
            }

            Console.WriteLine($"✓ 檔案已隱藏: {Path.GetFileName(hiddenFile)} ({FormatFileSize(hiddenSize)})");
        }

        /// <summary>
        /// 從圖片中提取隱藏的檔案 (自動偵測,不需要知道原始圖片大小)
        /// </summary>
        /// <param name="stegoImage">包含隱藏檔案的圖片路徑</param>
        /// <param name="outputFile">提取出的檔案路徑</param>
        public static bool ExtractFile(string stegoImage, string outputFile)
        {
            using (FileStream stegoStream = new FileStream(stegoImage, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE))
            {
                // 尋找魔術標記的位置
                long markerPosition = FindMagicMarker(stegoStream);

                if (markerPosition == -1)
                {
                    Console.WriteLine("✗ 沒有找到隱藏的檔案");
                    return false;
                }

                // 移動到標記後面,讀取檔案大小
                stegoStream.Seek(markerPosition + MAGIC_MARKER.Length, SeekOrigin.Begin);

                byte[] sizeBytes = new byte[8];
                stegoStream.Read(sizeBytes, 0, 8);
                long hiddenFileSize = BitConverter.ToInt64(sizeBytes, 0);

                if (hiddenFileSize <= 0 || hiddenFileSize > stegoStream.Length)
                {
                    Console.WriteLine("✗ 檔案大小資訊異常");
                    return false;
                }

                // 開始提取隱藏檔案 (已經在正確位置)
                using (FileStream outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE))
                {
                    byte[] buffer = new byte[BUFFER_SIZE];
                    long remaining = hiddenFileSize;

                    while (remaining > 0)
                    {
                        int toRead = (int)Math.Min(remaining, BUFFER_SIZE);
                        int bytesRead = stegoStream.Read(buffer, 0, toRead);

                        if (bytesRead == 0)
                            break;

                        outputStream.Write(buffer, 0, bytesRead);
                        remaining -= bytesRead;
                    }
                }

                Console.WriteLine($"✓ 已提取隱藏檔案: {outputFile} ({FormatFileSize(hiddenFileSize)})");
                return true;
            }
        }

        /// <summary>
        /// 將大檔案分割並隱藏到多個圖片中
        /// </summary>
        /// <param name="imageFiles">圖片檔案路徑陣列</param>
        /// <param name="hiddenFile">要隱藏的大檔案路徑</param>
        /// <param name="outputDirectory">輸出目錄</param>
        /// <param name="maxChunkSize">每個分割檔案的最大大小(位元組),預設 10MB</param>
        /// <returns>生成的圖片檔案路徑列表</returns>
        public static List<string> HideMultiFile(string[] imageFiles, string hiddenFile, string outputDirectory, string prefix = "img", long maxChunkSize = 800 * 1024)
        {
            if (imageFiles == null || imageFiles.Length == 0)
                throw new ArgumentException("至少需要一個圖片檔案");

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            FileInfo hiddenFileInfo = new FileInfo(hiddenFile);
            long totalSize = hiddenFileInfo.Length;
            int totalChunks = (int)Math.Ceiling((double)totalSize / maxChunkSize);

            List<string> outputFiles = new List<string>();

            Console.WriteLine($"開始分割檔案: {Path.GetFileName(hiddenFile)}");
            Console.WriteLine($"  總大小: {FormatFileSize(totalSize)}");
            Console.WriteLine($"  分割數: {totalChunks}");
            Console.WriteLine($"  每塊大小: {FormatFileSize(maxChunkSize)}");
            Console.WriteLine();

            using (FileStream hiddenStream = new FileStream(hiddenFile, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE))
            {
                for (int i = 0; i < totalChunks; i++)
                {
                    // 圖片不足時輪迴使用
                    string imageFile = imageFiles[i % imageFiles.Length];
                    string imageExt = Path.GetExtension(imageFile);
                    string imageName = Path.GetFileNameWithoutExtension(imageFile);
                    string outputFile = Path.Combine(outputDirectory, $"{prefix}_{i:D4}.bmp");

                    long chunkSize = Math.Min(maxChunkSize, totalSize - (i * maxChunkSize));

                    using (FileStream outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE))
                    {
                        // 1. 複製圖片
                        using (FileStream imageStream = new FileStream(imageFile, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE))
                        {
                            imageStream.CopyTo(outputStream, BUFFER_SIZE);
                        }

                        // 2. 寫入魔術標記
                        outputStream.Write(MAGIC_MARKER, 0, MAGIC_MARKER.Length);

                        // 3. 寫入本分塊大小
                        byte[] sizeBytes = BitConverter.GetBytes(chunkSize);
                        outputStream.Write(sizeBytes, 0, sizeBytes.Length);

                        // 4. 附加檔案分塊
                        long remaining = chunkSize;
                        byte[] buffer = new byte[BUFFER_SIZE];

                        while (remaining > 0)
                        {
                            int toRead = (int)Math.Min(remaining, BUFFER_SIZE);
                            int bytesRead = hiddenStream.Read(buffer, 0, toRead);

                            if (bytesRead == 0)
                                break;

                            outputStream.Write(buffer, 0, bytesRead);
                            remaining -= bytesRead;
                        }
                    }

                    outputFiles.Add(outputFile);
                    Console.WriteLine($"  [{i + 1}/{totalChunks}] {Path.GetFileName(outputFile)} ({FormatFileSize(chunkSize)}) ← {Path.GetFileName(imageFile)}");
                }
            }

            Console.WriteLine();
            Console.WriteLine($"✓ 完成! 已生成 {totalChunks} 個圖片檔案");

            return outputFiles;
        }

        /// <summary>
        /// 從多個圖片中提取並合併隱藏的檔案 (自動偵測版本)
        /// </summary>
        /// <param name="stegoImages">包含隱藏檔案的圖片路徑陣列(需依序排列)</param>
        /// <param name="outputFile">合併後的輸出檔案</param>
        public static bool ExtractMultiFile(string[] stegoImages, string outputFile)
        {
            if (stegoImages == null || stegoImages.Length == 0)
                throw new ArgumentException("至少需要一個圖片檔案");

            Console.WriteLine($"開始提取並合併檔案...");
            Console.WriteLine($"  來源圖片數: {stegoImages.Length}");
            Console.WriteLine();

            using (FileStream outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write, FileShare.None, BUFFER_SIZE))
            {
                for (int i = 0; i < stegoImages.Length; i++)
                {
                    string stegoImage = stegoImages[i];

                    using (FileStream stegoStream = new FileStream(stegoImage, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE))
                    {
                        // 尋找魔術標記
                        long markerPosition = FindMagicMarker(stegoStream);

                        if (markerPosition == -1)
                        {
                            Console.WriteLine($"  ⚠ 警告: {Path.GetFileName(stegoImage)} 沒有隱藏資料");
                            continue;
                        }

                        // 讀取分塊大小
                        stegoStream.Seek(markerPosition + MAGIC_MARKER.Length, SeekOrigin.Begin);
                        byte[] sizeBytes = new byte[8];
                        stegoStream.Read(sizeBytes, 0, 8);
                        long chunkSize = BitConverter.ToInt64(sizeBytes, 0);

                        // 複製隱藏的分塊到輸出檔案
                        byte[] buffer = new byte[BUFFER_SIZE];
                        long remaining = chunkSize;

                        while (remaining > 0)
                        {
                            int toRead = (int)Math.Min(remaining, BUFFER_SIZE);
                            int bytesRead = stegoStream.Read(buffer, 0, toRead);

                            if (bytesRead == 0)
                                break;

                            outputStream.Write(buffer, 0, bytesRead);
                            remaining -= bytesRead;
                        }

                        Console.WriteLine($"  [{i + 1}/{stegoImages.Length}] {Path.GetFileName(stegoImage)} ({FormatFileSize(chunkSize)})");
                    }
                }
            }

            FileInfo outputInfo = new FileInfo(outputFile);
            Console.WriteLine();
            Console.WriteLine($"✓ 合併完成: {outputFile}");
            Console.WriteLine($"  總大小: {FormatFileSize(outputInfo.Length)}");
            return true;
        }

        /// <summary>
        /// 尋找魔術標記的位置
        /// </summary>
        private static long FindMagicMarker(FileStream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            byte[] buffer = new byte[BUFFER_SIZE];
            long position = 0;
            int matchIndex = 0;

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == MAGIC_MARKER[matchIndex])
                    {
                        matchIndex++;
                        if (matchIndex == MAGIC_MARKER.Length)
                        {
                            // 找到完整標記,回傳標記的起始位置
                            return position + i - MAGIC_MARKER.Length + 1;
                        }
                    }
                    else
                    {
                        matchIndex = 0;
                        // 檢查當前 byte 是否是標記的開始
                        if (buffer[i] == MAGIC_MARKER[0])
                            matchIndex = 1;
                    }
                }

                position += bytesRead;
            }

            return -1; // 沒找到
        }

        /// <summary>
        /// 格式化檔案大小顯示
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:F2} {suffixes[suffixIndex]}";
        }
    }
}
