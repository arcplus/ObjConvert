using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Arctron.Gltf
{
    public enum AlphaMode
    {
        /// The alpha value is ignored and the rendered output is fully opaque.
        OPAQUE = 1,

        /// The rendered output is either fully opaque or fully transparent depending on
        /// the alpha value and the specified alpha cutoff value.
        MASK,

        /// The rendered output is either fully opaque or fully transparent depending on
        /// the alpha value and the specified alpha cutoff value.
        BLEND,
    }

    public class Material
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("pbrMetallicRoughness")]
        public PbrMetallicRoughness PbrMetallicRoughness { get; set; }
        [JsonProperty("emissiveFactor")]
        public double[] EmissiveFactor = new double[] { 0, 0, 0 };
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("alphaMode")]
        public AlphaMode AlphaMode { get; set; }
        [JsonProperty("doubleSided")]
        public bool DoubleSided { get; set; }
    }

    public class PbrMetallicRoughness
    {
        [JsonProperty("baseColorFactor")]
        public double[] BaseColorFactor { get; set; } = new double[] { 1, 1, 1, 1 };
        [JsonProperty("baseColorTexture")]
        public Info BaseColorTexture { get; set; }
        /// The metalness of the material.
        [JsonProperty("metallicFactor")]
        public double MetallicFactor { get; set; }
        /// The roughness of the material.
        ///
        /// * A value of 1.0 means the material is completely rough.
        /// * A value of 0.0 means the material is completely smooth.
        [JsonProperty("roughnessFactor")]
        public double RoughnessFactor { get; set; }
    }
}
