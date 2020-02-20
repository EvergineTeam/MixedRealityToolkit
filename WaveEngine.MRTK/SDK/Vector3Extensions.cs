// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.SDK
{
    /// <summary>
    /// Vector3 extension methods.
    /// </summary>
    public class Vector3Extensions
    {
        /// <summary>
        /// Project a vector into another vector.
        /// </summary>
        /// <param name="vector">The first vector.</param>
        /// <param name="onVector">The vector to project.</param>
        /// <returns>The projected vector.</returns>
        public static Vector3 Project(Vector3 vector, Vector3 onVector)
        {
            var lengthSquared = onVector.LengthSquared();

            if (MathHelper.FloatEquals(lengthSquared, 0.0f))
            {
                return Vector3.Zero;
            }
            else
            {
                return onVector * Vector3.Dot(vector, onVector) / lengthSquared;
            }
        }
    }
}
