// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Framework;
using Evergine.Framework.Physics;

namespace Evergine.MRTK.Services.InputSystem
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
        public Collider Collider;

        /// <summary>
        /// The static body.
        /// </summary>
        [BindComponent]
        public StaticBody StaticBody;
    }
}
