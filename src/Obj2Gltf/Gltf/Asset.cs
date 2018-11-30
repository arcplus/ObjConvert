using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    public class Asset
    {
        [JsonProperty("generator")]
        public string Generator { get; set; }
        [JsonProperty("version")]
        public string Version { get; set; }
    }
}
