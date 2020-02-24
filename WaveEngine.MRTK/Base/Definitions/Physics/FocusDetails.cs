// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;
using WaveEngine.Mathematics;
using WaveEngine.MixedReality.Toolkit.Input;

namespace WaveEngine.MixedReality.Toolkit.Physics
{
    /// <summary>
    /// Contains information about which game object has the focus currently.
    /// Also contains information about the normal of that point.
    /// </summary>
    public struct FocusDetails
    {
        /// <summary>
        /// Gets or sets the distance along the ray until a hit, or until the end of the ray if no hit.
        /// </summary>
        public float RayDistance { get; set; }

        /// <summary>
        /// Gets or sets the hit point of the raycast.
        /// </summary>
        public Vector3 Point { get; set; }

        /// <summary>
        /// Gets or sets the normal of the raycast.
        /// </summary>
        public Vector3 Normal { get; set; }

        /// <summary>
        /// Gets or sets the entity hit by the last raycast.
        /// </summary>
        public Entity Entity { get; set; }

        // TODO: MixedRealityRaycastHit
        /////// <summary>
        /////// The last raycast hit info.
        /////// </summary>
        ////public MixedRealityRaycastHit LastRaycastHit { get; set; }

        /// <summary>
        /// Gets or sets the last raycast hit info for graphic raycast.
        /// </summary>
        public RayHit3D LastGraphicsRaycastResult { get; set; }

        /// <summary>
        /// Gets or sets the point in local space.
        /// </summary>
        public Vector3 PointLocalSpace { get; set; }

        /// <summary>
        /// Gets or sets the point in local space.
        /// </summary>
        public Vector3 NormalLocalSpace { get; set; }
    }
}
