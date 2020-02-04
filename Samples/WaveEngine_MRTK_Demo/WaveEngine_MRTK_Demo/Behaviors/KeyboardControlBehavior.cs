using System;
using WaveEngine.Common.Input.Keyboard;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class KeyboardControlBehavior : Behavior
    {
        [BindComponent]
        protected Transform3D transform = null;

        public float Speed { get; set; } = 0.01f;

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

            displacement.Normalize();

            this.transform.LocalPosition += displacement * (float)gameTime.TotalSeconds * this.Speed;
        }
    }
}
