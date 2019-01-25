﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace Arctron.Obj2Gltf.WaveFront
{
    /// <summary>
    ///  represents an obj file model
    /// </summary>
    public class ObjModel
    {
        public string Name { get; set; }
        /// <summary>
        /// obj used mat file path
        /// </summary>
        public string MatFilename { get; set; }
        /// <summary>
        /// vertices coordinates list
        /// </summary>
        public List<Vec3> Vertices { get; set; } = new List<Vec3>();
        /// <summary>
        /// vertices normal list
        /// </summary>
        public List<Vec3> Normals { get; set; } = new List<Vec3>();
        /// <summary>
        /// vertices texture coordinates list
        /// </summary>
        public List<Vec2> Uvs { get; set; } = new List<Vec2>();
        /// <summary>
        /// grouped geometries
        /// </summary>
        public List<Geometry> Geometries { get; set; } = new List<Geometry>();
        /// <summary>
        /// mat list from mat file
        /// </summary>
        public List<Material> Materials { get; set; } = new List<Material>();
        /// <summary>
        /// write obj file
        /// </summary>
        /// <param name="writer"></param>
        public void Write(StreamWriter writer)
        {
            writer.WriteLine("# File generated by Arctron BIMClient");
            if (!String.IsNullOrEmpty(MatFilename))
            {
                writer.WriteLine($"mtllib {MatFilename}");
            }
            var vs = String.Join(Environment.NewLine, Vertices.Select(v => $"v {v.X} {v.Y} {v.Z}"));
            writer.WriteLine(vs);
            writer.Flush();
            var ts = String.Join(Environment.NewLine, Uvs.Select(t => $"vt {t.U} {t.V}"));
            writer.WriteLine(ts);
            writer.Flush();
            var ns = String.Join(Environment.NewLine, Normals.Select(n => $"vn {n.X} {n.Y} {n.Z}"));
            writer.WriteLine(ns);
            writer.Flush();
            foreach (var g in Geometries)
            {
                g.Write(writer);
            }
        }

        public List<ObjModel> Split(int level)
        {
            if (level <= 1)
            {
                return new List<ObjModel> { this };
            }
            var box = GetBounding();
            var boxes = box.Split(level);
            var geoes = new List<Geometry>[boxes.Count];
            var pnts = new List<int>[boxes.Count];
            var normals = new List<int>[boxes.Count];
            var uvs = new List<int>[boxes.Count];
            for(var i = 0;i<geoes.Length;i++)
            {
                geoes[i] = new List<Geometry>();
                pnts[i] = new List<int>();
                normals[i] = new List<int>();
                uvs[i] = new List<int>();
            }
            foreach(var g in Geometries)
            {
                var index = GetBoxIndex(g, boxes);
                var gg = AddGeo(g, pnts[index], normals[index], uvs[index]);
                geoes[index].Add(gg);
            }
            var objModels = new List<ObjModel>();
            for(var i = 0;i< geoes.Length;i++)
            {
                if (geoes[i].Count == 0) continue;
                var m = new ObjModel { Geometries = geoes[i], Name = Name + "_" + objModels.Count, MatFilename = MatFilename, Materials = Materials };
                if (m.Vertices == null) m.Vertices = new List<Vec3>();
                var ps = pnts[i];
                foreach(var v in ps)
                {
                    m.Vertices.Add(Vertices[v - 1]);
                }
                if (m.Normals == null) m.Normals = new List<Vec3>();
                var ns = normals[i];
                foreach(var n in ns)
                {
                    m.Normals.Add(Normals[n - 1]);
                }
                if (m.Uvs == null) m.Uvs = new List<Vec2>();
                var ts = uvs[i];
                foreach(var t in ts)
                {
                    m.Uvs.Add(Uvs[t - 1]);
                }
                objModels.Add(m);
            }
            return objModels;
        }

        private static FaceVertex GetVertex(FaceVertex v, List<int> pnts, List<int> normals, List<int> uvs)
        {
            var v1p = v.V;
            var v1n = v.N;
            var v1t = v.T;
            if (v1p > 0)
            {
                var index = pnts.IndexOf(v1p);
                if (index == -1)
                {
                    index = pnts.Count;
                    pnts.Add(v1p);
                }
                v1p = index+1;
            }
            if (v1n > 0)
            {
                var index = normals.IndexOf(v1n);
                if (index == -1)
                {
                    index = normals.Count;
                    normals.Add(v1n);
                }
                v1n = index+1;
            }
            if (v1t > 0)
            {
                var index = uvs.IndexOf(v1t);
                if (index == -1)
                {
                    index = uvs.Count;
                    uvs.Add(v1t);
                }
                v1t = index+1;
            }
            return new FaceVertex(v1p, v1t, v1n);
        }

        private Geometry AddGeo(Geometry g, List<int> pnts, List<int> normals, List<int> uvs)
        {
            var gg = new Geometry { Id = g.Id };
            foreach(var f in g.Faces)
            {
                var ff = new Face { MatName = f.MatName };
                foreach(var t in f.Triangles)
                {
                    var v1 = GetVertex(t.V1, pnts, normals, uvs);
                    var v2 = GetVertex(t.V2, pnts, normals, uvs);
                    var v3 = GetVertex(t.V3, pnts, normals, uvs);
                    var fv = new FaceTriangle(v1, v2, v3);
                    ff.Triangles.Add(fv);
                }

                gg.Faces.Add(ff);
            }

            return gg;
        }

        private int GetBoxIndex(Geometry g, IList<BoundingBox> boxes)
        {
            var gCenter = GetCenter(g);
            for(var i = 0;i<boxes.Count;i++)
            {
                if (boxes[i].IsIn(gCenter))
                {
                    return i;
                }
            }
            return -1;
        }

        private Vec3 GetCenter(Geometry g)
        {
            var ps = new List<int>();
            foreach(var f in g.Faces)
            {
                foreach(var t in f.Triangles)
                {
                    if (!ps.Contains(t.V1.V))
                    {
                        ps.Add(t.V1.V);
                    }
                    if (!ps.Contains(t.V2.V))
                    {
                        ps.Add(t.V2.V);
                    }
                    if (!ps.Contains(t.V3.V))
                    {
                        ps.Add(t.V3.V);
                    }
                }
            }
            var x = ps.Select(c => Vertices[c - 1].X).Average();
            var y = ps.Select(c => Vertices[c - 1].Y).Average();
            var z = ps.Select(c => Vertices[c - 1].Z).Average();
            return new Vec3(x, y, z);
        }

        public BoundingBox GetBounding()
        {
            var box = new BoundingBox();
            foreach(var v in Vertices)
            {
                var x = v.X;                
                if (box.X.Min > x)
                {
                    box.X.Min = x;
                } else if (box.X.Max < x)
                {
                    box.X.Max = x;
                }
                var y = v.Y;
                if (box.Y.Min > y)
                {
                    box.Y.Min = y;
                } else if (box.Y.Max < y)
                {
                    box.Y.Max = y;
                }
                var z = v.Z;
                if (box.Z.Min > z)
                {
                    box.Z.Min = z;
                } else if (box.Z.Max < z)
                {
                    box.Z.Max = z;
                }
            }
            return box;
        }
    }

    /// <summary>
    /// geometry with face meshes
    /// http://paulbourke.net/dataformats/obj/
    /// http://www.fileformat.info/format/wavefrontobj/egff.htm
    /// </summary>
    public class Geometry
    {
        /// <summary>
        /// group name
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// meshes
        /// </summary>
        public List<Face> Faces { get; set; } = new List<Face>();
        /// <summary>
        /// write geometry
        /// </summary>
        /// <param name="writer"></param>
        public void Write(StreamWriter writer)
        {
            writer.WriteLine($"g {Id}");
            writer.WriteLine($"s off");
            foreach (var f in Faces)
            {                
                f.Write(writer);
            }
        }
    }
}
