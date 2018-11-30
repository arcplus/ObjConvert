using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Arctron.Obj2Gltf.WaveFront
{
    public class ObjParser : IDisposable
    {
        private readonly string _objFile;

        private readonly StreamReader _reader;
        private ObjModel _model = null;
        

        public ObjParser(string objFile)
        {
            _objFile = objFile;
            _reader = new StreamReader(objFile, Encoding.UTF8);
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

        public ObjModel GetModel()
        {
            if(_model == null)
            {
                _model = new ObjModel();                
                while(!_reader.EndOfStream)
                {
                    var line = _reader.ReadLine().Trim();
                    if (line.StartsWith("#")) continue;
                    if (StartWith(line, "mtllib")) //(line.StartsWith("mtllib ") || line.StartsWith("mtllib\t"))
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
                    else if (StartWith(line, "vn"))  //(line.StartsWith("vn ") || line.StartsWith("vn\t"))
                    {
                        var vnStr = line.Substring(2).Trim();
                        var strs = SplitLine(vnStr);
                        var vn = new Vec3(double.Parse(strs[0]), double.Parse(strs[1]), double.Parse(strs[2]));
                        _model.Normals.Add(vn);
                    }
                    else if (StartWith(line, "vt"))  //(line.StartsWith("vt ") || line.StartsWith("vt\t"))
                    {
                        var vtStr = line.Substring(2).Trim();
                        var strs = SplitLine(vtStr);
                        var vt = new Vec2(double.Parse(strs[0]), double.Parse(strs[1]));
                        _model.Uvs.Add(vt);
                    }
                    else if (StartWith(line, "g")) //(line.StartsWith("g ") || line.StartsWith("g\t"))
                    {
                        var gStr = line.Substring(1).Trim();
                        var g = new Geometry { Id = gStr };
                        _model.Geometries.Add(g);
                    }
                    else if (StartWith(line, "usemtl")) //(line.StartsWith("usemtl ") || line.StartsWith("usemtl\t"))
                    {
                        var umtl = line.Substring(6).Trim();
                        var g = GetGeometry();
                        var face = new Face { MatName = umtl };
                        g.Faces.Add(face);
                    }
                    else if (StartWith(line, "f")) //(line.StartsWith("f ") || line.StartsWith("f\t"))
                    {
                        var fStr = line.Substring(1).Trim();
                        var g = GetGeometry();
                        Face face = GetFace(g);
                        var strs = SplitLine(fStr);
                        var v1 = GetVertex(strs[0]);
                        var v2 = GetVertex(strs[1]);
                        var v3 = GetVertex(strs[2]);
                        var f = new FaceTriangle(v1, v2, v3);
                        face.Triangles.Add(f);
                    }
                    else
                    {
                        var strs = SplitLine(line);
                    }
                }
                if (!String.IsNullOrEmpty(_model.MatFilename))
                {
                    var dir = Path.GetDirectoryName(_objFile);
                    var matFile = Path.Combine(dir, _model.MatFilename);
                    using (var mtlParser = new MtlParser(matFile))
                    {
                        var mats = mtlParser.GetMats();
                        _model.Materials.AddRange(mats);
                    }
                        
                }
            }
            return _model;
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
            }
        }
    }
}
