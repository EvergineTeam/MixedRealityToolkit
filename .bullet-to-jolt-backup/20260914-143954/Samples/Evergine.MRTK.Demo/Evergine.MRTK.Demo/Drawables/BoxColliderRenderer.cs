using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Managers;
using Evergine.Framework.Physics3D;
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
        protected BoxCollider3D collider = null;

        public override void Draw(DrawContext drawContext)
        {
            if (this.IsActivated)
            {
                var colliderShape = this.collider.InternalColliderShape as IBoxColliderShape3D;

                BoundingOrientedBox box;
                box.Center = Vector3.Transform(this.collider.EffectiveOffset, this.transform.WorldTransform);
                box.HalfExtent = colliderShape.Size * this.transform.Scale * 0.5f;
                box.Orientation = Quaternion.Concatenate(this.collider.OrientationOffset, this.transform.Orientation);

                this.renderManager.LineBatch3D.DrawBoundingOrientedBox(box, Color.White);
            }
        }
    }
}
