using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Arctron.Obj2Gltf.WaveFront
{
    /// <summary>
    /// face with material
    /// </summary>
    public class Face
    {
        /// <summary>
        /// face used material name
        /// </summary>
        public string MatName { get; set; } = String.Empty;
        /// <summary>
        /// face meshes
        /// </summary>
        public List<FaceTriangle> Triangles { get; set; } = new List<FaceTriangle>();
        /// <summary>
        /// write face info into obj file writer
        /// </summary>
        /// <param name="writer"></param>
        public void Write(StreamWriter writer)
        {
            if (!String.IsNullOrEmpty(MatName))
            {
                writer.WriteLine($"usemtl {MatName}");
            }

            var contents = String.Join(Environment.NewLine, Triangles);
            writer.WriteLine(contents);
            writer.Flush();
        }
    }
    /// <summary>
    /// represents a triangle
    /// </summary>
    public struct FaceTriangle
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="v3"></param>
        public FaceTriangle(FaceVertex v1, FaceVertex v2, FaceVertex v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }
        /// <summary>
        /// The first vertex
        /// </summary>
        public FaceVertex V1;
        /// <summary>
        /// The second vertex
        /// </summary>
        public FaceVertex V2;
        /// <summary>
        /// The third vertex
        /// </summary>

        public FaceVertex V3;

        public override string ToString()
        {
            return $"f {V1} {V2} {V3}";
        }
    }
    /// <summary>
    /// represents a vertex on a face
    /// </summary>
    public struct FaceVertex : IEquatable<FaceVertex>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v">vertex coordinates index</param>
        public FaceVertex(int v) : this(v, 0, 0) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v">vertex coordinates index</param>
        /// <param name="n">vertex normal index</param>
        public FaceVertex(int v, int n) : this(v, 0, n) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v">vertex coordinates index</param>
        /// <param name="t">vertex texture coordinates index</param>
        /// <param name="n">vertex normal index</param>
        public FaceVertex(int v, int t, int n)
        {
            V = v;
            N = n;
            T = t;
        }
        /// <summary>
        /// vertex coordinates index
        /// </summary>
        public int V;
        /// <summary>
        /// vertex texture coordinates index
        /// </summary>
        public int T;
        /// <summary>
        /// vertex normal index
        /// </summary>
        public int N;

        public override string ToString()
        {
            if (N > 0)
            {
                if (T > 0)
                {
                    return $"{V}/{T}/{N}";
                }
                else
                {
                    return $"{V}//{N}";
                }
            }
            if (T > 0)
            {
                return $"{V}/{T}";
            }
            return $"{V}";
            
        }

        public bool Equals(FaceVertex other)
        {
            return V == other.V && T == other.T && N == other.N;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != typeof(FaceVertex)) return false;
            return Equals((FaceVertex)obj);
        }

        public override int GetHashCode()
        {
            return V ^ T ^ N;
        }
    }
}
