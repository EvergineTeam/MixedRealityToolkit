// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Input.Keyboard;
using Evergine.Common.Input.Mouse;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Mathematics;
using System;
using System.Text;

namespace Evergine.MRTK.Services.TeleportSystem.Behaviors
{
    /// <summary>
    /// Character Controller class for mixed reality.
    /// </summary>
    public class MixedRealityCharacterControllerBehavior : Behavior
    {
        [BindComponent]
        private readonly CharacterController3D characterController = null;

        /// <summary>
        /// The Transform component of the entity to spin (own entity by default).
        /// </summary>
        [BindComponent(false)]
        private Transform3D transform = null;

        /// <summary>
        /// The Camera component of the entity (own entity by default).
        /// </summary>
        [BindComponent(false, true, BindComponentSource.ChildrenSkipOwner)]
        private Camera camera = null;

        /// <summary>
        /// Gets or sets the move speed of the camera.
        /// </summary>
        public float MoveSpeed { get; set; }

        /// <summary>
        /// Gets or sets the rotation speed of the camera.
        /// </summary>
        public float RotationSpeed { get; set; }

        private struct MoveStruct
        {
            public float moveForward;
            public float moveBackward;
            public float moveLeft;
            public float moveRight;
            public float moveUp;
            public float moveDown;

            public float yaw;
            public float pitch;
            public float roll;

            public void Clear()
            {
                this.moveForward = 0.0f;
                this.moveBackward = 0.0f;
                this.moveLeft = 0.0f;
                this.moveRight = 0.0f;
                this.moveUp = 0.0f;
                this.moveDown = 0.0f;

                this.yaw = 0.0f;
                this.pitch = 0.0f;
                this.roll = 0.0f;
            }
        }

        private MoveStruct moveStruct = new MoveStruct();

        /// <summary>
        /// Gets or sets the Mouse sensibility.
        /// </summary>
        /// <remarks>
        /// 0.5 is for stop, 1 is for raw delta, 2 is twice delta.
        /// </remarks>
        public float MouseSensibility { get; set; }

        /// <summary>
        /// Gets or sets the maximum pitch angle.
        /// </summary>
        public float MaxPitch { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MixedRealityCharacterControllerBehavior"/> class.
        /// </summary>
        public MixedRealityCharacterControllerBehavior()
        {
            this.MoveSpeed = 5.0f;
            this.RotationSpeed = 5.0f;
            this.MouseSensibility = 0.03f;
            this.MaxPitch = MathHelper.PiOver2 * 0.95f;

            this.moveStruct.Clear();
        }

        /// <summary>
        /// Teleports user to target position. It keeps the original rotation.
        /// </summary>
        /// <param name="newPosition">The target position.</param>
        public void Teleport(Vector3 newPosition)
        {
            if (this.characterController != null)
            {
                if (this.characterController.InternalBody != null)
                {
                    var characterTransform = this.characterController.InternalBody.Transform;
                    characterTransform.Translation = newPosition;
                    this.characterController.ResetTransform(characterTransform.Translation, characterTransform.Orientation, characterTransform.Scale);
                }
            }
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();
            this.characterController.Gravity = 0;
        }

        /// <summary>
        /// Handles every input device.
        /// </summary>
        private void HandleInput()
        {
            this.HandleKeyboard();
            this.HandleMouse();
        }

        /// <summary>
        /// Handles Keyboard Input.
        /// </summary>
        private void HandleKeyboard()
        {
            var display = this.camera.Display;

            if (display == null)
            {
                return;
            }

            var keyboardDispatcher = display.KeyboardDispatcher;

            // Keyboard Speed modifier
            var currentSpeed = 1.0f;

            if (keyboardDispatcher == null)
            {
                return;
            }

            if (keyboardDispatcher.IsKeyDown(Keys.LeftShift))
            {
                currentSpeed *= 2.0f;
            }
            else if (keyboardDispatcher.IsKeyDown(Keys.LeftControl))
            {
                currentSpeed /= 2.0f;
            }

            if (keyboardDispatcher.IsKeyDown(Keys.W))
            {
                this.moveStruct.moveForward = currentSpeed;
            }
            else if (keyboardDispatcher.IsKeyDown(Keys.S))
            {
                this.moveStruct.moveBackward = currentSpeed;
            }

            if (keyboardDispatcher.IsKeyDown(Keys.A))
            {
                this.moveStruct.moveLeft = currentSpeed;
            }
            else if (keyboardDispatcher.IsKeyDown(Keys.D))
            {
                this.moveStruct.moveRight = currentSpeed;
            }

            if (keyboardDispatcher.IsKeyDown(Keys.Q))
            {
                this.moveStruct.moveUp = currentSpeed;
            }
            else if (keyboardDispatcher.IsKeyDown(Keys.E))
            {
                this.moveStruct.moveDown = currentSpeed;
            }

            if (keyboardDispatcher.IsKeyDown(Keys.Up))
            {
                this.moveStruct.pitch = currentSpeed;
            }
            else if (keyboardDispatcher.IsKeyDown(Keys.Down))
            {
                this.moveStruct.pitch = -currentSpeed;
            }

            if (keyboardDispatcher.IsKeyDown(Keys.Left))
            {
                this.moveStruct.yaw = currentSpeed;
            }
            else if (keyboardDispatcher.IsKeyDown(Keys.Right))
            {
                this.moveStruct.yaw = -currentSpeed;
            }
        }

        /// <summary>
        /// Handles Mouse Input.
        /// </summary>
        private void HandleMouse()
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
                this.moveStruct.yaw = -positionDelta.X * this.MouseSensibility;
                this.moveStruct.pitch = -positionDelta.Y * this.MouseSensibility;
            }
        }

        /// <summary>
        /// Helper method to calculate displacement using configurable speed and amount.
        /// </summary>
        /// <param name="director">Direction Vector.</param>
        /// <param name="maxCurrentSpeed">Max Speed.</param>
        /// <param name="amount">Movement proportion. 0 = stop, 1 = max movement.</param>
        /// <param name="displacement">Output vector.</param>
        private void Displacement(Vector3 director, float maxCurrentSpeed, float amount, ref Vector3 displacement)
        {
            var elapsedAmount = maxCurrentSpeed * amount;

            // Manual in-line: position += speed * forward;
            displacement.X = displacement.X + (elapsedAmount * director.X);
            displacement.Y = displacement.Y + (elapsedAmount * director.Y);
            displacement.Z = displacement.Z + (elapsedAmount * director.Z);
        }

        /// <summary>
        /// Updates the entity transform component.
        /// </summary>
        /// <param name="elapsed">Elapsed time in seconds.</param>
        private void UpdatePositionAndOrientation(float elapsed)
        {
            Vector3 displacement = Vector3.Zero;
            Matrix4x4 localTransform = this.transform.LocalTransform;

            var elapsedMaxSpeed = elapsed * this.MoveSpeed;

            if (this.moveStruct.moveForward != 0.0f)
            {
                this.Displacement(localTransform.Forward, elapsedMaxSpeed, this.moveStruct.moveForward, ref displacement);
            }
            else if (this.moveStruct.moveBackward != 0.0f)
            {
                this.Displacement(localTransform.Backward, elapsedMaxSpeed, this.moveStruct.moveBackward, ref displacement);
            }

            if (this.moveStruct.moveLeft != 0.0f)
            {
                this.Displacement(localTransform.Left, elapsedMaxSpeed, this.moveStruct.moveLeft, ref displacement);
            }
            else if (this.moveStruct.moveRight != 0.0f)
            {
                this.Displacement(localTransform.Right, elapsedMaxSpeed, this.moveStruct.moveRight, ref displacement);
            }

            if (this.moveStruct.moveUp != 0.0f)
            {
                this.Displacement(localTransform.Up, elapsedMaxSpeed, this.moveStruct.moveUp, ref displacement);
            }
            else if (this.moveStruct.moveDown != 0.0f)
            {
                this.Displacement(localTransform.Down, elapsedMaxSpeed, this.moveStruct.moveDown, ref displacement);
            }

            // Manual in-line: camera.Position = position;
            ////this.transform.LocalPosition += displacement;
            this.characterController.SetVelocity(displacement);

            // Rotation:
            var rotation = this.transform.LocalRotation;
            rotation.Y += this.moveStruct.yaw * this.RotationSpeed * (1 / 60f);
            rotation.X += this.moveStruct.pitch * this.RotationSpeed * (1 / 60f);

            // Limit Pitch Angle
            rotation.X = MathHelper.Clamp(rotation.X, -this.MaxPitch, this.MaxPitch);
            ////this.transform.LocalRotation = rotation;
            this.characterController.Rotate(rotation.X);
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            this.HandleInput();
            this.UpdatePositionAndOrientation((float)gameTime.TotalSeconds);
        }

        ////public void Rotate(float angle)
        ////{

        ////}
    }
}
