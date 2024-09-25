using System.IO;
using System.Collections.Generic;
using ZstdNet;

using Lutra.Utility;

namespace Lutra.Assets
{
    public struct ArchiveFileIndex
    {
        public long Offset;
        public long LengthBytes;
    }
    public struct ArchiveManifest
    {
        public Dictionary<string, ArchiveFileIndex> FileIndex;
        public int Version;
        public int LoadPriority;
        public bool Compressed;
        public long DataBeginOffset;
        public string Name;
    }

    public class Archive
    {
        private const string MAGIC_NUMBER = "LUTARC";
        private const int CURRENT_VERSION = 1;

        public ArchiveManifest Manifest;
        private BinaryReader ArchiveFileReader;

        public Archive(BinaryReader reader)
        {
            ArchiveFileReader = reader;
        }

        public bool ContainsFile(string filePath)
        {
            return Manifest.FileIndex.ContainsKey(filePath);
        }

        public long GetFileSize(string filePath)
        {
            if (!ContainsFile(filePath))
            {
                return -1;
            }

            return Manifest.FileIndex[filePath].LengthBytes;
        }

        public MemoryStream GetStreamForFile(string filePath)
        {
            if (!ContainsFile(filePath))
            {
                Util.LogError($"No file {filePath} present in archive {Manifest.Name}!");
                return null;
            }

            var memoryStream = new MemoryStream(GetFileBytes(filePath));
            memoryStream.Seek(0, SeekOrigin.Begin);
            return memoryStream;
        }

        public byte[] GetFileBytes(string filePath)
        {
            if (!ContainsFile(filePath))
            {
                Util.LogError($"No file {filePath} present in archive {Manifest.Name}!");
                return null;
            }
            ArchiveFileIndex file = Manifest.FileIndex[filePath];
            ArchiveFileReader.BaseStream.Seek(Manifest.DataBeginOffset + file.Offset, SeekOrigin.Begin);

            if (Manifest.Compressed)
            {
                Decompressor decompressor = new Decompressor();
                byte[] fileBytes = decompressor.Unwrap(ArchiveFileReader.ReadBytes((int)file.LengthBytes));
                return fileBytes;
            }
            else
            {
                return ArchiveFileReader.ReadBytes((int)file.LengthBytes);
            }
        }

        public string GetFileString(string filePath)
        {
            if (!ContainsFile(filePath))
            {
                Util.LogError($"No file {filePath} present in archive {Manifest.Name}!");
                return null;
            }

            return System.Text.Encoding.UTF8.GetString(GetFileBytes(filePath));
        }

        public static Archive LoadFromFile(string archivePath)
        {
            // Load archive file at path.
            if (!File.Exists(archivePath))
            {
                Util.LogError($"Archive {archivePath} does not exist.");
                return null;
            }

            var archiveFileStream = File.Open(archivePath, FileMode.Open);

            var reader = new BinaryReader(archiveFileStream, System.Text.Encoding.UTF8, true);

            Util.LogInfo($"Loading archive {archivePath}...");
            // Read 6 bytes to see if our magic key is here.
            var magicKeyTest = reader.ReadBytes(6);

            if (System.Text.Encoding.UTF8.GetString(magicKeyTest) != MAGIC_NUMBER)
            {
                Util.LogError($"Archive {archivePath} is not a Lutra Asset Archive. Got {magicKeyTest} as magic key.");
                return null;
            }

            // Now, read manifest data.
            ArchiveManifest newManifest = new ArchiveManifest();
            newManifest.Name = archivePath;
            newManifest.FileIndex = new Dictionary<string, ArchiveFileIndex>();

            // First, the version.
            newManifest.Version = reader.ReadInt32();
            if (newManifest.Version != CURRENT_VERSION)
            {
                Util.LogError($"Detected manifest version {newManifest.Version}; this version of Lutra supports only {CURRENT_VERSION}.");
                return null;
            }

            // Next, the load priority.
            newManifest.LoadPriority = reader.ReadInt32();

            // Compression enabled?
            newManifest.Compressed = reader.ReadBoolean();

            // Number of manifest records to follow.
            var records = reader.ReadInt32();

            // Load all records
            for (int i = 0; i < records; i++)
            {
                var path = reader.ReadString();
                var offset = reader.ReadInt64();
                var len = reader.ReadInt64();

                newManifest.FileIndex.Add(path, new ArchiveFileIndex { Offset = offset, LengthBytes = len });
            }

            newManifest.DataBeginOffset = reader.BaseStream.Position;

            Archive newArchive = new Archive(reader);
            newArchive.Manifest = newManifest;

            Util.LogInfo("Archive loaded!");
            Util.LogInfo($"* Files in archive: {newManifest.FileIndex.Count}");
            Util.LogInfo($"* Version: {newManifest.Version}");
            Util.LogInfo($"* Compressed: {newManifest.Compressed}");
            Util.LogInfo($"* Data Offset: {newManifest.DataBeginOffset}");
            Util.LogInfo($"* File Listing: ");
            foreach (var file in newManifest.FileIndex)
            {
                Util.LogInfo($"* * {file.Key} {file.Value.Offset}, {file.Value.LengthBytes}");
            }

            return newArchive;
        }

        [Utility.Debugging.DebugCommand(alias: "tools.createarchive", help: "Creates a Lutra Asset Archive from the specified directory, and places it in the specified location.")]
        public static void ConstructFromDirectoryAndSave(string directoryPath, string archivePath, int loadPriority, bool compressed)
        {
            if (!Directory.Exists(directoryPath))
            {
                Util.LogError($"Directory {directoryPath} does not exist.");
                return;
            }

            if (File.Exists(archivePath))
            {
                Util.LogError($"Archive {archivePath} already exists, refusing to overwrite.");
                return;
            }

            using (var archiveFileStream = File.Open(archivePath, FileMode.Create))
            {
                using (var writer = new BinaryWriter(archiveFileStream))
                {
                    Util.LogInfo($"Archiving {directoryPath} to {archivePath}...");
                    Util.LogInfo($"* Writing manifest...");
                    // First, write manifest. 
                    writer.Write(MAGIC_NUMBER.ToCharArray());
                    writer.Write(CURRENT_VERSION);
                    writer.Write(loadPriority);
                    writer.Write(compressed);

                    // Now, collate all information about files.
                    Dictionary<string, ArchiveFileIndex> fileIndex = new();
                    DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
                    FileInfo[] fileListing = directoryInfo.GetFiles("*.*", SearchOption.AllDirectories);
                    long currentOffset = 0;

                    Dictionary<string, byte[]> compressedFiles = new Dictionary<string, byte[]>();
                    Compressor compressor = new Compressor(new CompressionOptions(11));

                    foreach (FileInfo file in fileListing)
                    {
                        var relativeFilePath = file.FullName.Substring(System.Environment.CurrentDirectory.Length + 1);
                        var fileLength = file.Length;

                        if (compressed)
                        {
                            Util.LogInfo($"* Compressing file {file.Name}...");
                            var compressedFile = compressor.Wrap(File.ReadAllBytes(file.FullName));
                            compressedFiles.Add(file.FullName, compressedFile);
                            fileLength = compressedFile.Length;
                            Util.LogInfo($"* Compressed. Orig:{file.Length} After:{fileLength}");
                        }

                        fileIndex.Add(relativeFilePath, new ArchiveFileIndex
                        {
                            Offset = currentOffset,
                            LengthBytes = fileLength
                        });

                        currentOffset += fileLength;
                    }

                    Util.LogInfo($"* Writing file index...");
                    writer.Write(fileIndex.Count);
                    foreach (var fileEntry in fileIndex)
                    {
                        writer.Write(fileEntry.Key);
                        writer.Write(fileEntry.Value.Offset);
                        writer.Write(fileEntry.Value.LengthBytes);
                    }

                    // Now that the manifest is written, write the actual file data for each file.
                    Util.LogInfo($"* Archiving...");
                    foreach (FileInfo file in fileListing)
                    {
                        Util.LogInfo($"* * -> {file.FullName}");
                        if (compressed)
                        {
                            writer.Write(compressedFiles[file.FullName]);
                        }
                        else
                        {
                            writer.Write(File.ReadAllBytes(file.FullName));
                        }
                    }
                }
            }

            Util.LogInfo($"Archive creation complete at {archivePath}!");
        }
    }
}
