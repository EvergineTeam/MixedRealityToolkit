// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Attributes;
using Evergine.Framework;
using Evergine.Framework.Physics3D;
using Evergine.Mathematics;

namespace Evergine.MRTK.Services.InputSystem
{
    /// <summary>
    /// The near interaction touchable component.
    /// </summary>
    public class NearInteractionTouchable : Component
    {
        /// <summary>
        /// The box collider 3D.
        /// </summary>
        [BindComponent]
        public BoxCollider3D BoxCollider3D;

        /// <summary>
        /// The static body 3D.
        /// </summary>
        [BindComponent(isExactType: false)]
        public StaticBody3D StaticBody3D;

        /// <summary>
        /// Gets the local press direction.
        /// By default it is set as -Forward as by convention all controls and GUI elements (such as the Text component) face the Forward direction.
        /// </summary>
        [IgnoreEvergine]
        [DontRenderProperty]
        public Vector3 LocalPressDirection { get; private set; } = -Vector3.Forward;

        /// <summary>
        /// Gets the box collider transform.
        /// </summary>
        [IgnoreEvergine]
        [DontRenderProperty]
        public Matrix4x4 BoxCollider3DTransform = Matrix4x4.Identity;

        /// <summary>
        /// Gets the box collider inverse transform.
        /// </summary>
        [IgnoreEvergine]
        [DontRenderProperty]
        public Matrix4x4 BoxCollider3DTransformInverse = Matrix4x4.Identity;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            var collider = this.BoxCollider3D;
            if (collider != null)
            {
                // Precompute box collider local transforms
                this.BoxCollider3DTransform = Matrix4x4.CreateFromTRS(collider.Offset, collider.OrientationOffset, collider.Size);
                this.BoxCollider3DTransformInverse = Matrix4x4.Invert(this.BoxCollider3DTransform);
            }

            return true;
        }
    }
}
