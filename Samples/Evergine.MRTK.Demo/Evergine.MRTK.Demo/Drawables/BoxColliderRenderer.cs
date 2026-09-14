using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Managers;
using Evergine.Framework.Physics;
using Evergine.Mathematics;

namespace Evergine.MRTK.Demo.Drawables
{
    public class BoxColliderRenderer : Drawable3D
    {
        [BindSceneManager]
        private RenderManager renderManager = null;

        [BindComponent]
        protected Transform3D transform = null;

        [BindComponent]
        protected BoxCollider collider = null;

        public override void Draw(DrawContext drawContext)
        {
            if (this.IsActivated)
            {
                var colliderShape = this.collider;

                BoundingOrientedBox box;
                box.Center = Vector3.Transform(this.collider.Offset, this.transform.WorldTransform);
                box.HalfExtent = colliderShape.Size * this.transform.Scale * 0.5f;
                box.Orientation = Quaternion.Concatenate(Quaternion.CreateFromYawPitchRoll(this.collider.RotationOffset.Y, this.collider.RotationOffset.X, this.collider.RotationOffset.Z), this.transform.Orientation);

                this.renderManager.LineBatch3D.DrawBoundingOrientedBox(box, Color.White);
            }
        }
    }
}
