using System;
using Xunit;
using System.IO;
using System.Collections.Generic;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Threading.Tasks;

namespace Arctron.Obj23dTiles.Tests
{
    public class TilesConverterTests
    {
        private static string TestObjFile = @"..\..\..\..\testassets\Office\model.obj";

        static void CheckObjFiles()
        {
            Assert.True(File.Exists(TestObjFile), "Obj File does not exist!");
        }

        [Fact]
        public void Test_WriteTileset()
        {
            CheckObjFiles();
            var outputDir = "tileset";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            var gisPosition = new GisPosition(121.42824921445794, 31.2151756310598, 0); //(103.844016705, 1.32066772, 2.0);
            TilesConverter.WriteTilesetFile(TestObjFile, outputDir, gisPosition);
            Assert.True(File.Exists(Path.Combine(outputDir, "tileset.json")));
        }

        public static void ExtractZipFile(string archiveFilenameIn, string password, string outFolder)
        {
            ZipFile zf = null;
            try
            {
                FileStream fs = File.OpenRead(archiveFilenameIn);
                zf = new ZipFile(fs);
                if (!String.IsNullOrEmpty(password))
                {
                    zf.Password = password;     // AES encrypted entries are handled automatically
                }
                foreach (ZipEntry zipEntry in zf)
                {
                    if (!zipEntry.IsFile)
                    {
                        continue;           // Ignore directories
                    }
                    String entryFileName = zipEntry.Name;
                    // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                    // Optionally match entrynames against a selection list here to skip as desired.
                    // The unpacked length is available in the zipEntry.Size property.

                    byte[] buffer = new byte[4096];     // 4K is optimum
                    Stream zipStream = zf.GetInputStream(zipEntry);

                    // Manipulate the output filename here as desired.
                    String fullZipToPath = Path.Combine(outFolder, entryFileName);
                    string directoryName = Path.GetDirectoryName(fullZipToPath);
                    if (directoryName.Length > 0)
                        Directory.CreateDirectory(directoryName);

                    // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                    // of the file, but does not waste memory.
                    // The "using" will close the stream even if an exception occurs.
                    using (FileStream streamWriter = File.Create(fullZipToPath))
                    {
                        StreamUtils.Copy(zipStream, streamWriter, buffer);
                    }
                }
            }
            finally
            {
                if (zf != null)
                {
                    zf.IsStreamOwner = true; // Makes close also shut the underlying stream
                    zf.Close(); // Ensure we release resources
                }
            }
        }

        [Fact]
        public void Test_BatchTileset()
        {
            var folder = @"TestObjs";
            Assert.True(Directory.Exists(folder), "Input folder does not exist!");
            var outputDir = "BatchTests";
            var files = Directory.GetFiles(folder); // obj Files are zipped with mtl files
            foreach(var f in files)
            {
                var name = Path.GetFileNameWithoutExtension(f);
                var dir = Path.Combine(folder, name);
                Directory.CreateDirectory(dir);
                ExtractZipFile(f, String.Empty, dir);
            }
            var gisPosition = new GisPosition
            {
                Longitude = 1.81245, //1.812478, //2.1196599980996,
                Latitude = 0.023043, //0.0230270, //0.543224178326409,
                TransHeight = 15.0
            };
            var objFiles = Directory.GetFiles(folder, "*.obj", SearchOption.AllDirectories);
            var tasks = new Task<string>[objFiles.Length];
            for(var i = 0;i<objFiles.Length;i++)
            {
                var objFile = objFiles[i];
                var dd = Path.GetDirectoryName(objFile);
                
                var name = "Batched" + Path.GetFileNameWithoutExtension(dd);
                var outFolder = Path.Combine(outputDir, name);
                Directory.CreateDirectory(outFolder);
                tasks[i] = Task.Run(() => TilesConverter.WriteTilesetFile(objFile, outFolder, gisPosition));
            }
            Task.WaitAll(tasks);
            var strs = new List<string>();
            var tilesetListFile = Path.Combine(outputDir, "tileset.txt");
            foreach(var t in tasks)
            {
                var res = t.Result;
                var name = Path.GetFileNameWithoutExtension(res);
                var dir = Path.GetDirectoryName(res);
                var bName = Path.GetFileNameWithoutExtension(dir);
                strs.Add($"loadTileSet('{bName}', 'BatchedTest/{bName}/tileset.json');");
            }
            using (var sw = new StreamWriter(tilesetListFile, true, System.Text.Encoding.UTF8))
            {
                foreach(var s in strs)
                {
                    sw.WriteLine(s);
                }
                sw.Flush();
                sw.Close();
            }
        }

        [Fact]
        public void Test_CombineTilesets()
        {
            var name = "test";
            var objFolder = @"TestObjsFolder";
            Assert.True(Directory.Exists(objFolder), "Input Folder does not exist!");
            var outputDir = name; //"mtileset";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            // 31.19703,121.45238
            var gisPosition = new GisPosition(121.449, 31.1989, 40.0);

            var tileConverter = new TilesConverter(objFolder, outputDir, gisPosition);
            var tilesetJson = tileConverter.Run();

            //var objFiles = Directory.GetFiles(objFolder, "*.obj");
            //var tilesetJson = Utility.CombineTilesets(outputDir, gisPosition, objFiles);

            Assert.True(File.Exists(tilesetJson));
        }

        [Fact]
        public void Test_MergeTilesets()
        {
            var name = "test";
            var objFolder = @"TestObjsFolder";
            Assert.True(Directory.Exists(objFolder), "Input Folder does not exist!");
            var outputDir = name+"2"; //"mtileset";
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            // 31.19703,121.45238
            var gisPosition = new GisPosition(121.23823421692855, 31.838769935520705, 0.0);

            var tileConverter = new TilesConverter(objFolder, outputDir, gisPosition, true);
            var tilesetJson = tileConverter.Run();
            //var objFiles = Directory.GetFiles(objFolder, "*.obj");
            //var tilesetJson = Utility.MergeTilesets(outputDir, gisPosition, true, objFiles);

            Assert.True(File.Exists(tilesetJson));
        }

        [Fact]
        public void TestIntBytes()
        {
            var i = 1;
            uint ii = 1;
            var iBytes = BitConverter.GetBytes(i);
            var iiBytes = BitConverter.GetBytes(ii);
            if (!BitConverter.IsLittleEndian)
            {
            }
            Assert.True(iBytes.Length == iiBytes.Length);
        }

        [Fact]
        public void Test_Wgs84()
        {
            var m = GisUtil.Wgs84Transform(2.1196599980996, 0.543224178326409, 0);
            Assert.True(m != null);
        }
    }
}
