using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    /// <summary>
    /// A texture and its sampler.
    /// </summary>
    public class Texture
    {
        /// <summary>
        /// Optional user-defined name for this object.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// The index of the sampler used by this texture.
        /// </summary>
        [JsonProperty("sampler")]
        public int Sampler { get; set; }
        /// <summary>
        /// The index of the image used by this texture.
        /// </summary>
        [JsonProperty("source")]
        public int Source { get; set; }
    }
    /// <summary>
    /// Reference to a `Texture`.
    /// </summary>
    public class Info
    {
        /// <summary>
        /// The index of the texture.
        /// </summary>
        [JsonProperty("index")]
        public int Index { get; set; }
    }
    /// <summary>
    /// Minification filter.
    /// </summary>
    public enum MinFilter
    {
        /// Corresponds to `GL_NEAREST`.
        Nearest = 9728,

        /// Corresponds to `GL_LINEAR`.
        Linear,

        /// Corresponds to `GL_NEAREST_MIPMAP_NEAREST`.
        NearestMipmapNearest = 9984,

        /// Corresponds to `GL_LINEAR_MIPMAP_NEAREST`.
        LinearMipmapNearest,

        /// Corresponds to `GL_NEAREST_MIPMAP_LINEAR`.
        NearestMipmapLinear,

        /// Corresponds to `GL_LINEAR_MIPMAP_LINEAR`.
        LinearMipmapLinear,
    }
    /// <summary>
    /// Magnification filter.
    /// </summary>
    public enum MagFilter
    {
        /// Corresponds to `GL_NEAREST`.
        Nearest = 9728,

        /// Corresponds to `GL_LINEAR`.
        Linear,
    }
    /// <summary>
    /// Texture sampler properties for filtering and wrapping modes.
    /// </summary>
    public class Sampler
    {
        /// <summary>
        /// Optional user-defined name for this object.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// Magnification filter.
        /// </summary>
        [JsonProperty("magFilter")]
        public MagFilter MagFilter { get; set; }
        /// <summary>
        /// Minification filter.
        /// </summary>
        [JsonProperty("minFilter")]
        public MinFilter MinFilter { get; set; }
        /// <summary>
        /// `s` wrapping mode.
        /// </summary>
        [JsonProperty("wrapS")]
        public WrappingMode WrapS { get; set; }
        /// <summary>
        /// `t` wrapping mode.
        /// </summary>
        [JsonProperty("wrapT")]
        public WrappingMode WrapT { get; set; }
    }
    /// <summary>
    /// Texture co-ordinate wrapping mode.
    /// </summary>
    public enum WrappingMode
    {
        /// Corresponds to `GL_CLAMP_TO_EDGE`.
        ClampToEdge = 33071,

        /// Corresponds to `GL_MIRRORED_REPEAT`.
        MirroredRepeat = 33648,

        /// Corresponds to `GL_REPEAT`.
        Repeat = 10497,
    }
}
