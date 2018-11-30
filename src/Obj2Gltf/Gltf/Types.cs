using System;
using System.Collections.Generic;
using System.Text;

namespace Arctron.Gltf
{
    /// <summary>
    /// Specifies whether an attribute, vector, or matrix.
    /// </summary>
    public enum AccessorType
    {
        /// Scalar quantity.
        SCALAR = 1,

        /// 2D vector.
        VEC2,

        /// 3D vector.
        VEC3,

        /// 4D vector.
        VEC4,

        /// 2x2 matrix.
        MAT2,

        /// 3x3 matrix.
        MAT3,

        /// 4x4 matrix.
        MAT4
    }
    /// <summary>
    /// The component data type.
    /// </summary>
    public enum ComponentType
    {
        /// Corresponds to `GL_BYTE`.
        I8 = 5120,

        /// Corresponds to `GL_UNSIGNED_BYTE`.
        U8,

        /// Corresponds to `GL_SHORT`.
        I16,

        /// Corresponds to `GL_UNSIGNED_SHORT`.
        U16,

        /// Corresponds to `GL_UNSIGNED_INT`.
        U32 = 5125,

        /// Corresponds to `GL_FLOAT`.
        F32,
    }
}
