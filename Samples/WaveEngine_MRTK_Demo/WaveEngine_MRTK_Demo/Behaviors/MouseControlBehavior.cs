using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Common.Input.Keyboard;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class MouseControlBehavior : Behavior
    {
        [BindComponent]
        protected Transform3D transform = null;

        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "MainCamera")]
        protected Camera3D camera;

        [BindComponent(isRequired: true, source: BindComponentSource.Owner)]
        protected Cursor cursor;

        public Keys key = Keys.LeftShift;

        float camDist = 0.5f;

        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            return attached;
        }

        protected override void Update(TimeSpan gameTime)
        {
            var graphicsPresenter = Application.Current.Container.Resolve<GraphicsPresenter>();
            var keyboardDispatcher = graphicsPresenter.FocusedDisplay.KeyboardDispatcher;

            if (keyboardDispatcher.ReadKeyState(key) == WaveEngine.Common.Input.ButtonState.Pressed)
            {
                var mouseDispatcher = graphicsPresenter.FocusedDisplay.MouseDispatcher;

                if (keyboardDispatcher.ReadKeyState(Keys.LeftControl) == WaveEngine.Common.Input.ButtonState.Pressed)
                {
                    transform.Orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, mouseDispatcher.PositionDelta.X * 0.01f) * transform.Orientation;
                    transform.Orientation = Quaternion.CreateFromAxisAngle(Vector3.Left, -mouseDispatcher.PositionDelta.Y * 0.01f) * transform.Orientation;
                }
                else
                {
                    camDist += mouseDispatcher.ScrollDelta.Y * 0.01f;

                    Ray ray;
                    Vector2 mousePos = mouseDispatcher.Position.ToVector2();
                    camera.CalculateRay(ref mousePos, out ray);

                    float ang = Vector3.Angle(ray.Direction, camera.Transform.WorldTransform.Forward);
                    float rDist = camDist / (float)Math.Cos(ang);

                    this.transform.Position = ray.GetPoint(rDist);
                }


                if(mouseDispatcher.ReadButtonState(WaveEngine.Common.Input.Mouse.MouseButtons.Left) == WaveEngine.Common.Input.ButtonState.Pressing)
                {
                    cursor.Pinch = true;
                }
                else if (mouseDispatcher.ReadButtonState(WaveEngine.Common.Input.Mouse.MouseButtons.Left) == WaveEngine.Common.Input.ButtonState.Releasing)
                {
                    cursor.Pinch = false;
                }
            }
        }
    }
}
