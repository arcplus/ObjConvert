using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    public class Buffer
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("byteLength")]
        public int ByteLength { get; set; }
        /// <summary>
        /// Data URL
        /// </summary>
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }

    public class BufferView
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("buffer")]
        public int Buffer { get; set; }
        [JsonProperty("byteLength")]
        public int ByteLength { get; set; }
        [JsonProperty("byteOffset")]
        public int ByteOffset { get; set; }
        [JsonProperty("byteStride")]
        public int? ByteStride { get; set; }
        [JsonProperty("target")]
        public int? Target { get; set; }
    }
}
