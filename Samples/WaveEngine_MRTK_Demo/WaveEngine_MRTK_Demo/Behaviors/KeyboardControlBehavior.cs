using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Input.Keyboard;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class KeyboardControlBehavior : Behavior
    {
        [BindComponent]
        protected Transform3D transform = null;

        [BindComponent]
        protected Cursor cursor;

        [RenderProperty(Tooltip = "The speed at which the entity will be moved")]
        public float Speed { get; set; } = 0.01f;

        [RenderProperty(Tooltip = "Set whether the component needs the right Shift key to be pressed in order to move the entity")]
        public bool UseShift { get; set; } = false;

        private Vector3 initialPosition;

        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.initialPosition = this.transform.LocalPosition;
            }

            return attached;
        }

        protected override void Update(TimeSpan gameTime)
        {
            var graphicsPresenter = Application.Current.Container.Resolve<GraphicsPresenter>();
            var keyboardDispatcher = graphicsPresenter.FocusedDisplay.KeyboardDispatcher;

            if (keyboardDispatcher.IsKeyDown(Keys.RightShift) != this.UseShift)
            {
                return;
            }

            if (keyboardDispatcher.IsKeyDown(Keys.R))
            {
                this.transform.LocalPosition = this.initialPosition;
            }

            Vector3 displacement = Vector3.Zero;
            Quaternion rotation = Quaternion.Identity;

            Matrix4x4 localTransform = this.transform.LocalTransform;

            if (keyboardDispatcher.IsKeyDown(Keys.I))
            {
                displacement += localTransform.Forward;
            }
            else if (keyboardDispatcher.IsKeyDown(Keys.K))
            {
                displacement += localTransform.Backward;
            }

            if (keyboardDispatcher.IsKeyDown(Keys.J))
            {
                displacement += localTransform.Left;
            }
            else if (keyboardDispatcher.IsKeyDown(Keys.L))
            {
                displacement += localTransform.Right;
            }

            if (keyboardDispatcher.IsKeyDown(Keys.O))
            {
                displacement += localTransform.Up;
            }
            else if (keyboardDispatcher.IsKeyDown(Keys.U))
            {
                displacement += localTransform.Down;
            }

            if (keyboardDispatcher.IsKeyDown(Keys.F))
            {
                rotation *= Quaternion.CreateFromEuler(Vector3.UnitX * 0.01f);
            }
            else if (keyboardDispatcher.IsKeyDown(Keys.H))
            {
                rotation *= Quaternion.CreateFromEuler(-Vector3.UnitX * 0.01f);
            }

            displacement.Normalize();

            this.transform.LocalPosition += displacement * Math.Min(0.3f, (float)gameTime.TotalSeconds) * this.Speed;
            this.transform.LocalOrientation *= rotation;

            if (keyboardDispatcher.ReadKeyState(Keys.P) == WaveEngine.Common.Input.ButtonState.Pressing)
            {
                this.cursor.Pinch = !this.cursor.Pinch;
            }
        }
    }
}
