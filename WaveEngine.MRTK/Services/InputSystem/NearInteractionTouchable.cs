// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.Services.InputSystem
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
        /// </summary>
        [WaveIgnore]
        [DontRenderProperty]
        public Vector3 LocalPressDirection { get; private set; } = Vector3.Forward;

        /// <summary>
        /// Gets the box collider transform.
        /// </summary>
        [WaveIgnore]
        [DontRenderProperty]
        public Matrix4x4 BoxCollider3DTransform = Matrix4x4.Identity;

        /// <summary>
        /// Gets the box collider inverse transform.
        /// </summary>
        [WaveIgnore]
        [DontRenderProperty]
        public Matrix4x4 BoxCollider3DTransformInverse = Matrix4x4.Identity;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                var collider = this.BoxCollider3D;
                if (collider != null)
                {
                    // Precompute box collider local transforms
                    this.BoxCollider3DTransform = Matrix4x4.CreateFromTRS(collider.Offset, collider.OrientationOffset, collider.Size);
                    this.BoxCollider3DTransformInverse = Matrix4x4.Invert(this.BoxCollider3DTransform);
                }
            }

            return attached;
        }
    }
}
