using System;
using System.Collections.Generic;
using System.Text;

namespace Arctron.Obj2Gltf
{
    public class MeshAttribute
    {
        public Dictionary<string, int> Attributes { get; } = new Dictionary<string, int>();

        public List<int[]> Indices { get; } = new List<int[]>();
    }
}
