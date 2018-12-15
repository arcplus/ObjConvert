using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Arctron.Gltf
{
    /// <summary>
    /// The alpha rendering mode of a material.
    /// </summary>
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
    /// <summary>
    /// The material appearance of a primitive.
    /// </summary>
    public class Material
    {
        /// <summary>
        /// Optional user-defined name for this object.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// A set of parameter values that are used to define the metallic-roughness
        /// material model from Physically-Based Rendering (PBR) methodology. When not
        /// specified, all the default values of `pbrMetallicRoughness` apply.
        /// </summary>
        [JsonProperty("pbrMetallicRoughness")]
        public PbrMetallicRoughness PbrMetallicRoughness { get; set; }
        /// <summary>
        /// The emissive color of the material.
        /// </summary>
        [JsonProperty("emissiveFactor")]
        public double[] EmissiveFactor { get; set; } // = new double[] { 0, 0, 0 };
        /// <summary>
        /// The alpha rendering mode of the material.
        ///
        /// The material's alpha rendering mode enumeration specifying the
        /// interpretation of the alpha value of the main factor and texture.
        ///
        /// * In `Opaque` mode (default) the alpha value is ignored and the rendered
        ///   output is fully opaque.
        ///
        /// * In `Mask` mode, the rendered output is either fully opaque or fully
        ///   transparent depending on the alpha value and the specified alpha cutoff
        ///   value.
        ///
        /// * In `Blend` mode, the alpha value is used to composite the source and
        ///   destination areas and the rendered output is combined with the
        ///   background using the normal painting operation (i.e. the Porter and
        ///   Duff over operator).
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("alphaMode")]
        public AlphaMode AlphaMode { get; set; }
        /// <summary>
        /// Specifies whether the material is double-sided.
        ///
        /// * When this value is false, back-face culling is enabled.
        ///
        /// * When this value is true, back-face culling is disabled and double sided
        ///   lighting is enabled.
        ///
        /// The back-face must have its normals reversed before the lighting
        /// equation is evaluated.
        /// </summary>
        [JsonProperty("doubleSided")]
        public bool DoubleSided { get; set; }
    }
    /// <summary>
    /// A set of parameter values that are used to define the metallic-roughness
    /// material model from Physically-Based Rendering (PBR) methodology.
    /// </summary>
    public class PbrMetallicRoughness
    {
        /// <summary>
        /// The material's base color factor.
        /// </summary>
        [JsonProperty("baseColorFactor")]
        public double[] BaseColorFactor { get; set; } = new double[] { 1, 1, 1, 1 };
        /// <summary>
        /// The base color texture.
        /// </summary>
        [JsonProperty("baseColorTexture")]
        public Info BaseColorTexture { get; set; }
        /// <summary>
        /// The metalness of the material.
        /// </summary>
        [JsonProperty("metallicFactor")]
        public double MetallicFactor { get; set; } = 1.0;
        /// The roughness of the material.
        ///
        /// * A value of 1.0 means the material is completely rough.
        /// * A value of 0.0 means the material is completely smooth.
        [JsonProperty("roughnessFactor")]
        public double RoughnessFactor { get; set; } = 0.9;
    }
}
