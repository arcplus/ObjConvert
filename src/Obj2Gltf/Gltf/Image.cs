using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    public class Image
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mimeType")]
        public string MimeType { get; set; }
        [JsonProperty("bufferView")]
        public int BufferView { get; set; }
    }
}
