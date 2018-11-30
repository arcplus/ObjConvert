using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    /// <summary>
    /// The type of primitives to render.
    /// </summary>
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
    /// <summary>
    /// A set of primitives to be rendered.
    ///
    /// A node can contain one or more meshes and its transform places the meshes in
    /// the scene.
    /// </summary>
    public class Mesh
    {
        /// <summary>
        /// Optional user-defined name for this object.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// Defines the geometry to be renderered with a material.
        /// </summary>
        [JsonProperty("primitives")]
        public List<Primitive> Primitives { get; set; } = new List<Primitive>();
        
    }
    /// <summary>
    /// Geometry to be rendered with the given material.
    /// </summary>
    public class Primitive
    {
        /// <summary>
        /// Maps attribute semantic names to the `Accessor`s containing the
        /// corresponding attribute data.
        /// </summary>
        [JsonProperty("attributes")]
        public Dictionary<string, int> Attributes { get; set; }
        /// <summary>
        /// The index of the accessor that contains the indices.
        /// </summary>
        [JsonProperty("indices")]
        public int Indices { get; set; }
        /// <summary>
        /// The index of the material to apply to this primitive when rendering
        /// </summary>
        [JsonProperty("material")]
        public int Material { get; set; }
        /// <summary>
        /// The type of primitives to render.
        /// </summary>
        [JsonProperty("mode")]
        public Mode Mode { get; set; }
    }
}
