// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.
using Evergine.Common.Input.Mouse;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;
using System;

namespace Evergine.MRTK.Services.TeleportSystem.Behaviors
{
    /// <summary>
    /// Camera behavior to control Character Controller Camera in mixed reality by the use of mouse.
    /// </summary>
    public class MixedRealityCharacterControllerCameraBehavior : Behavior
    {
        /// <summary>
        /// The Transform component of the entity to spin (own entity by default).
        /// </summary>
        [BindComponent(false)]
        private Transform3D transform = null;

        /// <summary>
        /// The Camera component of the entity (own entity by default).
        /// </summary>
        [BindComponent(false)]
        private Camera camera = null;

        [BindComponent(source: BindComponentSource.ParentsSkipOwner)]
        private MixedRealityCharacterControllerBehavior MRCharacterController = null;

        /// <summary>
        /// Gets or sets the Mouse sensibility.
        /// </summary>
        /// <remarks>
        /// 0.5 is for stop, 1 is for raw delta, 2 is twice delta.
        /// </remarks>
        public float MouseSensibility = 0;

        /// <summary>
        /// Gets or sets the maximum pitch angle.
        /// </summary>
        public float MaxPitch = 0;

        /// <summary>
        /// Gets or sets the rotation speed of the camera.
        /// </summary>
        public float RotationSpeed { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MixedRealityCharacterControllerCameraBehavior"/> class.
        /// </summary>
        public MixedRealityCharacterControllerCameraBehavior()
        {
            this.MouseSensibility = 0.03f;
            this.MaxPitch = MathHelper.PiOver2 * 0.95f;
            this.RotationSpeed = 5.0f;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            this.HandleMouseInput();
        }

        private void HandleMouseInput()
        {
            var display = this.camera.Display;

            if (display == null)
            {
                return;
            }

            var mouseDispatcher = display.MouseDispatcher;

            if (mouseDispatcher?.IsButtonDown(MouseButtons.Right) == true)
            {
                var positionDelta = mouseDispatcher.PositionDelta;
                var yaw = -positionDelta.X * this.MouseSensibility;
                var pitch = -positionDelta.Y * this.MouseSensibility;

                var rotation = this.transform.LocalRotation;
                rotation.X += pitch * this.RotationSpeed * (1 / 60f);

                // Limit Pitch Angle
                rotation.X = MathHelper.Clamp(rotation.X, -this.MaxPitch, this.MaxPitch);

                // Pitch moves only camera
                this.transform.LocalRotation = rotation;

                // Yaw moves the whole entity
                this.MRCharacterController.Rotate(-yaw * this.RotationSpeed * (1 / 60f));
            }
        }
    }
}
