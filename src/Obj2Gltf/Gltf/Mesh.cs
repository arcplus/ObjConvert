using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    public enum Mode
    {
        /// Corresponds to `GL_POINTS`.
        Points = 0,

        /// Corresponds to `GL_LINES`.
        Lines,

        /// Corresponds to `GL_LINE_LOOP`.
        LineLoop,

        /// Corresponds to `GL_LINE_STRIP`.
        LineStrip,

        /// Corresponds to `GL_TRIANGLES`.
        Triangles,

        /// Corresponds to `GL_TRIANGLE_STRIP`.
        TriangleStrip,

        /// Corresponds to `GL_TRIANGLE_FAN`.
        TriangleFan,
    }
    public class Mesh
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("primitives")]
        public List<Primitive> Primitives { get; set; } = new List<Primitive>();
        
    }

    public class Primitive
    {
        [JsonProperty("attributes")]
        public Dictionary<string, int> Attributes { get; set; }
        [JsonProperty("indices")]
        public int Indices { get; set; }
        [JsonProperty("material")]
        public int Material { get; set; }
        [JsonProperty("mode")]
        public Mode Mode { get; set; }
    }
}
