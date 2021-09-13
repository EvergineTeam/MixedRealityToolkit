// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Input;
using WaveEngine.Common.Input.Keyboard;
using WaveEngine.Common.Input.Mouse;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Emulation;

namespace WaveEngine.MRTK.Behaviors
{
    /// <summary>
    /// Mouse control behaviour for hololens cursors.
    /// </summary>
    public class MouseControlBehavior : Behavior
    {
        /// <summary>
        /// The transform.
        /// </summary>
        [BindComponent]
        protected Transform3D transform;

        /// <summary>
        /// The camera.
        /// </summary>
        [BindComponent(source: BindComponentSource.Scene)]
        protected Camera3D camera;

        /// <summary>
        /// The cursor to move.
        /// </summary>
        [BindComponent(isExactType: false, source: BindComponentSource.Children)]
        protected Cursor cursor;

        /// <summary>
        /// Gets or sets the key that must be pressed.
        /// </summary>
        public Keys Key { get; set; } = Keys.LeftShift;

        private float camDist = 0.5f;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            this.UpdateOrder = this.cursor.UpdateOrder + 0.1f; // Ensure this is executed always after the Cursor (because it cached PreviousPinch)

            return true;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            var graphicsPresenter = Application.Current.Container.Resolve<GraphicsPresenter>();
            var keyboardDispatcher = graphicsPresenter.FocusedDisplay.KeyboardDispatcher;

            if (keyboardDispatcher.ReadKeyState(this.Key) == ButtonState.Pressed)
            {
                var mouseDispatcher = graphicsPresenter.FocusedDisplay.MouseDispatcher;

                if (keyboardDispatcher.ReadKeyState(Keys.LeftControl) == ButtonState.Pressed)
                {
                    this.transform.Orientation = Quaternion.CreateFromAxisAngle(Vector3.Up, mouseDispatcher.PositionDelta.X * 0.01f) * this.transform.Orientation;
                    this.transform.Orientation = Quaternion.CreateFromAxisAngle(Vector3.Left, -mouseDispatcher.PositionDelta.Y * 0.01f) * this.transform.Orientation;
                }
                else
                {
                    this.camDist += mouseDispatcher.ScrollDelta.Y * 0.01f;

                    Ray ray;
                    Vector2 mousePos = mouseDispatcher.Position.ToVector2();
                    this.camera.CalculateRay(ref mousePos, out ray);

                    float ang = Vector3.Angle(ray.Direction, this.camera.Transform.WorldTransform.Forward);
                    float rDist = this.camDist / (float)Math.Cos(ang);

                    this.transform.Position = ray.GetPoint(rDist);
                }

                if (mouseDispatcher.ReadButtonState(MouseButtons.Left) == ButtonState.Pressing)
                {
                    this.cursor.Pinch = true;
                }
                else if (mouseDispatcher.ReadButtonState(MouseButtons.Left) == ButtonState.Releasing)
                {
                    this.cursor.Pinch = false;
                }
            }
        }
    }
}
