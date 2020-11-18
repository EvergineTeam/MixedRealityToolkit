using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Mathematics;

namespace WaveEngine_MRTK_Demo.Drawables
{
    public class BoxColliderRenderer : Drawable3D
    {
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

                this.RenderManager.LineBatch3D.DrawBoundingOrientedBox(box, Color.White);
            }
        }
    }
}
