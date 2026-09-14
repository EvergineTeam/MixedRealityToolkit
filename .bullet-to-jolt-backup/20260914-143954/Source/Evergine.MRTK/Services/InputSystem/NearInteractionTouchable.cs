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
        [BindComponent(isRequired: false)]
        public BoxCollider3D BoxCollider3D;

        /// <summary>
        /// The mesh collider 3D.
        /// </summary>
        [BindComponent(isRequired: false)]
        public MeshCollider3D MeshCollider3D;

        /// <summary>
        /// The static body 3D.
        /// </summary>
        [BindComponent(isRequired: false, isExactType: false)]
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
            if (!base.OnAttached() || (this.MeshCollider3D == null && this.BoxCollider3D == null))
            {
                return false;
            }

            Vector3 offset = Vector3.Zero;
            Quaternion orientationOffset = Quaternion.Identity;
            Vector3 size = Vector3.Zero;
            if (this.BoxCollider3D != null)
            {
                offset = this.BoxCollider3D.Offset;
                orientationOffset = this.BoxCollider3D.OrientationOffset;
                size = this.BoxCollider3D.Size;
            }
            else if (this.MeshCollider3D != null)
            {
                offset = this.MeshCollider3D.Offset;
                orientationOffset = this.MeshCollider3D.OrientationOffset;
                size = this.MeshCollider3D.Size;
            }

            if (this.BoxCollider3D != null || this.MeshCollider3D != null)
            {
                // Precompute box collider local transforms
                this.BoxCollider3DTransform = Matrix4x4.CreateFromTRS(offset, orientationOffset, size);
                this.BoxCollider3DTransformInverse = Matrix4x4.Invert(this.BoxCollider3DTransform);
            }

            return true;
        }
    }
}
