using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Arctron.Obj2Gltf;

// https://my.oschina.net/u/1585572/blog/290548

namespace Arctron.Obj23dTiles
{
    /// <summary>
    /// GIS helpers
    /// </summary>
    public static class GisUtil
    {
        public static double LatitudeToMeters(double latDiff)
        {
            return latDiff / 0.000000157891;
        }

        public static double LongitudeToMeters(double lonDiff, double lat)
        {
            return (lonDiff / 0.000000156785) * Math.Cos(lat);
        }
        /// <summary>
        /// Extent
        /// </summary>
        /// <param name="meters"></param>
        /// <param name="latitude">in radian</param>
        /// <returns></returns>
        public static double MetersToLongitude(double meters, double latitude)
        {
            return meters * 0.000000156785 / Math.Cos(latitude);
        }
        /// <summary>
        /// Extent
        /// </summary>
        /// <param name="meters"></param>
        /// <returns></returns>
        public static double MetersToLatituide(double meters)
        {
            return meters * 0.000000157891;
        }

        private static Matrix4 EastNorthUpToFixedFrame(Vec3 origin, Ellipsoid ellipsoid)
        {
            var firstAxis = "east";
            var secondAxis = "north";
            return LocalFrameToFixedFrame(origin, ellipsoid, firstAxis, secondAxis);
        }

        private static Dictionary<string, Dictionary<string, string>> VectorProductLocalFrame
             = new Dictionary<string, Dictionary<string, string>>
             {
                 {"up", new Dictionary<string, string>{
                     { "south", "east"}, {"north", "west"},
                     { "west", "south"}, { "east", "north"} } },
                 { "down", new Dictionary<string, string>{
                     { "south", "west" }, { "north", "east" },
                     { "west", "north" }, { "east", "south" }  } },

                 { "south", new Dictionary<string, string>
                 {
                     { "up", "west" }, { "down", "east"},
                     { "west", "down" }, { "east", "up"}
                 } },
                 { "north", new Dictionary<string, string>
                 {
                     {"up", "east" }, {"down", "west"},
                     { "west", "up" }, { "east", "down" }
                 } },

                 { "west", new Dictionary<string, string>
                 {
                     { "up", "north" }, { "down", "south" },
                     { "north", "down" }, { "south", "up" }
                 } },
                 { "east", new Dictionary<string, string>
                 {
                     { "up", "south" }, { "down", "north" },
                     { "north", "up" }, { "south", "down" }
                 } }
             };

        private static Dictionary<string, Vec3> DegeneratePositionLocalFrame =
            new Dictionary<string, Vec3>
            {
                { "north", new Vec3(-1, 0, 0) },
                { "east",  new Vec3( 0, 1, 0) },
                { "up",    new Vec3( 0, 0, 1) },
                { "south", new Vec3( 1, 0, 0) },
                { "west",  new Vec3( 0,-1, 0) },
                { "down",  new Vec3( 0, 0,-1) }
            };

        private static Dictionary<string, Vec3> ScratchCalculateCartesian =
            new Dictionary<string, Vec3>
            {
                { "north", new Vec3() },
                { "east", new Vec3() },
                { "up", new Vec3() },
                { "south", new Vec3() },
                { "west", new Vec3() },
                { "down", new Vec3() }
            };
        /// <summary>
        /// computes a 4x4 transformation matrix from a reference frame
        /// centered at the provided origin to the provided ellipsoid's fixed reference frame.
        /// </summary>
        /// <param name="firstAxis"></param>
        /// <param name="secondAxis"></param>
        /// <param name="origin"></param>
        /// <param name="ellipsoid"></param>
        /// <returns></returns>
        private static Matrix4 LocalFrameToFixedFrame(
            Vec3 origin, Ellipsoid ellipsoid, string firstAxis, string secondAxis)
        {
            var thirdAxis = VectorProductLocalFrame[firstAxis][secondAxis];
            Vec3 scratchFirstCartesian, scratchSecondCartesian, scratchThirdCartesian;
            if (Math.Abs(origin.X) < 1e-14 && Math.Abs(origin.Y) < 1e-14)
            {
                // almost zero
                var sign = 0;
                if (origin.Z > 0) sign = 1;
                else if (origin.Z < 0) sign = -1;
                if (!DegeneratePositionLocalFrame.ContainsKey(firstAxis))
                {
                    throw new ArgumentException("no firstAxis", nameof(firstAxis));
                }
                if (!DegeneratePositionLocalFrame.ContainsKey(secondAxis))
                {
                    throw new ArgumentException("no secondAxis", nameof(secondAxis));
                }
                scratchFirstCartesian = DegeneratePositionLocalFrame[firstAxis];
                if (firstAxis != "east" && firstAxis != "west")
                {
                    scratchFirstCartesian = scratchFirstCartesian.MultiplyBy(sign);
                }
                scratchSecondCartesian = DegeneratePositionLocalFrame[secondAxis];
                if (secondAxis != "east" && secondAxis != "west")
                {
                    scratchSecondCartesian = scratchSecondCartesian.MultiplyBy(sign);
                }
                scratchThirdCartesian = DegeneratePositionLocalFrame[thirdAxis];
                if (thirdAxis != "east" && thirdAxis != "west")
                {
                    scratchThirdCartesian = scratchThirdCartesian.MultiplyBy(sign);
                }
            }
            else
            {
                if (ellipsoid == null)
                {
                    ellipsoid = Ellipsoid.Wgs84;
                }
                ScratchCalculateCartesian["up"] = ellipsoid.GeodeticSurfaceNormal(origin);
               
                ScratchCalculateCartesian["east"] = 
                    (new Vec3(-origin.Y, origin.X, 0.0)).Normalize();
                ScratchCalculateCartesian["north"] = 
                    Vec3.Cross(ScratchCalculateCartesian["up"], ScratchCalculateCartesian["east"]);

                ScratchCalculateCartesian["down"] = 
                    ScratchCalculateCartesian["up"].MultiplyBy(-1);
                ScratchCalculateCartesian["west"] = 
                    ScratchCalculateCartesian["east"].MultiplyBy(-1);
                ScratchCalculateCartesian["south"] =
                    ScratchCalculateCartesian["north"].MultiplyBy(-1);

                scratchFirstCartesian = ScratchCalculateCartesian[firstAxis];
                scratchSecondCartesian = ScratchCalculateCartesian[secondAxis];
                scratchThirdCartesian = ScratchCalculateCartesian[thirdAxis];

            }

            var result = new double[16];
            result[0] = scratchFirstCartesian.X;
            result[1] = scratchFirstCartesian.Y;
            result[2] = scratchFirstCartesian.Z;
            result[3] = 0.0;
            result[4] = scratchSecondCartesian.X;
            result[5] = scratchSecondCartesian.Y;
            result[6] = scratchSecondCartesian.Z;
            result[7] = 0.0;
            result[8] = scratchThirdCartesian.X;
            result[9] = scratchThirdCartesian.Y;
            result[10] = scratchThirdCartesian.Z;
            result[11] = 0.0;
            result[12] = origin.X;
            result[13] = origin.Y;
            result[14] = origin.Z;
            result[15] = 1.0;

            return new Matrix4
            {
                P00 = result[0],
                P01 = result[1],
                P02 = result[2],
                P03 = result[3],
                P10 = result[4],
                P11 = result[5],
                P12 = result[6],
                P13 = result[7],
                P20 = result[8],
                P21 = result[9],
                P22 = result[10],
                P23 = result[11],
                P30 = result[12],
                P31 = result[13],
                P32 = result[14],
                P33 = result[15]
            };
        }

        internal static Matrix4 HeadingPitchRollToFixedFrame(Vec3 origin, HeadingPitchRoll hpr)
        {
            var hprQuaternion = Quaternion.FromHeadingPitchRoll(hpr);
            var scratchScale = new Vec3(1.0);
            var hprMatrix = Matrix4.FromTranslationQuaternionRotationScale(
                new Vec3(), hprQuaternion, scratchScale);

            var fixedFrameTransform = EastNorthUpToFixedFrame(origin, Ellipsoid.Wgs84);

            return Matrix4.Multiply(fixedFrameTransform, hprMatrix);

        }

        public static Vec3 CesiumFromRadians(double longitude, double latitude,
            double height, Vec3 radiiSquared)
        {
            var cosLatitude = Math.Cos(latitude);
            var scratchN = (new Vec3(
                cosLatitude * Math.Cos(longitude),
                cosLatitude * Math.Sin(longitude),
                Math.Sin(latitude)
                )).Normalize();
            var scratchK = Vec3.Multiply(radiiSquared, scratchN);
            var gamma = Math.Sqrt(Vec3.Dot(scratchN, scratchK));
            scratchK = scratchK.DividedBy(gamma);
            scratchN = scratchN.MultiplyBy(height);
            return Vec3.Add(scratchK, scratchN);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="longitude">longitude in radian</param>
        /// <param name="latitude">latitude in radian</param>
        /// <param name="height">height in meter</param>
        /// <returns></returns>
        public static Matrix4 Wgs84Transform(double longitude, double latitude, double height)
        {
            var wgs84RadiiSquared =  new Vec3(6378137.0 * 6378137.0, 
                6378137.0 * 6378137.0, 6356752.3142451793 * 6356752.3142451793);
            var pnt = CesiumFromRadians(
                longitude, latitude, height, wgs84RadiiSquared);

            return HeadingPitchRollToFixedFrame(pnt, new HeadingPitchRoll());
        }
    }

    internal class HeadingPitchRoll
    {
        public double Heading { get; set; }

        public double Pitch { get; set; }

        public double Roll { get; set; }
    }
    /// <summary>
    /// A set of 4-dimensional coordinates used to represent rotation in 3-dimensional space.
    /// </summary>
    internal class Quaternion
    {
        public double X { get; set; }

        public double Y { get; set; }

        public double Z { get; set; }

        public double W { get; set; }

        public static Quaternion Multiply(Quaternion left, Quaternion right)
        {
            var leftX = left.X;
            var leftY = left.Y;
            var leftZ = left.Z;
            var leftW = left.W;

            var rightX = right.X;
            var rightY = right.Y;
            var rightZ = right.Z;
            var rightW = right.W;

            var x = leftW * rightX + leftX * rightW + leftY * rightZ - leftZ * rightY;
            var y = leftW * rightY - leftX * rightZ + leftY * rightW + leftZ * rightX;
            var z = leftW * rightZ + leftX * rightY - leftY * rightX + leftZ * rightW;
            var w = leftW * rightW - leftX * rightX - leftY * rightY - leftZ * rightZ;

            return new Quaternion { X = x, Y = y, Z = z, W = w };
        }

        public static Quaternion FromAxisAngle(Vec3 axis, double angle)
        {
            var halfAngle = angle / 2.0;
            var s = Math.Sin(halfAngle);

            var fromAxisAngleScratch = axis.Normalize();

            var x = fromAxisAngleScratch.X * s;
            var y = fromAxisAngleScratch.Y * s;
            var z = fromAxisAngleScratch.Z * s;
            var w = Math.Cos(halfAngle);

            return new Quaternion { X = x, Y = y, Z = z, W = w };
        }
        /// <summary>
        /// Computes a rotation from the given heading, pitch and roll angles. 
        /// Heading is the rotation about the negative z axis. 
        /// Pitch is the rotation about the negative y axis.
        /// Roll is the rotation about the positive x axis.
        /// </summary>
        /// <param name="hpr"></param>
        /// <returns></returns>
        public static Quaternion FromHeadingPitchRoll(HeadingPitchRoll hpr)
        {
            var scratchRollQuaternion = FromAxisAngle(new Vec3(1.0, 0, 0), hpr.Roll);
            var scratchPitchQuaternion = FromAxisAngle(new Vec3(0, 1.0, 0), -hpr.Pitch);
            scratchPitchQuaternion = Multiply(scratchPitchQuaternion, scratchRollQuaternion);
            var scratchHeadingQuaternion = FromAxisAngle(new Vec3(0, 0, 1.0), -hpr.Heading);

            return Multiply(scratchHeadingQuaternion, scratchPitchQuaternion);

        }

        static Quaternion scratchHPRQuaternion = new Quaternion();
        static Quaternion scratchHeadingQuaternion = new Quaternion();
        static Quaternion scratchPitchQuaternion = new Quaternion();
        static Quaternion scratchRollQuaternion = new Quaternion();
    }

    public class Matrix4
    {

        public double P00;

        public double P01;

        public double P02;

        public double P03;

        public double P10;

        public double P11;

        public double P12;

        public double P13;

        public double P20;

        public double P21;

        public double P22;

        public double P23;

        public double P30;

        public double P31;

        public double P32;

        public double P33;
        /// <summary>
        /// Computes a Matrix4 instance from a translation, rotation, and scale (TRS)
        ///  representation with the rotation represented as a quaternion.
        /// </summary>
        /// <param name="translation"></param>
        /// <param name="rotation"></param>
        /// <param name="scale"></param>
        /// <returns></returns>
        internal static Matrix4 FromTranslationQuaternionRotationScale(Vec3 translation,
            Quaternion rotation, Vec3 scale)
        {
            var scaleX = scale.X;
            var scaleY = scale.Y;
            var scaleZ = scale.Z;

            var x2 = rotation.X * rotation.X;
            var xy = rotation.X * rotation.Y;
            var xz = rotation.X * rotation.Z;
            var xw = rotation.X * rotation.W;
            var y2 = rotation.Y * rotation.Y;
            var yz = rotation.Y * rotation.Z;
            var yw = rotation.Y * rotation.W;
            var z2 = rotation.Z * rotation.Z;
            var zw = rotation.Z * rotation.W;
            var w2 = rotation.W * rotation.W;

            var m00 = x2 - y2 - z2 + w2;
            var m01 = 2.0 * (xy - zw);
            var m02 = 2.0 * (xz + yw);

            var m10 = 2.0 * (xy + zw);
            var m11 = -x2 + y2 - z2 + w2;
            var m12 = 2.0 * (yz - xw);

            var m20 = 2.0 * (xz - yw);
            var m21 = 2.0 * (yz + xw);
            var m22 = -x2 - y2 + z2 + w2;

            var result = new double[16];

            result[0]  = m00 * scaleX;
            result[1]  = m10 * scaleX;
            result[2]  = m20 * scaleX;
            result[3]  = 0.0;
            result[4]  = m01 * scaleY;
            result[5]  = m11 * scaleY;
            result[6]  = m21 * scaleY;
            result[7]  = 0.0;
            result[8]  = m02 * scaleZ;
            result[9]  = m12 * scaleZ;
            result[10] = m22 * scaleZ;
            result[11] = 0.0;
            result[12] = translation.X;
            result[13] = translation.Y;
            result[14] = translation.Z;
            result[15] = 1.0;

            return new Matrix4
            {
                P00 = result[0],
                P01 = result[1],
                P02 = result[2],
                P03 = result[3],
                P10 = result[4],
                P11 = result[5],
                P12 = result[6],
                P13 = result[7],
                P20 = result[8],
                P21 = result[9],
                P22 = result[10],
                P23 = result[11],
                P30 = result[12],
                P31 = result[13],
                P32 = result[14],
                P33 = result[15]
            };
        }

        public static Matrix4 Multiply(Matrix4 left, Matrix4 right)
        {
            var left0  = left.P00;
            var left1  = left.P01;
            var left2  = left.P02;
            var left3  = left.P03;
            var left4  = left.P10;
            var left5  = left.P11;
            var left6  = left.P12;
            var left7  = left.P13;
            var left8  = left.P20;
            var left9  = left.P21;
            var left10 = left.P22;
            var left11 = left.P23;
            var left12 = left.P30;
            var left13 = left.P31;
            var left14 = left.P32;
            var left15 = left.P33;

            var right0  = right.P00;
            var right1  = right.P01;
            var right2  = right.P02;
            var right3  = right.P03;
            var right4  = right.P10;
            var right5  = right.P11;
            var right6  = right.P12;
            var right7  = right.P13;
            var right8  = right.P20;
            var right9  = right.P21;
            var right10 = right.P22;
            var right11 = right.P23;
            var right12 = right.P30;
            var right13 = right.P31;
            var right14 = right.P32;
            var right15 = right.P33;

            var column0Row0 = left0 * right0 + left4 * right1 + left8 * right2 + left12 * right3;
            var column0Row1 = left1 * right0 + left5 * right1 + left9 * right2 + left13 * right3;
            var column0Row2 = left2 * right0 + left6 * right1 + left10 * right2 + left14 * right3;
            var column0Row3 = left3 * right0 + left7 * right1 + left11 * right2 + left15 * right3;

            var column1Row0 = left0 * right4 + left4 * right5 + left8 * right6 + left12 * right7;
            var column1Row1 = left1 * right4 + left5 * right5 + left9 * right6 + left13 * right7;
            var column1Row2 = left2 * right4 + left6 * right5 + left10 * right6 + left14 * right7;
            var column1Row3 = left3 * right4 + left7 * right5 + left11 * right6 + left15 * right7;

            var column2Row0 = left0 * right8 + left4 * right9 + left8 * right10 + left12 * right11;
            var column2Row1 = left1 * right8 + left5 * right9 + left9 * right10 + left13 * right11;
            var column2Row2 = left2 * right8 + left6 * right9 + left10 * right10 + left14 * right11;
            var column2Row3 = left3 * right8 + left7 * right9 + left11 * right10 + left15 * right11;

            var column3Row0 = left0 * right12 + left4 * right13 + left8 * right14 + left12 * right15;
            var column3Row1 = left1 * right12 + left5 * right13 + left9 * right14 + left13 * right15;
            var column3Row2 = left2 * right12 + left6 * right13 + left10 * right14 + left14 * right15;
            var column3Row3 = left3 * right12 + left7 * right13 + left11 * right14 + left15 * right15;

            var result = new double[16];
            result[0]  = column0Row0;
            result[1]  = column0Row1;
            result[2]  = column0Row2;
            result[3]  = column0Row3;
            result[4]  = column1Row0;
            result[5]  = column1Row1;
            result[6]  = column1Row2;
            result[7]  = column1Row3;
            result[8]  = column2Row0;
            result[9]  = column2Row1;
            result[10] = column2Row2;
            result[11] = column2Row3;
            result[12] = column3Row0;
            result[13] = column3Row1;
            result[14] = column3Row2;
            result[15] = column3Row3;

            return new Matrix4
            {
                P00 = result[0],
                P01 = result[1],
                P02 = result[2],
                P03 = result[3],
                P10 = result[4],
                P11 = result[5],
                P12 = result[6],
                P13 = result[7],
                P20 = result[8],
                P21 = result[9],
                P22 = result[10],
                P23 = result[11],
                P30 = result[12],
                P31 = result[13],
                P32 = result[14],
                P33 = result[15]
            };
        }

        public double[] ToArray()
        {
            return new[]
            {
                P00, P01, P02, P03,
                P10, P11, P12, P13,
                P20, P21, P22, P23,
                P30, P31, P32, P33
            };
        }

        public override string ToString()
        {
            return String.Join(",", ToArray());
        }
    }

    // https://github.com/AnalyticalGraphicsInc/cesium
    /// <summary>
    /// A quadratic surface defined in Cartesian coordinates by the equation
    /// <code>(x / a)^2 + (y / b)^2 + (z / c)^2 = 1</code>.  Primarily used
    /// by Cesium to represent the shape of planetary bodies.
    /// </summary>
    public class Ellipsoid
    {

        public Vec3 Radii { get; }

        public Vec3 RadiiSquared { get; }

        public Vec3 RadiiToTheFourth { get; }

        public Vec3 OneOverRadii { get; }

        public Vec3 OneOverRadiiSquared { get; }

        public double MinimumRadius { get; }

        public double MaximumRadius { get; }

        public double CenterToleranceSquared { get; }

        public double SquaredXOverSquaredZ { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x">The radius in the x direction.</param>
        /// <param name="y">The radius in the y direction.</param>
        /// <param name="z">The radius in the z direction.</param>
        public Ellipsoid(double x, double y, double z)
        {
            Radii = new Vec3(x, y, z);
            RadiiSquared = new Vec3(x * x, y * y, z * z);
            RadiiToTheFourth = new Vec3(x * x * x * x, y * y * y * y, z * z * z * z);
            OneOverRadii = new Vec3(
                x == 0.0 ? 0.0 : 1.0 /x,
                y == 0.0 ? 0.0 : 1.0 /y,
                z == 0.0 ? 0.0 : 1.0 /z
            );
            OneOverRadiiSquared = new Vec3(
                x == 0.0 ? 0.0 : 1.0 / (x*x),
                y == 0.0 ? 0.0 : 1.0 / (y*y),
                z == 0.0 ? 0.0 : 1.0 / (z*z)
            );
            MinimumRadius = (new[] { x, y, z }).Min();
            MaximumRadius = (new[] { x, y, z }).Max();
            CenterToleranceSquared = 0.1;

            if (RadiiSquared.Z != 0.0)
            {
                SquaredXOverSquaredZ = RadiiSquared.X / RadiiSquared.Z;
            }
        }

        public Vec3 GeodeticSurfaceNormal(Vec3 pnt)
        {
            var res = Vec3.Multiply(pnt, OneOverRadiiSquared);
            return res.Normalize();
        }
        /// <summary>
        /// Converts the provided cartesian to cartographic representation.
        /// The cartesian is undefined at the center of the ellipsoid.
        /// </summary>
        /// <param name="xyz"></param>
        /// <returns></returns>
        public Vec3? CartesianToCartographic(Vec3 xyz)
        {

            var p = ScaleToGeodeticSurface(xyz, OneOverRadii, OneOverRadiiSquared, CenterToleranceSquared);
            if (!p.HasValue) return p;
            var n = GeodeticSurfaceNormal(p.Value);
            var h = xyz.Substract(p.Value);

            var longitude = Math.Atan2(n.Y, n.X);
            var latitude = Math.Asin(n.Z);

            var heitDot = Vec3.Dot(h, xyz);
            var heitSign = 0.0;
            if (heitDot > 0)
            {
                heitSign = 1.0;
            }
            else if (heitDot < 0)
            {
                heitSign = -1.0;
            }
            var height = heitSign * h.GetLength();

            return new Vec3(longitude, latitude, height);
        }

        /// <summary>
        /// Scales the provided Cartesian position along the geodetic surface normal
        /// so that it is on the surface of this ellipsoid.If the position is
        /// at the center of the ellipsoid, this function returns undefined.
        /// </summary>
        /// <param name="cartesian">The Cartesian position to scale.</param>
        /// <param name="oneOverRadii">One over radii of the ellipsoid.</param>
        /// <param name="oneOverRadiiSquared">One over radii squared of the ellipsoid.</param>
        /// <param name="centerToleranceSquared">Tolerance for closeness to the center.</param>
        /// <returns></returns>
        private static Vec3? ScaleToGeodeticSurface(Vec3 cartesian,
            Vec3 oneOverRadii, Vec3 oneOverRadiiSquared, 
            double centerToleranceSquared)
        {

            var positionX = cartesian.X;
            var positionY = cartesian.Y;
            var positionZ = cartesian.Z;

            var oneOverRadiiX = oneOverRadii.X;
            var oneOverRadiiY = oneOverRadii.Y;
            var oneOverRadiiZ = oneOverRadii.Z;

            var x2 = positionX * positionX * oneOverRadiiX * oneOverRadiiX;
            var y2 = positionY * positionY * oneOverRadiiY * oneOverRadiiY;
            var z2 = positionZ * positionZ * oneOverRadiiZ * oneOverRadiiZ;

            // Compute the squared ellipsoid norm.
            var squaredNorm = x2 + y2 + z2;
            var ratio = Math.Sqrt(1.0 / squaredNorm);

            // As an initial approximation, assume that the radial intersection is the projection point.
            var intersection = cartesian.MultiplyBy(ratio);

            // If the position is near the center, the iteration will not converge.
            if (squaredNorm < centerToleranceSquared)
            {
                return double.IsInfinity(ratio) ? default(Vec3?) : intersection;
            }

            var oneOverRadiiSquaredX = oneOverRadiiSquared.X;
            var oneOverRadiiSquaredY = oneOverRadiiSquared.Y;
            var oneOverRadiiSquaredZ = oneOverRadiiSquared.Z;

            // Use the gradient at the intersection point in place of the true unit normal.
            // The difference in magnitude will be absorbed in the multiplier.
            var gradient = new Vec3(
                intersection.X * oneOverRadiiSquaredX * 2.0,
                intersection.Y * oneOverRadiiSquaredY * 2.0,
                intersection.Z * oneOverRadiiSquaredZ * 2.0
                );

            // Compute the initial guess at the normal vector multiplier, lambda.
            var lambda = (1.0 - ratio) * cartesian.GetLength() / (0.5 * gradient.GetLength());
            var correction = 0.0;

            double xMultiplier;
            double yMultiplier;
            double zMultiplier;
            double func;
            do
            {
                lambda -= correction;

                xMultiplier = 1.0 / (1.0 + lambda * oneOverRadiiSquaredX);
                yMultiplier = 1.0 / (1.0 + lambda * oneOverRadiiSquaredY);
                zMultiplier = 1.0 / (1.0 + lambda * oneOverRadiiSquaredZ);

                var xMultiplier2 = xMultiplier * xMultiplier;
                var yMultiplier2 = yMultiplier * yMultiplier;
                var zMultiplier2 = zMultiplier * zMultiplier;

                var xMultiplier3 = xMultiplier2 * xMultiplier;
                var yMultiplier3 = yMultiplier2 * yMultiplier;
                var zMultiplier3 = zMultiplier2 * zMultiplier;

                func = x2 * xMultiplier2 + y2 * yMultiplier2 + z2 * zMultiplier2 - 1.0;

                // "denominator" here refers to the use of this expression in the velocity and acceleration
                // computations in the sections to follow.
                var denominator = x2 * xMultiplier3 * oneOverRadiiSquaredX + y2 * yMultiplier3 * oneOverRadiiSquaredY + z2 * zMultiplier3 * oneOverRadiiSquaredZ;

                var derivative = -2.0 * denominator;

                correction = func / derivative;
            } while (Math.Abs(func) > 1e-12); // CesiumMath.EPSILON12

            return new Vec3(positionX * xMultiplier, positionY * yMultiplier, positionZ * zMultiplier);

        }

        public static Ellipsoid Wgs84 { get; } = new Ellipsoid(
            6378137.0, 6378137.0, 6356752.3142451793);
    }
}
