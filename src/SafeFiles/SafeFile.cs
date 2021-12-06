using System;
using System.IO;
using System.Threading.Tasks;

namespace SafeFiles {
    public static class SafeFile {
        const string PartFileSuffix = ".part";

        public static bool Exists(string filePath) {
            return File.Exists(filePath);
        }

        static void DeletePart(string filePath) {
            var partPath = $"{filePath}{PartFileSuffix}";
            if (File.Exists(partPath)) {
                File.Delete(partPath);
            }
        }

        public static void Replace(string srcFilePath, string dstFilePath) {
            if (!File.Exists(srcFilePath)) {
                throw new FileNotFoundException("File to replace is not found.", srcFilePath);
            }
            
            DeletePart(srcFilePath);
            File.Replace(srcFilePath, dstFilePath, null);
        }

        public static void Delete(string filePath) {
            DeletePart(filePath);
            if (File.Exists(filePath)) {
                File.Delete(filePath);
            }
        }

        public static async Task<T> Read<T>(string filePath, Func<Stream?, Task<T>> reader) {
            DeletePart(filePath);

            if (!File.Exists(filePath)) {
                return await reader(null).ConfigureAwait(false);
            }

#if NETSTANDARD2_0
            using var stream = File.OpenRead(filePath);
#else
            await using var stream = File.OpenRead(filePath);
#endif
            return await reader(stream).ConfigureAwait(false);
        }

        public static async Task Write(string filePath, Func<Stream, Task> writer) {
            var partPath = $"{filePath}{PartFileSuffix}";
            
#if NETSTANDARD2_0
            using (var stream = File.OpenWrite(partPath)) {
#else
            await using (var stream = File.OpenWrite(partPath)) {
#endif
                await writer(stream).ConfigureAwait(false);
            }
            if (File.Exists(filePath)) {
                File.Replace(partPath, filePath, null);
            }
            else {
                File.Move(partPath, filePath);
            }
        }

        public static async Task<string?> ReadAllText(string filePath) =>
            await Read(filePath, async stream => {
                if (stream == null) {
                    return null;
                }

                using var sr = new StreamReader(stream);
                return await sr.ReadToEndAsync();
            });

        public static async Task WriteAllText(string filePath, string content) =>
            await Write(filePath, async stream => {
#if NETSTANDARD2_0
            using var sr = new StreamWriter(stream);
#else
            await using var sr = new StreamWriter(stream);
#endif
                await sr.WriteAsync(content);
            });
    }
}