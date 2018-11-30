using System;
using Arctron.Obj2Gltf;
using Newtonsoft.Json;

namespace Arctron.Obj23dTiles
{
    /// <summary>
    /// Gis Position
    /// </summary>
    public class GisPosition
    {
        public GisPosition() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lonDegree">Longitude in degrees</param>
        /// <param name="latDegree">Latitude in degrees</param>
        /// <param name="transHeight">Tile origin's height in meters.</param>
        public GisPosition(double lonDegree, double latDegree, double transHeight)
        {
            Longitude = Math.PI * lonDegree / 180.0;
            Latitude = Math.PI * latDegree / 180.0;
            TransHeight = transHeight;
        }
        /// <summary>
        /// Tile origin's(models' point (0,0,0)) longitude in radian.
        /// </summary>
        [JsonProperty("longitude")]
        public double Longitude { get; set; }
        /// <summary>
        /// Tile origin's latitude in radian.
        /// </summary>
        [JsonProperty("latitude")]
        public double Latitude { get; set; }
        /// <summary>
        /// Tile origin's height in meters.
        /// </summary>
        [JsonProperty("transHeight")]
        public double? TransHeight { get; set; }
    }

    //public class TilesetOptions
    //{
    //    /// <summary>
    //    /// Tile origin's(models' point (0,0,0)) longitude in radian.
    //    /// </summary>
    //    [JsonProperty("longitude")]
    //    public double Longitude { get; set; }
    //    /// <summary>
    //    /// Tile origin's latitude in radian.
    //    /// </summary>
    //    [JsonProperty("latitude")]
    //    public double Latitude { get; set; }
    //    /// <summary>
    //    /// Tile origin's height in meters.
    //    /// </summary>
    //    [JsonProperty("transHeight")]
    //    public double TransHeight { get; set; }
    //    /// <summary>
    //    /// Using region bounding volume.
    //    /// </summary>
    //    [JsonProperty("region")]
    //    public bool UseRegion { get; set; } = true;
    //    /// <summary>
    //    /// Using box bounding volume.
    //    /// </summary>
    //    [JsonProperty("box")]
    //    public bool UseBox { get; set; }
    //    /// <summary>
    //    /// Using sphere bounding volume.
    //    /// </summary>
    //    [JsonProperty("sphere")]
    //    public bool UseSphere { get; set; }
    //}

    
    /// <summary>
    /// generated when convert b3dm data
    /// </summary>
    public class TilesetCreationOptions
    {
        /// <summary>
        /// The tile name of root.
        /// </summary>
        [JsonProperty("tileName")]
        public string TileName { get; set; }
        /// <summary>
        /// The longitute of tile origin point.
        /// </summary>
        [JsonProperty("longitude")]
        public double Longitude { get; set; } = 2.1196599980996;
        /// <summary>
        /// The latitute of tile origin point
        /// </summary>
        [JsonProperty("latitude")]
        public double Latitude { get; set; } = 0.543224178326409;
        /// <summary>
        /// The minimum height of the tile.
        /// </summary>
        [JsonProperty("minHeight")]
        public double MinHeight { get; set; }
        /// <summary>
        /// The maximum height of the tile.
        /// </summary>
        [JsonProperty("maxHeight")]
        public double MaxHeight { get; set; } = 40.0;
        /// <summary>
        /// The horizontal length (cross longitude) of tile.
        /// </summary>
        [JsonProperty("tileWidth")]
        public double TileWidth { get; set; } = 200.0;
        /// <summary>
        /// The vertical length (cross latitude) of tile.
        /// </summary>
        [JsonProperty("tileHeight")]
        public double TileHeight { get; set; } = 200.0;
        /// <summary>
        /// The transform height of the tile.
        /// </summary>
        [JsonProperty("transHeight")]
        public double TransHeight { get; set; }
        [JsonIgnore]
        public MinMax OriginalX { get; set; }
        [JsonIgnore]
        public MinMax OriginalY { get; set; }
        [JsonIgnore]
        public MinMax OriginalZ { get; set; }

        [JsonProperty("offsetX")]
        public double OffsetX { get; set; }
        
        [JsonProperty("offsetY")]
        public double OffsetY { get; set; }
        /// <summary>
        /// The up axis of model. X, Y, Z
        /// </summary>
        [JsonProperty("gltfUpAxis")]
        public string GltfUpAxis { get; set; }
        /// <summary>
        /// The geometric error of tile.
        /// </summary>
        [JsonProperty("geometricError")]
        public double GeometricError { get; set; } = 200.0;
        /// <summary>
        /// The tile transform.
        /// </summary>
        [JsonProperty("transfrom")]
        public double[] Transfrom { get; set; }
        /// <summary>
        /// Using region bounding volume.
        /// </summary>
        [JsonProperty("region")]
        public bool UseRegion { get; set; } = true;
        /// <summary>
        /// Using box bounding volume.
        /// </summary>
        [JsonProperty("box")]
        public bool UseBox { get; set; }
        /// <summary>
        /// Using sphere bounding volume.
        /// </summary>
        [JsonProperty("sphere")]
        public bool UseSphere { get; set; }

        public void SetPosition(GisPosition pos)
        {
            if (pos != null)
            {
                Longitude = pos.Longitude;
                Latitude = pos.Latitude;
                if (pos.TransHeight.HasValue)
                {
                    TransHeight = pos.TransHeight.Value;
                }
                
            }
        }
    }
}
