using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.Services.InputSystem
{
    public class NearInteractionTouchable : Component
    {
        [BindComponent]
        public BoxCollider3D BoxCollider3D;

        [BindComponent]
        public StaticBody3D StaticBody3D;

        [DontRenderProperty]
        public Vector3 LocalPressDirection { get; private set; } = Vector3.Forward;

        [DontRenderProperty]
        public Matrix4x4 BoxCollider3DTransform = Matrix4x4.Identity;

        [DontRenderProperty]
        public Matrix4x4 BoxCollider3DTransformInverse = Matrix4x4.Identity;

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