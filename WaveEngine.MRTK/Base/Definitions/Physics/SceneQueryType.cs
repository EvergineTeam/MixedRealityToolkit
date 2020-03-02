// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

namespace WaveEngine.MixedReality.Toolkit.Physics
{
    /// <summary>
    /// Defines the different scene query types. Mostly used by pointers.
    /// </summary>
    public enum SceneQueryType
    {
        /// <summary>
        /// Use a simple raycast from a single point in a given direction.
        /// </summary>
        SimpleRaycast,

        /// <summary>
        /// Complex raycast from multiple points using a box collider.
        /// </summary>
        BoxRaycast,

        /// <summary>
        /// Use Sphere cast.
        /// </summary>
        SphereCast,

        /// <summary>
        /// Check for colliders within a specific radius.
        /// </summary>
        SphereOverlap,
    }
}
