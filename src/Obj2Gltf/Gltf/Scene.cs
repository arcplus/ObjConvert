using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    public class Scene
    {
        [JsonProperty("nodes")]
        public List<int> Nodes { get; set; } = new List<int>();
    }
    //TODO:
    public class Node
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("mesh")]
        public int? Mesh { get; set; }
    }
}
