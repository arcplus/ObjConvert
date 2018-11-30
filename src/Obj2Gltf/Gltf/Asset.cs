using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    /// <summary>
    /// Metadata about the glTF asset.
    /// </summary>
    public class Asset
    {
        /// <summary>
        /// Tool that generated this glTF model.
        /// </summary>
        [JsonProperty("generator")]
        public string Generator { get; set; }
        /// <summary>
        /// The glTF version of this asset.
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
