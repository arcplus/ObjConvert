using Arctron.Obj2Gltf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Arctron.Obj23dTiles
{
    /// <summary>
    /// represents a tileset.json file
    /// </summary>
    public class SingleTileset
    {       
        /// <summary>
        ///  asset
        /// </summary>
        [JsonProperty("asset")]
        public TilesetAsset Asset { get; set; }
        /// <summary>
        /// additional properties
        /// </summary>
        [JsonProperty("properties")]
        public Dictionary<string, object> Properties { get; set; }
        /// <summary>
        /// geometric errors
        /// </summary>
        [JsonProperty("geometricError")]
        public double GeometricError { get; set; } = 200.0;
        /// <summary>
        /// tile root (tileset.json or b3dm file)
        /// </summary>
        [JsonProperty("root")]
        public Tile Root { get; set; }

        public void ResetGeometricErrors(double factor = 2.0)
        {
            if (Root == null) return;
            if (factor <= 1)
            {
                return;
            }
            Root.ResetGeometricErrors(factor);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="options">options obtained when convert b3dm data</param>
        /// <returns></returns>
        public static SingleTileset Create(TilesetCreationOptions options)
        {
            if (options == null) options = new TilesetCreationOptions();

            var longitude = options.Longitude;
            var latitude = options.Latitude;
            var minHeight = options.MinHeight;
            var maxHeight = options.MaxHeight;
            var transHeight = options.TransHeight;
            var tileWidth = options.TileWidth;
            var tileHeight = options.TileHeight;
            var offsetX = options.OffsetX;
            var offsetY = options.OffsetY;
            var upAxis = options.GltfUpAxis ?? "Y";
            // properties
            var geometricError = options.GeometricError;
            var transformArray = options.Transfrom;
            if (transformArray == null || transformArray.Length == 0)
            {
                transformArray = GisUtil.Wgs84Transform(
                    longitude, latitude, transHeight).ToArray();
            }
            var height = maxHeight - minHeight;
            if (!(options.UseRegion || options.UseBox || options.UseSphere))
            {
                options.UseRegion = true;
            }
            BoundingVolume boundingVolume = null;
            if (options.UseRegion)
            {
                var longitudeExtent = GisUtil.MetersToLongitude(tileWidth, latitude);
                var latitudeExtent = GisUtil.MetersToLatituide(tileHeight);

                var west = longitude - longitudeExtent / 2 + 
                    offsetX / tileWidth * longitudeExtent;
                var south = latitude - latitudeExtent / 2 - 
                    offsetY / tileHeight * latitudeExtent;
                var east = longitude + longitudeExtent / 2 + 
                    offsetX / tileWidth * longitudeExtent;
                var north = latitude + latitudeExtent / 2 - 
                    offsetY / tileHeight * latitudeExtent;

                boundingVolume = new BoundingVolume
                {
                    Region = new double[] { west, south, east, north, minHeight, maxHeight }
                };
            }
            else if (options.UseBox)
            {
                boundingVolume = new BoundingVolume
                {
                    Box = new double[] {
                        offsetX, -offsetY, height / 2 + minHeight,       // center
                        tileWidth / 2, 0, 0,                 // width
                        0, tileHeight / 2, 0,                // depth
                        0, 0, height / 2                     // height
                    }
                };
            }
            else if (options.UseSphere)
            {
                boundingVolume = new BoundingVolume
                {
                    Sphere = new double[]
                    {
                        offsetX, -offsetY, height / 2 + minHeight,
                        Math.Sqrt(tileWidth * tileWidth / 4 + tileHeight * tileHeight / 4 + height * height / 4)
                    }
                };
            }

            return new SingleTileset
            {
                Asset = new TilesetAsset { GltfUpAxis = upAxis },
                //Properties
                GeometricError = geometricError,
                Root = new Tile
                {
                    Transform = transformArray,
                    BoundingVolume = boundingVolume,
                    Content = new TileContent
                    {
                        Url = options.TileName
                    },
                    OriginalX = options.OriginalX,
                    OriginalY = options.OriginalY,
                    OriginalZ = options.OriginalZ
                }
            };
        }


    }
    /// <summary>
    /// tile data model
    /// </summary>
    public class Tile
    {
        /// <summary>
        /// geo transform
        /// </summary>
        [JsonProperty("transform")]
        public double[] Transform { get; set; }
        /// <summary>
        /// bounding
        /// </summary>
        [JsonProperty("boundingVolume")]
        public BoundingVolume BoundingVolume { get; set; }
        /// <summary>
        /// geo error
        /// </summary>
        [JsonProperty("geometricError")]
        public double GeometricError { get; set; }
        /// <summary>
        /// ADD or REPLACE
        /// </summary>
        [JsonProperty("refine")]
        public string Refine { get; set; } // "ADD"
        /// <summary>
        /// tileset.json or b3dm file
        /// </summary>
        [JsonProperty("content")]
        public TileContent Content { get; set; }
        /// <summary>
        /// children tiles
        /// </summary>
        [JsonProperty("children")]
        public List<Tile> Children { get; set; }
        [JsonIgnore]
        public MinMax OriginalX { get; set; }
        [JsonIgnore]
        public MinMax OriginalY { get; set; }
        [JsonIgnore]
        public MinMax OriginalZ { get; set; }

        public void ResetGeometricErrors(double factor=2.0)
        {
            if (Children != null && GeometricError > 0)
            {
                foreach(var t in Children)
                {
                    if (t.GeometricError >= GeometricError)
                    {
                        t.GeometricError /= factor;
                        t.ResetGeometricErrors(factor);
                    }                    
                }
            }
        }

        public double GetBounding()
        {
            var x = (OriginalX.Max - OriginalX.Min);
            var y = (OriginalY.Max - OriginalY.Min);
            var z = (OriginalZ.Max - OriginalZ.Min);

            return x + y + z;
        }
    }
    /// <summary>
    /// tile data
    /// </summary>
    public class TileContent
    {
        /// <summary>
        /// bounding
        /// </summary>
        [JsonProperty("boundingVolume")]
        public BoundingVolume BoundingVolume { get; set; }
        /// <summary>
        /// tileset.json or b3dm file
        /// </summary>

        [JsonProperty("url")]
        public string Url { get; set; }        
    }
    /// <summary>
    /// asset definition
    /// </summary>
    public class TilesetAsset
    {
        /// <summary>
        /// asset version
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; } = "0.0";
        /// <summary>
        /// TilesetVersion
        /// </summary>
        [JsonProperty("tilesetVersion")]
        public string TilesetVersion { get; set; } = "1.0.0-arctron";
        /// <summary>
        /// Up axis, Y or Z
        /// </summary>
        [JsonProperty("gltfUpAxis")]
        public string GltfUpAxis { get; set; } = "Y";
        
    }
    /// <summary>
    ///  bounding
    /// </summary>
    public class BoundingVolume
    {
        /// <summary>
        /// use region (west, south, east, north, minHeight, maxHeight)
        /// </summary>
        [JsonProperty("region")]
        public double[] Region { get; set; }
        /// <summary>
        /// bounding box, 12
        /// </summary>
        [JsonProperty("box")]
        public double[] Box { get; set; }
        /// <summary>
        /// bounding sphere, 4
        /// </summary>
        [JsonProperty("sphere")]
        public double[] Sphere { get; set; }
    }

    public class TilesetRegionComparer : Comparer<SingleTileset>
    {
        private readonly TileRegionComparer _comparer = new TileRegionComparer();
        public override int Compare(SingleTileset x0, SingleTileset y0)
        {
            if (x0 == null || y0 == null) return 0;
            var xTile = x0.Root;
            var yTile = y0.Root;

            var val = _comparer.Compare(xTile, yTile);
            if (val != 0) return val;
            return TileRegionComparer.ContainCompare(xTile, yTile);
        }

        private static Tile ToTile(SingleTileset tileset, string parentFolder)
        {
            var tile = new Tile
            {
                BoundingVolume = tileset.Root.BoundingVolume,
                Content = new TileContent
                {
                    BoundingVolume = tileset.Root.BoundingVolume,
                    Url = parentFolder + "/" + tileset.Root.Content.Url
                },
                GeometricError = tileset.GeometricError,
                Children = new List<Tile>()
            };
            var max = new[] {
                tileset.Root.OriginalX.Max - tileset.Root.OriginalX.Min,
                tileset.Root.OriginalY.Max - tileset.Root.OriginalY.Min,
                tileset.Root.OriginalZ.Max - tileset.Root.OriginalZ.Min
            }.Max();
            tile.GeometricError = max / 20.0;
            return tile;
        }

        public static List<Tile> SortTrees(List<SingleTileset> tilesets, string parentFolder)
        {
            var comparer = new TilesetRegionComparer();
            tilesets.Sort(comparer);
            var nodes = new List<Tile>();
            var ts0 = tilesets[tilesets.Count - 1];
            nodes.Add(ToTile(ts0, parentFolder));
            Tile parent = null;
            Tile current = nodes[0];
            for(var i = tilesets.Count-1;i>0;i--)
            {
                var j = i - 1;
                var node = ToTile(tilesets[j], parentFolder);
                var sign = comparer.Compare(tilesets[i], tilesets[j]);
                if (sign == 1)
                {                    
                    current.Children.Add(node);
                    parent = current;                    
                }
                else if (sign == 0)
                {
                    if (parent != null)
                    {
                        parent.Children.Add(node);
                    }
                    else
                    {
                        nodes.Add(node);
                    }
                    //parent = current;
                }
                else
                {

                }
                current = node;
            }
            return nodes;
        }
        
    }
    

    public class TileRegionComparer : Comparer<Tile>
    {
        /// <summary>
        /// near / far
        /// </summary>
        /// <param name="m1"></param>
        /// <param name="m2"></param>
        /// <returns></returns>
        internal static double Percent(MinMax m1, MinMax m2)
        {
            var min = m1.Min;
            if (min > m2.Min) min = m2.Min;
            var max = m1.Max;
            if (max < m2.Max) max = m2.Max;
            var size = max - min;
            if (m1.Max <= m2.Min) return 0.0;
            if (m2.Max <= m1.Min) return 0.0;

            if (m1.Max > m2.Min)
            {
                return (m1.Max - m2.Min) / size;
            }
            if (m2.Max > m1.Min)
            {
                return (m2.Max - m1.Min) / size;
            }
            if (m1.Min >= m2.Min && m1.Max <= m2.Max)
            {
                return (m1.Max - m1.Min) / size;
            }
            if (m2.Min >= m1.Min && m2.Max <= m1.Max)
            {
                return (m2.Max - m2.Min) / size;
            }

            return 0.0;

        }
        private static int CompareXToY(double xMin, double xMax, double yMin, double yMax)
        {
            if (xMin >= yMin && xMax < yMax) return -1;
            if (xMin <= yMin && xMax > yMax) return 1;
            return 0;
        }
        public override int Compare(Tile xTile, Tile yTile)
        {
            var x = xTile.BoundingVolume;
            var y = yTile.BoundingVolume;
            if (x.Region == null || y.Region == null) return 0;
            var xWest = xTile.OriginalX.Min; //x.Region[0];
            var xSouth = xTile.OriginalZ.Min; //x.Region[1];
            var xEast = xTile.OriginalX.Max; //x.Region[2];
            var xNorth = xTile.OriginalZ.Max; //x.Region[3];
            var xMin = xTile.OriginalY.Min; //x.Region[4];
            var xMax = xTile.OriginalY.Max; //x.Region[5];

            var yWest = yTile.OriginalX.Min; //y.Region[0];
            var ySouth = yTile.OriginalZ.Min; //y.Region[1];
            var yEast = yTile.OriginalX.Max; //y.Region[2];
            var yNorth = yTile.OriginalZ.Max; //y.Region[3];
            var yMin = yTile.OriginalY.Min; //y.Region[4];
            var yMax = yTile.OriginalY.Max; //y.Region[5];

            var weSign = CompareXToY(xWest, xEast, yWest, yEast);
            var nsSign = CompareXToY(xSouth, xNorth, ySouth, yNorth);
            var heitSign = CompareXToY(xMin, xMax, yMin, yMax);
            if (weSign == -1 && nsSign == -1 && heitSign == -1)
            {
                return -1;
            }
            if (weSign == 1 && nsSign == 1 && heitSign == 1)
            {
                return 1;
            }


            return 0;

        }

        internal static int ContainCompare(Tile xTile, Tile yTile)
        {
            var xVol = xTile.GetBounding();
            var yVol = yTile.GetBounding();
            var xScale = Percent(xTile.OriginalX, yTile.OriginalX);
            var yScale = Percent(xTile.OriginalY, yTile.OriginalY);
            var zScale = Percent(xTile.OriginalZ, yTile.OriginalZ);

            var factor = 0.6;
            if (xScale > factor && yScale > factor && zScale > factor)
            {
                if (xVol < yVol) return -1;
                if (xVol > yVol) return 1;
            }

            return 0;
        }
    }
}
