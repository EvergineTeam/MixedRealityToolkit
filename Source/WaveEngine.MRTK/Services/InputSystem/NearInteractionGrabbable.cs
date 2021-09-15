// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;
using WaveEngine.Framework.Physics3D;

namespace WaveEngine.MRTK.Services.InputSystem
{
    /// <summary>
    /// The near interaction grabbable component.
    /// </summary>
    public class NearInteractionGrabbable : Component
    {
        /// <summary>
        /// The collider 3D.
        /// </summary>
        [BindComponent(isExactType: false)]
        public Collider3D Collider3D;

        /// <summary>
        /// The static body.
        /// </summary>
        [BindComponent]
        public StaticBody3D StaticBody3D;
    }
}
