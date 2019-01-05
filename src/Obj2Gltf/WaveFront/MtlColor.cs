using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

//http://paulbourke.net/dataformats/mtl/

namespace Arctron.Obj2Gltf.WaveFront
{
    
    /// <summary>
    /// mtl material
    /// </summary>
    public class Material
    {
        /// <summary>
        ///  matname
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Ka: Ambient Color
        /// </summary>
        public Reflectivity Ambient { get; set; } = new Reflectivity(new Color());
        /// <summary>
        /// Kd: Diffuse Color
        /// </summary>
        public Reflectivity Diffuse { get; set; } = new Reflectivity(new Color(0.5f));
        /// <summary>
        /// map_Kd: Diffuse texture file path
        /// </summary>
        public string DiffuseTextureFile { get; set; }
        /// <summary>
        /// map_Ka: Ambient texture file path
        /// </summary>
        public string AmbientTextureFile { get; set; }
        /// <summary>
        /// Ks: specular reflectivity of the current material
        /// </summary>
        public Reflectivity Specular { get; set; } = new Reflectivity(new Color());
        /// <summary>
        /// Tf: transmission filter: Any light passing through the object 
        /// is filtered by the transmission filter
        /// </summary>
        public Reflectivity Filter { get; set; }
        /// <summary>
        /// Ke: emissive color
        /// </summary>
        public Reflectivity Emissive { get; set; } = new Reflectivity(new Color());
        /// <summary>
        /// illum: illum_# 0 ~ 10
        /// </summary>
        public int? Illumination { get; set; }
        /// <summary>
        /// d: the dissolve for the current material.
        /// </summary>
        public Dissolve Dissolve { get; set; }
        /// <summary>
        /// Tr: Transparency
        /// </summary>
        public Transparency Transparency { get; set; }
        /// <summary>
        /// Ns: specularShininess 0 ~ 1000
        /// </summary>
        public double SpecularExponent { get; set; }
        /// <summary>
        /// sharpness value 0 ~ 1000, The default is 60
        /// </summary>
        public int? Sharpness { get; set; }
        /// <summary>
        /// 0.001 ~ 10
        /// </summary>
        public double? OpticalDensity { get; set; }

        public double GetAlpha()
        {
            if (Dissolve != null)
            {
                return Dissolve.Factor;                
            }
            if (Transparency != null)
            {
                return (1.0 - Transparency.Factor);
            }
            return 1.0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"newmtl {Name}" + Environment.NewLine);
            if (Ambient != null)
            {
                sb.Append($"Ka {Diffuse}" + Environment.NewLine);
            }
            if (Diffuse != null)
            {
                sb.Append($"Kd {Diffuse}" + Environment.NewLine);
            }
            if (Specular != null)
            {
                sb.Append($"Ks {Specular}" + Environment.NewLine);
            }
            if (Emissive != null)
            {
                sb.Append($"Ke {Emissive}" + Environment.NewLine);
            }
            if (Dissolve != null && Dissolve.Factor < 1)
            {
                sb.Append(Dissolve + Environment.NewLine);
            }
            if (SpecularExponent > 0)
            {
                sb.Append($"Ns {SpecularExponent}" + Environment.NewLine);
            }
            if (Sharpness != null)
            {
                sb.Append($"sharpness {Sharpness}" + Environment.NewLine);
            }
            if (Filter != null)
            {
                sb.Append($"Tf {Filter}" + Environment.NewLine);
            }
            if (!String.IsNullOrEmpty(AmbientTextureFile))
            {
                sb.Append($"map_Ka {DiffuseTextureFile}" + Environment.NewLine);
            }
            if (!String.IsNullOrEmpty(DiffuseTextureFile))
            {
                sb.Append($"map_Kd {DiffuseTextureFile}" + Environment.NewLine);
            }
            return sb.ToString();
        }
    }

    public class Reflectivity
    {
        public Color Color { get; }

        public Spectral Spectral { get; }

        public XYZ XYZ { get; }

        public ReflectivityType AmbientType { get; }

        public Reflectivity(Color color)
        {
            AmbientType = ReflectivityType.Color;
            Color = color;
        }

        public Reflectivity(Spectral spectral)
        {
            AmbientType = ReflectivityType.Spectral;
            Spectral = spectral;
        }

        public Reflectivity(XYZ xyz)
        {
            AmbientType = ReflectivityType.XYZ;
            XYZ = xyz;
        }

        public override string ToString()
        {
            switch (AmbientType)
            {
                case ReflectivityType.Color:
                    return Color.ToString();
                default: // TODO:
                    return String.Empty;
            }
        }
    }

    public enum ReflectivityType
    {
        Color,
        Spectral,
        XYZ
    }

    public class Color
    {
        public Color()
        {

        }
        public Color(double v)
        {
            Red = v;
            Green = v;
            Blue = v;
        }

        public Color(double r, double g, double b)
        {
            Red = r;
            Green = g;
            Blue = b;
        }
        public double Red { get; set; }

        public double Green { get; set; }

        public double Blue { get; set; }

        public override string ToString()
        {
            return $"{Red:0.0000} {Green:0.0000} {Blue:0.0000}";
        }

        public double[] ToArray(double? alpha=null)
        {
            var max = new double[] { Red, Green, Blue }.Max();
            if (max > 1)
            {
                Red /= max;
                Green /= max;
                Blue /= max;
            }
            if (alpha == null) return new double[] { Red, Green, Blue };
            return new double[] { Red, Green, Blue, alpha.Value };
        }

    }

    public class Spectral
    {
        public string Filename { get; set; }

        public int Factor { get; set; }
    }

    public class XYZ
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }
    }

    public class Transparency
    {
        public double Factor { get; set; }
    }
    /// <summary>
    /// 1- transparency
    /// "factor" is the amount this material dissolves into the background.  A 
    ///  factor of 1.0 is fully opaque.This is the default when a new material 
    /// is created.A factor of 0.0 is fully dissolved(completely
    /// transparent).
    /// </summary>
    public class Dissolve
    {
        /// <summary>
        /// A factor of 1.0 is fully opaque.
        /// </summary>
        public double Factor { get; set; }
        /// <summary>
        /// d -halo 0.0, will be fully dissolved at its center and will 
        /// appear gradually more opaque toward its edge.
        /// </summary>
        public bool Halo { get; set; }

        public override string ToString()
        {
            if (!Halo)
            {
                return $"d {Factor}";
            }
            return $"d -halo {Factor}";
        }
    }
}
