// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Input.Keyboard;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Mathematics;
using System;

namespace Evergine.MRTK.Services.TeleportSystem.Behaviors
{
    /// <summary>
    /// Character Controller class for mixed reality.
    /// </summary>
    public class MixedRealityCharacterControllerBehavior : Behavior
    {
        [BindComponent]
        private readonly CharacterController3D characterController = null;

        [BindComponent]
        private readonly Transform3D transform = null;

        /// <summary>
        /// The Camera component of the entity (own entity by default).
        /// </summary>
        [BindComponent(false, true, BindComponentSource.ChildrenSkipOwner)]
        private Camera camera = null;

        /// <summary>
        /// Gets or sets the move speed of the camera.
        /// </summary>
        public float MoveSpeed { get; set; }

        private struct MoveStruct
        {
            public float moveForward;
            public float moveBackward;
            public float moveLeft;
            public float moveRight;
            public float moveUp;
            public float moveDown;

            public void Clear()
            {
                this.moveForward = 0.0f;
                this.moveBackward = 0.0f;
                this.moveLeft = 0.0f;
                this.moveRight = 0.0f;
                this.moveUp = 0.0f;
                this.moveDown = 0.0f;
            }
        }

        private MoveStruct moveStruct = new MoveStruct();

        /// <summary>
        /// Initializes a new instance of the <see cref="MixedRealityCharacterControllerBehavior"/> class.
        /// </summary>
        public MixedRealityCharacterControllerBehavior()
        {
            this.MoveSpeed = 5.0f;
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
        }

        /// <summary>
        /// Updates the entity transform component.
        /// </summary>
        /// <param name="elapsed">Elapsed time in seconds.</param>
        private void UpdatePositionAndOrientation(float elapsed)
        {
            Vector3 displacement = Vector3.Zero;

            var elapsedMaxSpeed = elapsed * this.MoveSpeed * 100;

            if (this.moveStruct.moveForward != 0.0f)
            {
                displacement += this.transform.LocalForward * this.moveStruct.moveForward;
            }
            else if (this.moveStruct.moveBackward != 0.0f)
            {
                displacement += this.transform.LocalBackward * this.moveStruct.moveBackward;
            }

            if (this.moveStruct.moveLeft != 0.0f)
            {
                displacement += this.transform.LocalLeft * this.moveStruct.moveLeft;
            }
            else if (this.moveStruct.moveRight != 0.0f)
            {
                displacement += this.transform.LocalRight * this.moveStruct.moveRight;
            }

            this.characterController.SetVelocity(displacement * elapsedMaxSpeed);
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            this.moveStruct.Clear();
            this.HandleInput();
            this.UpdatePositionAndOrientation((float)gameTime.TotalSeconds);
        }

        /// <summary>
        /// Updates the entity rotation.
        /// </summary>
        /// <param name="angle">Angle.</param>
        public void Rotate(float angle)
        {
            this.characterController.Rotate(angle);
        }
    }
}
