using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Arctron.Obj2Gltf;
using Newtonsoft.Json;

namespace Arctron.Obj23dTiles
{
    public class TilesConverter
    {
        private readonly string _objFolder;
        private readonly string _outputFolder;
        private readonly GisPosition _gisPosition;
        private readonly bool _lod;

        public bool MergeTileJsonFiles { get; set; } = true;

        public TilesConverter(string objFolder, string outputFolder, 
            GisPosition gisPosition, bool lod=false)
        {
            _objFolder = objFolder;
            _outputFolder = outputFolder;
            _gisPosition = gisPosition;
            _lod = lod;
        }

        public string Run()
        {
            var outputDir = _outputFolder;
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }
            var objFolder = _objFolder;
            // 31.19703,121.45238
            var gisPosition = _gisPosition;
            var objFiles = Directory.GetFiles(objFolder, "*.obj");
            if (MergeTileJsonFiles)
            {
                return MergeTilesets(outputDir, gisPosition, _lod, objFiles);
            }
            return CombineTilesets(outputDir, gisPosition, objFiles);
        }

        public static string MergeTilesets(string outputFolder, GisPosition gisPosition, bool lod, params string[] objFiles)
        {
            var tasks = new Task<SingleTileset>[objFiles.Length];
            var dataFolderName = "BatchedModels";
            var outputPath = Path.Combine(outputFolder, dataFolderName);
            if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
            for (var i = 0; i < objFiles.Length; i++)
            {
                var objFile = objFiles[i];
                var name = Path.GetFileNameWithoutExtension(objFile);
                tasks[i] = Task.Run(() => WriteTileset(objFile, outputPath, gisPosition));
            }
            Task.WaitAll(tasks);

            var west = double.MaxValue;
            var south = double.MaxValue;
            var north = double.MinValue;
            var east = double.MinValue;
            var minheight = double.MaxValue;
            var maxheight = double.MinValue;

            var outputTileset = Path.Combine(outputFolder, "tileset.json");

            var tilesetList = new List<SingleTileset>();

            var tiles = new List<Tile>();
            var geometricError = 500.0;
            foreach (var t in tasks)
            {
                var json = t.Result;
                tilesetList.Add(json);
                if (json.Root == null) continue;
                var boundingVolume = json.Root.BoundingVolume;
                if (boundingVolume == null) continue;
                var geometricError0 = json.GeometricError;
                if (boundingVolume.Region != null
                    && boundingVolume.Region.Length >= 6)
                {
                    west = Math.Min(west, boundingVolume.Region[0]);
                    south = Math.Min(south, boundingVolume.Region[1]);
                    east = Math.Max(east, boundingVolume.Region[2]);
                    north = Math.Max(north, boundingVolume.Region[3]);
                    minheight = Math.Min(minheight, boundingVolume.Region[4]);
                    maxheight = Math.Max(maxheight, boundingVolume.Region[5]);
                }
                if (!lod)
                {
                    tiles.Add(new Tile
                    {
                        BoundingVolume = boundingVolume,
                        GeometricError = geometricError0,
                        Refine = null,
                        Content = new TileContent
                        {
                            Url = dataFolderName + "/" + json.Root.Content.Url,
                            BoundingVolume = boundingVolume
                        }
                    });
                }

            }
            if (lod)
            {
                tiles = TilesetRegionComparer.SortTrees(tilesetList, dataFolderName);
            }

            var longitude = gisPosition.Longitude;
            var latitude = gisPosition.Latitude;
            var transHeight = gisPosition.TransHeight.HasValue ? gisPosition.TransHeight.Value : 0.0;
            var transformArray = GisUtil.Wgs84Transform(
                    longitude, latitude, transHeight).ToArray();

            var allTileset = new SingleTileset
            {
                Asset = new TilesetAsset
                {
                    Version = "0.0",
                    TilesetVersion = "1.0.0-arctron"
                },
                GeometricError = geometricError,
                Root = new Tile
                {
                    BoundingVolume = new BoundingVolume
                    {
                        Region = new double[] {
                            west,south,
                            east, north,
                            minheight, maxheight }
                    },
                    Refine = "ADD",
                    GeometricError = geometricError / 2.0,
                    Children = tiles,
                    Transform = transformArray
                }

            };


            allTileset.ResetGeometricErrors(); //TODO:

            //var longitude = gisPosition.Longitude * Math.PI / 180;
            //var latitude = gisPosition.Longitude * Math.PI / 180;
            //var transHeight = gisPosition.TransHeight.HasValue ? gisPosition.TransHeight.Value : 0;
            //var transformArray = GisUtil.Wgs84Transform(
            //        longitude, latitude, transHeight).ToArray();
            //allTileset.Root.Transform = transformArray; //TODO:
            var tilesetJsonPath = Path.Combine(outputFolder, "tileset.json");
            File.WriteAllText(tilesetJsonPath,
                JsonConvert.SerializeObject(allTileset,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                    //,Formatting = Formatting.Indented
                }));
            return tilesetJsonPath;
        }

        public static string CombineTilesets(string outputFolder, GisPosition gisPosition, params string[] objFiles)
        {
            var tasks = new Task<string>[objFiles.Length];
            for (var i = 0; i < objFiles.Length; i++)
            {
                var objFile = objFiles[i];
                var name = Path.GetFileNameWithoutExtension(objFile);
                var outputPath = Path.Combine(outputFolder, "Batched" + name);
                if (!Directory.Exists(outputPath)) Directory.CreateDirectory(outputPath);
                tasks[i] = Task.Run(() => WriteTilesetFile(objFile, outputPath, gisPosition));
            }
            Task.WaitAll(tasks);

            var west = double.MaxValue;
            var south = double.MaxValue;
            var north = double.MinValue;
            var east = double.MinValue;
            var minheight = double.MaxValue;
            var maxheight = double.MinValue;

            var outputTileset = Path.Combine(outputFolder, "tileset.json");

            var tiles = new List<Tile>();
            var geometricError = 500.0;
            foreach (var t in tasks)
            {
                var jsonFile = t.Result;
                var json = JsonConvert.DeserializeObject<SingleTileset>(
                    File.ReadAllText(jsonFile, Encoding.UTF8));
                if (json.Root == null) continue;
                var boundingVolume = json.Root.BoundingVolume;
                if (boundingVolume == null) continue;
                var geometricError0 = json.GeometricError;
                if (boundingVolume.Region != null
                    && boundingVolume.Region.Length >= 6)
                {
                    west = Math.Min(west, boundingVolume.Region[0]);
                    south = Math.Min(south, boundingVolume.Region[1]);
                    east = Math.Max(east, boundingVolume.Region[2]);
                    north = Math.Max(north, boundingVolume.Region[3]);
                    minheight = Math.Min(minheight, boundingVolume.Region[4]);
                    maxheight = Math.Max(maxheight, boundingVolume.Region[5]);
                }
                tiles.Add(new Tile
                {
                    BoundingVolume = boundingVolume,
                    GeometricError = geometricError0,
                    Refine = json.Root.Refine,
                    Content = new TileContent
                    {
                        Url = jsonFile.Substring(outputFolder.Length).TrimStart(new char[] { '\\', '/' }).Replace('\\', '/')
                    }
                });
            }

            var allTileset = new SingleTileset
            {
                Asset = new TilesetAsset
                {
                    Version = "0.0",
                    TilesetVersion = "1.0.0-arctron"
                },
                GeometricError = geometricError,
                Root = new Tile
                {
                    BoundingVolume = new BoundingVolume
                    {
                        Region = new double[] {
                            west,south,
                            east, north,
                            minheight, maxheight }
                    },
                    Refine = "ADD",
                    GeometricError = geometricError,
                    Children = tiles
                }

            };
            var tilesetJsonPath = Path.Combine(outputFolder, "tileset.json");
            File.WriteAllText(tilesetJsonPath,
                JsonConvert.SerializeObject(allTileset,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    Formatting = Formatting.Indented
                }));
            return tilesetJsonPath;
        }

        public static SingleTileset WriteTileset(string objFile, string outputPath, GisPosition gisPosition)
        {
            var fileName = Path.GetFileNameWithoutExtension(objFile);
            var b3dmFile = Path.Combine(outputPath, fileName + ".b3dm");
            var tilesetOptions = WriteB3dm(objFile, b3dmFile);
            //tilesetOptions.TransHeight = 0;
            //tilesetOptions.MinHeight = 0;
            //tilesetOptions.MaxHeight = 40;
            //tilesetOptions.TileWidth = 200;
            //tilesetOptions.TileHeight = 200;
            tilesetOptions.SetPosition(gisPosition);
            tilesetOptions.UseRegion = true;
            var singleTileset = SingleTileset.Create(tilesetOptions);
            return singleTileset;
        }

        public static string WriteTilesetFile(string objFile, string outputPath, GisPosition gisPosition)
        {
            var singleTileset = WriteTileset(objFile, outputPath, gisPosition);

            var tilesetFile = Path.Combine(outputPath, "tileset.json");
            var tilesetJson = JsonConvert.SerializeObject(singleTileset, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            });
            File.WriteAllText(tilesetFile, tilesetJson, Encoding.UTF8);
            return tilesetFile;
        }

        public static TilesetCreationOptions WriteB3dm(string objFile,
            string outputFile, Options options = null)
        {
            if (options == null)
            {
                options = new Options();
            }
            var converter = new Converter(objFile, true, true);
            var glb = converter.GetGlb();
            var batchTableJson = converter.GetBatchTable();
            var length = batchTableJson.MaxPoint.Count;
            var boundary = 8;
            if (options.FeatureTableJson == null || options.FeatureTableJson.Count == 0)
            {
                if (options.FeatureTableJson == null)
                {
                    options.FeatureTableJson = new List<byte>();
                }
                var featureTable = new FeatureTable() { BatchLength = (uint)length };
                var featureTableJsonBuffer =
                Converter.GetJsonBufferPadded(featureTable, boundary, 28);
                options.FeatureTableJson.AddRange(featureTableJsonBuffer);
            }
            List<byte> featureTableBinary;
            if (options != null && options.FeatureTableBinary != null)
            {
                featureTableBinary = options.FeatureTableBinary;
            }
            else
            {
                featureTableBinary = new List<byte>();
            }
            var batchTableJsonBuffer = Converter.GetJsonBufferPadded(batchTableJson, boundary);
            options.BatchTableJson = batchTableJsonBuffer.ToList();
            List<byte> batchTableBinary;
            if (options != null && options.BatchTableBinary != null)
            {
                batchTableBinary = options.BatchTableBinary;
            }
            else
            {
                batchTableBinary = new List<byte>();
            }

            var b3dm = new B3dm(glb);
            var bytes = b3dm.Convert(options);

            File.WriteAllBytes(outputFile, bytes);

            var tileFullname = Path.GetFileName(outputFile);
            var folder = Path.GetDirectoryName(outputFile);
            var tilesetPath = Path.Combine(folder, "tileset.json");
            var tilesetOptions = new TilesetCreationOptions();
            //var batchTableJson = json;
            var minMaxX = new MinMax();
            Converter.UpdateMinMax(
                batchTableJson.MaxPoint.Select(c => c[0]).Concat(
                    batchTableJson.MinPoint.Select(c => c[0])).ToArray(), minMaxX);
            var minMaxY = new MinMax();
            Converter.UpdateMinMax(
                batchTableJson.MaxPoint.Select(c => c[1]).Concat(
                    batchTableJson.MinPoint.Select(c => c[1])).ToArray(), minMaxY);
            var minMaxZ = new MinMax();
            Converter.UpdateMinMax(
                batchTableJson.MaxPoint.Select(c => c[2]).Concat(
                    batchTableJson.MinPoint.Select(c => c[2])).ToArray(), minMaxZ);
            var width = minMaxX.Max - minMaxX.Min;
            var height = minMaxZ.Max - minMaxZ.Min;
            width = Math.Ceiling(width);
            height = Math.Ceiling(height);

            var offsetX = width / 2 + minMaxX.Min;
            var offsetY = height / 2 + minMaxZ.Min;
            tilesetOptions.TileName = tileFullname;
            tilesetOptions.TileWidth = width;
            tilesetOptions.TileHeight = height;
            tilesetOptions.OriginalX = minMaxX;
            tilesetOptions.OriginalY = minMaxY;
            tilesetOptions.OriginalZ = minMaxZ;
            tilesetOptions.TransHeight = -minMaxY.Min;
            tilesetOptions.MinHeight = minMaxY.Min + tilesetOptions.TransHeight;
            tilesetOptions.MaxHeight = minMaxY.Max + tilesetOptions.TransHeight;
            tilesetOptions.OffsetX = offsetX;
            tilesetOptions.OffsetY = offsetY;

            return tilesetOptions;

        }
    }
}
