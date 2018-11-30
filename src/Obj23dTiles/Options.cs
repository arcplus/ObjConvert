using System;
using System.Collections.Generic;
using System.Text;
using Arctron.Obj2Gltf;

namespace Arctron.Obj23dTiles
{
    /// <summary>
    /// b3dm converting options
    /// </summary>
    public class Options
    {
        /// <summary>
        /// FeatureTableJson
        /// </summary>
        public List<byte> FeatureTableJson { get; set; } = new List<byte>();
        /// <summary>
        /// FeatureTableBinary
        /// </summary>
        public List<byte> FeatureTableBinary { get; set; } = new List<byte>();
        /// <summary>
        /// BatchTableJson
        /// </summary>
        public List<byte> BatchTableJson { get; set; } = new List<byte>();
        /// <summary>
        /// BatchTableBinary
        /// </summary>
        public List<byte> BatchTableBinary { get; set; } = new List<byte>();
    }
}
