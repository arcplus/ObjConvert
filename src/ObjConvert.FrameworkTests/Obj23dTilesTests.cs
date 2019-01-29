using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Arctron.Obj23dTiles;
using ICSharpCode.SharpZipLib.Core;
using ICSharpCode.SharpZipLib.Zip;

namespace Arctron.ObjConvert.FrameworkTests
{
    class Obj23dTilesTests
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objFile">obj文件路径</param>
        /// <param name="modelName">模型名称</param>
        /// <param name="outputDir">导出的文件夹路径</param>
        /// <param name="gisPosition">GIS坐标</param>
        /// <returns></returns>
        public static string WriteTileset(string objFile, string modelName,
            string outputDir, GisPosition gisPosition)
        {
            var name = modelName;
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            return TilesConverter.WriteTilesetFile(objFile, outputDir, gisPosition);
        }

        public static string WriteTilesetWithZip(string objZipFile, string outputDir, GisPosition gisPosition)
        {
            var name = Path.GetFileNameWithoutExtension(objZipFile);
            var unzipDir = Path.Combine(Path.GetDirectoryName(objZipFile), name);
            if (Directory.Exists(unzipDir))
            {
                Directory.Delete(unzipDir, true);
            }
            Directory.CreateDirectory(unzipDir);
            try
            {
                ExtractZipFile(objZipFile, unzipDir);
                var objFile = Path.Combine(unzipDir, "model.obj");
                if (!File.Exists(objFile))
                {
                    objFile = Path.Combine(unzipDir, name + ".obj");
                }
                if (!File.Exists(objFile))
                {
                    throw new FileNotFoundException("Obj file not found", objFile);
                }
                return WriteTileset(objFile, name, outputDir, gisPosition);
            }
            finally
            {
                Directory.Delete(unzipDir, true);
            }

        }
        

        public static string WriteMTilesetsWithZip(string objZipFile, string outputDir, GisPosition gisPosition)
        {
            var name = Path.GetFileNameWithoutExtension(objZipFile);
            var unzipDir = Path.Combine(Path.GetDirectoryName(objZipFile), name);
            if (Directory.Exists(unzipDir))
            {
                Directory.Delete(unzipDir, true);
            }
            Directory.CreateDirectory(unzipDir);
            try
            {
                ExtractZipFile(objZipFile, unzipDir);
                return WriteMTilesets(unzipDir, outputDir, gisPosition);
            }
            finally
            {
                Directory.Delete(unzipDir, true);
            }

        }

        public static string MergeMTilesetsWithZip(string objZipFile, string outputDir, GisPosition gisPosition, bool lod)
        {
            var name = Path.GetFileNameWithoutExtension(objZipFile);
            var unzipDir = Path.Combine(Path.GetDirectoryName(objZipFile), name);
            if (Directory.Exists(unzipDir))
            {
                Directory.Delete(unzipDir, true);
            }
            Directory.CreateDirectory(unzipDir);
            try
            {
                ExtractZipFile(objZipFile, unzipDir);
                return MergeMTilesets(unzipDir, outputDir, gisPosition, lod);
            }
            finally
            {
                Directory.Delete(unzipDir, true);
            }

        }

        public static string WriteMTilesets(string objFolder, string outputDir, GisPosition gisPosition, bool merge=false)
        {
            var tileConverter = new TilesConverter(objFolder, outputDir, gisPosition) { MergeTileJsonFiles = merge };
            return tileConverter.Run();
            //var objFiles = Directory.GetFiles(objFolder, "*.obj");
            //var tilesetJson = Utility.CombineTilesets(outputDir, gisPosition, objFiles);
            //return tilesetJson;
        }

        public static string MergeMTilesets(string objFolder, string outputDir, GisPosition gisPosition, bool lod)
        {
            var tileConverter = new TilesConverter(objFolder, outputDir, gisPosition);
            return tileConverter.Run(lod);
            //var objFiles = Directory.GetFiles(objFolder, "*.obj");
            //var tilesetJson = Utility.MergeTilesets(outputDir, gisPosition, true, objFiles);
            //return tilesetJson;
        }

        public static string SplitObjAndMergeMTilesetsWithZip(string objZipFile, string outputDir, GisPosition gisPosition, int splitLevel = 2)
        {
            var name = Path.GetFileNameWithoutExtension(objZipFile);
            var unzipDir = Path.Combine(Path.GetDirectoryName(objZipFile), name);
            if (Directory.Exists(unzipDir))
            {
                Directory.Delete(unzipDir, true);
            }
            Directory.CreateDirectory(unzipDir);
            try
            {
                ExtractZipFile(objZipFile, unzipDir);
                var objFile = Path.Combine(unzipDir, "model.obj");
                if (!File.Exists(objFile))
                {
                    objFile = Path.Combine(unzipDir, name + ".obj");
                }
                if (!File.Exists(objFile))
                {
                    throw new FileNotFoundException("Obj file not found", objFile);
                }
                var tilesOpts = new TilesOptions { MergeTileJsonFiles = true, OutputFolder = outputDir, WriteChildTileJson = false };
                using (var objParser = new Obj2Gltf.WaveFront.ObjParser(objFile))
                {
                    var objModel = objParser.GetModel();
                    var objModels = objModel.Split(splitLevel);
                    var tilesConverter = new TilesConverter(unzipDir, objModels, gisPosition, tilesOpts);
                    return tilesConverter.Run();
                }
                    
            }
            finally
            {
                Directory.Delete(unzipDir, true);
            }

        }

        public static void ExtractZipFile(string archiveFilenameIn, string outFolder)
        {
            string password = String.Empty;

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
                    string fullZipToPath = Path.Combine(outFolder, entryFileName);
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
    }
}
