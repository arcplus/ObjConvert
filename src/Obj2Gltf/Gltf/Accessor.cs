using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Arctron.Gltf
{
    public class Accessor
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("componentType")]
        public ComponentType ComponentType { get; set; }
        [JsonProperty("count")]
        public int Count { get; set; }
        [JsonProperty("min")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] Min { get; set; }
        [JsonProperty("max")]
        [JsonConverter(typeof(DoubleArrayJsonConverter))]
        public double[] Max { get; set; }
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public AccessorType Type { get;set; }
        [JsonProperty("bufferView")]
        public int BufferView { get; set; }
        [JsonProperty("byteOffset")]
        public int ByteOffset { get; set; }
    }
}
