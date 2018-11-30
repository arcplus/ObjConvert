using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Arctron.Obj2Gltf.WaveFront
{
    public class Face
    {
        public string MatName { get; set; }
        public List<FaceTriangle> Triangles { get; set; } = new List<FaceTriangle>();

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

    public struct FaceTriangle
    {
        public FaceTriangle(FaceVertex v1, FaceVertex v2, FaceVertex v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }
        public FaceVertex V1;

        public FaceVertex V2;

        public FaceVertex V3;

        public override string ToString()
        {
            return $"f {V1} {V2} {V3}";
        }
    }
    /// <summary>
    /// 三角面的顶点设置
    /// </summary>
    public struct FaceVertex : IEquatable<FaceVertex>
    {
        public FaceVertex(int v) : this(v, 0, 0) { }
        public FaceVertex(int v, int n) : this(v, 0, n) { }
        public FaceVertex(int v, int t, int n)
        {
            V = v;
            N = n;
            T = t;
        }
        /// <summary>
        /// 顶点编号
        /// </summary>
        public int V;
        /// <summary>
        /// 纹理坐标编号
        /// </summary>
        public int T;
        /// <summary>
        /// 法向编号
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
