using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Arctron.Obj2Gltf.Geom;

namespace Arctron.Obj2Gltf.WaveFront
{
    /// <summary>
    /// parse obj file with mat file
    /// </summary>
    public class ObjParser : IDisposable
    {
        private readonly string _objFile;

        private readonly StreamReader _reader;
        private ObjModel _model = null;
        private readonly Encoding _encoding;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="objFile">obj file path</param>
        public ObjParser(string objFile):this(objFile, MtlParser.InitEncoding(objFile))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objFile">obj file path</param>
        /// <param name="encoding"></param>
        public ObjParser(string objFile, Encoding encoding)
        {
            _objFile = objFile;
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            _encoding = encoding;
            _reader = new StreamReader(objFile, _encoding);
        }

        private Geometry GetGeometry()
        {
            Geometry g = null;
            if (_model.Geometries.Count == 0)
            {
                g = new Geometry { Id = "default" };
                _model.Geometries.Add(g);
            }
            else
            {
                g = _model.Geometries[_model.Geometries.Count - 1];
            }
            return g;
        }

        private FaceVertex GetVertex(string vStr)
        {
            var v1Str = vStr.Split('/');
            if (v1Str.Length >= 3)
            {
                var v = int.Parse(v1Str[0]);
                var t = 0;
                if (!String.IsNullOrEmpty(v1Str[1]))
                {
                    t = int.Parse(v1Str[1]);
                }
                var n = int.Parse(v1Str[2]);
                return new FaceVertex(v, t, n);
            }
            else if (v1Str.Length >= 2)
            {
                return new FaceVertex(int.Parse(v1Str[0]), int.Parse(v1Str[1]), 0);
            }
            return new FaceVertex(int.Parse(v1Str[0]));
        }

        private Face GetFace(Geometry g)
        {
            Face face = null;
            if (g.Faces.Count > 0)
            {
                face = g.Faces[g.Faces.Count - 1];
            }
            else
            {
                face = new Face();
                g.Faces.Add(face);
            }
            return face;
        }

        private static string[] SplitLine(string line)
        {
            return line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
        }

        private static bool StartWith(string line, string str)
        {
            return line.StartsWith(str+" ") || line.StartsWith(str+"\t");
        }
        /// <summary>
        /// get parsed obj model
        /// </summary>
        /// <returns></returns>
        public ObjModel GetModel()
        {
            if(_model == null)
            {
                var modelName = "Untitled";
                if (!String.IsNullOrEmpty(_objFile))
                {
                    modelName = Path.GetFileNameWithoutExtension(_objFile);
                }
                _model = new ObjModel { Name = modelName };                
                while(!_reader.EndOfStream)
                {
                    var line = _reader.ReadLine().Trim();
                    if (String.IsNullOrEmpty(line)) continue;
                    if (line.StartsWith("#")) continue;
                    if (StartWith(line, "mtllib"))
                    {
                        _model.MatFilename = line.Substring(6).Trim();
                    }
                    else if (StartWith(line, "v"))
                    {
                        var vStr = line.Substring(1).Trim();
                        var strs = SplitLine(vStr);
                        var v = new Vec3(double.Parse(strs[0]), double.Parse(strs[1]), double.Parse(strs[2]));
                        _model.Vertices.Add(v);
                    }
                    else if (StartWith(line, "vn"))
                    {
                        var vnStr = line.Substring(2).Trim();
                        var strs = SplitLine(vnStr);
                        var vn = new Vec3(double.Parse(strs[0]), double.Parse(strs[1]), double.Parse(strs[2]));
                        _model.Normals.Add(vn);
                    }
                    else if (StartWith(line, "vt"))
                    {
                        var vtStr = line.Substring(2).Trim();
                        var strs = SplitLine(vtStr);
                        var vt = new Vec2(double.Parse(strs[0]), double.Parse(strs[1]));
                        _model.Uvs.Add(vt);
                    }
                    else if (StartWith(line, "g"))
                    {
                        var gStr = line.Substring(1).Trim();
                        var g = new Geometry { Id = gStr };
                        _model.Geometries.Add(g);
                    }
                    else if (StartWith(line, "usemtl"))
                    {
                        var umtl = line.Substring(6).Trim();
                        var g = GetGeometry();
                        var face = new Face { MatName = umtl };
                        g.Faces.Add(face);
                    }
                    else if (StartWith(line, "f"))
                    {
                        var fStr = line.Substring(1).Trim();
                        var g = GetGeometry();
                        Face face = GetFace(g);
                        var strs = SplitLine(fStr);
                        if (strs.Length < 3) continue; // ignore face that has less than 3 vertices
                        if (strs.Length == 3)
                        {
                            var v1 = GetVertex(strs[0]);
                            var v2 = GetVertex(strs[1]);
                            var v3 = GetVertex(strs[2]);
                            var f = new FaceTriangle(v1, v2, v3);
                            face.Triangles.Add(f);
                        }
                        else if (strs.Length == 4)
                        {
                            var v1 = GetVertex(strs[0]);
                            var v2 = GetVertex(strs[1]);
                            var v3 = GetVertex(strs[2]);
                            var f = new FaceTriangle(v1, v2, v3);
                            face.Triangles.Add(f);
                            var v4 = GetVertex(strs[3]);
                            var ff = new FaceTriangle(v1, v3, v4);
                            face.Triangles.Add(ff);
                        }
                        else //if (strs.Length > 4)
                        {
                            var points = new List<Vec3>();
                            for(var i = 0;i<strs.Length;i++)
                            {
                                var vv = GetVertex(strs[i]);
                                var p = _model.Vertices[vv.V-1];
                                points.Add(p);
                            }
                            var planeAxis = GeomUtil.ComputeProjectTo2DArguments(points);
                            if (planeAxis != null)
                            {
                                var points2D = GeomUtil.CreateProjectPointsTo2DFunction(planeAxis, points);
                                var indices = PolygonPipeline.Triangulate(points2D, null);
                                if (indices.Length == 0)
                                {
                                    // TODO:
                                }
                                for(var i = 0; i < indices.Length-2;i+=3)
                                {
                                    var vv1 = GetVertex(strs[indices[i]]);
                                    var vv2 = GetVertex(strs[indices[i + 1]]);
                                    var vv3 = GetVertex(strs[indices[i + 2]]);
                                    var ff = new FaceTriangle(vv1, vv2, vv3);
                                    face.Triangles.Add(ff);
                                }
                            }
                            else
                            {
                                // TODO:
                            }
                        }
                    }
                    else
                    {
                        //var strs = SplitLine(line);
                    }
                }
                if (!String.IsNullOrEmpty(_model.MatFilename))
                {
                    var dir = Path.GetDirectoryName(_objFile);
                    var matFile = Path.Combine(dir, _model.MatFilename);
                    using (var mtlParser = new MtlParser(matFile, _encoding))
                    {
                        var mats = mtlParser.GetMats();
                        _model.Materials.AddRange(mats);
                    }
                        
                }
            }
            return _model;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
            }
        }
    }
}
