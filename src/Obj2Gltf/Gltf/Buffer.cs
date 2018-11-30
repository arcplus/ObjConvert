using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    /// <summary>
    /// A buffer points to binary data representing geometry, animations, or skins.
    /// </summary>
    public class Buffer
    {
        /// <summary>
        /// Optional user-defined name for this object.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// The length of the buffer in bytes.
        /// </summary>
        [JsonProperty("byteLength")]
        public int ByteLength { get; set; }
        /// <summary>
        /// The uri of the buffer.  Relative paths are relative to the .gltf file.
        /// Instead of referencing an external file, the uri can also be a data-uri.
        /// </summary>
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
    /// <summary>
    /// A view into a buffer generally representing a subset of the buffer.
    /// https://github.com/KhronosGroup/glTF/tree/master/specification/2.0#reference-bufferview
    /// </summary>
    public class BufferView
    {
        /// <summary>
        /// Optional user-defined name for this object.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// The parent `Buffer`.
        /// </summary>
        [JsonProperty("buffer")]
        public int Buffer { get; set; }
        /// <summary>
        /// The length of the `BufferView` in bytes.
        /// </summary>
        [JsonProperty("byteLength")]
        public int ByteLength { get; set; }
        /// <summary>
        /// Offset into the parent buffer in bytes.
        /// </summary>
        [JsonProperty("byteOffset")]
        public int ByteOffset { get; set; }
        /// <summary>
        /// The stride in bytes between vertex attributes or other interleavable data.
        /// When zero, data is assumed to be tightly packed.
        /// </summary>
        [JsonProperty("byteStride")]
        public int? ByteStride { get; set; }
        /// <summary>
        /// Optional target the buffer should be bound to.
        /// </summary>
        [JsonProperty("target")]
        public int? Target { get; set; }
    }
}
