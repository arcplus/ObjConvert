using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    public class Texture
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("sampler")]
        public int Sampler { get; set; }
        [JsonProperty("source")]
        public int Source { get; set; }
    }

    public class Info
    {
        /// <summary>
        /// The index of the texture.
        /// </summary>
        [JsonProperty("index")]
        public int Index { get; set; }
    }

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

    public enum MagFilter
    {
        /// Corresponds to `GL_NEAREST`.
        Nearest = 9728,

        /// Corresponds to `GL_LINEAR`.
        Linear,
    }

    public class Sampler
    {
        [JsonProperty("magFilter")]
        public MagFilter MagFilter { get; set; }
        [JsonProperty("minFilter")]
        public MinFilter MinFilter { get; set; }
        [JsonProperty("wrapS")]
        public WrappingMode WrapS { get; set; }
        [JsonProperty("wrapT")]
        public WrappingMode WrapT { get; set; }
    }

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
