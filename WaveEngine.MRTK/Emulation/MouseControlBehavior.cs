// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Common.Input.Keyboard;
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
        protected Transform3D transform = null;

        /// <summary>
        /// The camera.
        /// </summary>
        [BindComponent(isRequired: true, source: BindComponentSource.Scene)]
        protected Camera3D camera;

        /// <summary>
        /// The cursor to move.
        /// </summary>
        [BindComponent(isRequired: true, source: BindComponentSource.Owner)]
        protected Cursor cursor;

        /// <summary>
        /// Key that must be pressed.
        /// </summary>
        public Keys key = Keys.LeftShift;

        private float camDist = 0.5f;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();
            this.UpdateOrder = this.cursor.UpdateOrder + 0.1f; // Ensure this is executed always afte the Cursor (beacuse it cactched PreviousPinch)

            return attached;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            var graphicsPresenter = Application.Current.Container.Resolve<GraphicsPresenter>();
            var keyboardDispatcher = graphicsPresenter.FocusedDisplay.KeyboardDispatcher;

            if (keyboardDispatcher.ReadKeyState(this.key) == WaveEngine.Common.Input.ButtonState.Pressed)
            {
                var mouseDispatcher = graphicsPresenter.FocusedDisplay.MouseDispatcher;

                if (keyboardDispatcher.ReadKeyState(Keys.LeftControl) == WaveEngine.Common.Input.ButtonState.Pressed)
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

                if (mouseDispatcher.ReadButtonState(WaveEngine.Common.Input.Mouse.MouseButtons.Left) == WaveEngine.Common.Input.ButtonState.Pressing)
                {
                    this.cursor.Pinch = true;
                }
                else if (mouseDispatcher.ReadButtonState(WaveEngine.Common.Input.Mouse.MouseButtons.Left) == WaveEngine.Common.Input.ButtonState.Releasing)
                {
                    this.cursor.Pinch = false;
                }
            }
        }
    }
}
