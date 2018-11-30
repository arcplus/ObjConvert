using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Arctron.Gltf
{
    /// <summary>
    /// The root `Node`s of a scene.
    /// </summary>
    public class Scene
    {
        /// <summary>
        /// The indices of each root node.
        /// </summary>
        [JsonProperty("nodes")]
        public List<int> Nodes { get; set; } = new List<int>();
    }
    /// <summary>
    /// A node in the node hierarchy.  When the node contains `skin`, all
    /// `mesh.primitives` must contain `JOINTS_0` and `WEIGHTS_0` attributes.
    /// A node can have either a `matrix` or any combination of
    /// `translation`/`rotation`/`scale` (TRS) properties. TRS properties are converted
    /// to matrices and postmultiplied in the `T * R * S` order to compose the
    /// transformation matrix; first the scale is applied to the vertices, then the
    /// rotation, and then the translation. If none are provided, the transform is the
    /// identity. When a node is targeted for animation (referenced by an
    /// animation.channel.target), only TRS properties may be present; `matrix` will not
    /// be present.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Optional user-defined name for this object.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }
        /// <summary>
        /// The index of the mesh in this node.
        /// </summary>
        [JsonProperty("mesh")]
        public int? Mesh { get; set; }
    }
}
