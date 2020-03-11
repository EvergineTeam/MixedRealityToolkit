// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;

namespace WaveEngine.MRTK.SDK.Features.Input.Handlers.Manipulation
{
    /// <summary>
    /// A simple manipulation handler.
    /// </summary>
    public class SimpleManipulationHandler : Behavior, IMixedRealityPointerHandler
    {
        /// <summary>
        /// The transform.
        /// </summary>
        [BindComponent]
        protected Transform3D transform = null;

        /// <summary>
        /// The rigid body.
        /// </summary>
        [BindComponent(isRequired: false)]
        protected RigidBody3D rigidBody;

        /// <summary>
        /// Gets or sets a value indicating whether the manipulation smoothing is enabled.
        /// </summary>
        [RenderProperty(Tooltip = "Enable manipulation smoothing")]
        public bool SmoothingActive { get; set; } = true;

        /// <summary>
        /// Gets  or sets the amount of smoothing to apply to the movement, scale and rotation. 0 means no smoothing, 1 means no change to value.
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "The amount of smoothing to apply to the movement, scale and rotation. 0 means no smoothing, 1 means no change to value", MinLimit = 0, MaxLimit = 1)]
        public float SmoothingAmount { get; set; } = 0.001f;

        /// <summary>
        /// The manipulation started event.
        /// </summary>
        public event EventHandler ManipulationStarted;

        /// <summary>
        /// The manipulation ended event.
        /// </summary>
        public event EventHandler ManipulationEnded;

        private bool lastLeftPressed;
        private bool lastRightPressed;
        private bool leftPressed;
        private bool rightPressed;

        // Transform matrix of the grabbed object in controller space at the moment the grab is started
        private Matrix4x4 grabTransform;

        // Distance between the controllers at the moment the grab is started
        private float grabDistance;

        private readonly Dictionary<Entity, Matrix4x4> activeCursors = new Dictionary<Entity, Matrix4x4>();

        private Vector3 originalPosition;
        private Quaternion originalRotation;

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.activeCursors.Clear();
        }

        /// <inheritdoc/>
        public void OnPointerDown(MixedRealityPointerEventData eventData)
        {
            var cursor = eventData.Cursor;

            if (!this.activeCursors.ContainsKey(cursor))
            {
                this.activeCursors[cursor] = this.CreateCursorTransform(eventData);

                if (this.activeCursors.Count == 1)
                {
                    this.ManipulationStarted?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc/>
        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            var cursor = eventData.Cursor;

            if (this.activeCursors.ContainsKey(cursor))
            {
                this.activeCursors[cursor] = this.CreateCursorTransform(eventData);
            }
        }

        /// <inheritdoc/>
        public void OnPointerUp(MixedRealityPointerEventData eventData)
        {
            var cursor = eventData.Cursor;

            if (this.activeCursors.ContainsKey(cursor))
            {
                this.activeCursors.Remove(cursor);

                if (this.activeCursors.Count == 0)
                {
                    this.ManipulationEnded?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <inheritdoc/>
        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            // Nothing to do
        }

        private Matrix4x4 CreateCursorTransform(MixedRealityPointerEventData eventData)
        {
            return Matrix4x4.CreateFromQuaternion(eventData.Orientation) * Matrix4x4.CreateTranslation(eventData.Position);
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            this.originalPosition = this.transform.Position;
            this.originalRotation = this.transform.Orientation;

            // Ensure this is always updated after the rigidbody
            if (this.rigidBody != null)
            {
                this.UpdateOrder = this.rigidBody.UpdateOrder + 0.1f;

                // Hack: it seems that a Reset Transform is mandatory, and it needs to be different
                this.rigidBody.ResetTransform(this.originalPosition * 10000f, this.originalRotation, this.transform.Scale);
            }
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            var values = this.activeCursors.Values;
            var leftPressed = values.Count > 0;
            var rightPressed = values.Count > 1;
            var leftTransform = values.FirstOrDefault();
            var rightTransform = values.Skip(1).FirstOrDefault();

            this.DoTransform(leftPressed, rightPressed, leftTransform, rightTransform, (float)gameTime.TotalSeconds);
        }

        private void DoTransform(bool leftPressed, bool rightPressed, Matrix4x4 leftTransform, Matrix4x4 rightTransform, float timeStep)
        {
            // Update press states
            this.lastLeftPressed = this.leftPressed;
            this.lastRightPressed = this.rightPressed;
            this.leftPressed = leftPressed;
            this.rightPressed = rightPressed;

            // Apply new transform if any of the controllers is pressed
            if (this.leftPressed || this.rightPressed)
            {
                // Check whether the presses changed in this update cycle
                bool leftPressedChanged = this.leftPressed != this.lastLeftPressed;
                bool rightPressedChanged = this.rightPressed != this.lastRightPressed;

                // Current manipulation controller transform
                Matrix4x4 controllerTransform = Matrix4x4.Identity;

                if (this.leftPressed)
                {
                    if (this.rightPressed)
                    {
                        // Calculate two-hand combined world transform
                        Vector3 right = rightTransform.Translation - leftTransform.Translation;
                        Vector3 up_avg = Vector3.Lerp(leftTransform.Up, rightTransform.Up, 0.5f);

                        Vector3 position = Vector3.Lerp(leftTransform.Translation, rightTransform.Translation, 0.5f);
                        Vector3 forward = Vector3.Cross(up_avg, right);
                        Vector3 up = Vector3.Cross(right, forward);

                        controllerTransform = Matrix4x4.CreateWorld(position, forward, up);

                        // Calculate current distance between the controllers
                        var currentDistance = right.Length();

                        // Update grab distance if any of the presses changed
                        if (leftPressedChanged || rightPressedChanged)
                        {
                            this.grabDistance = currentDistance;
                        }

                        // Calculate target scale
                        var scale = currentDistance / this.grabDistance;

                        // Final controller transform
                        controllerTransform = Matrix4x4.CreateScale(scale) * controllerTransform;
                    }
                    else
                    {
                        // Set controller transform to the left transform
                        controllerTransform = leftTransform;
                    }
                }
                else if (this.rightPressed)
                {
                    // Set controller transform to the right transform
                    controllerTransform = rightTransform;
                }

                // Update grab transform matrix if any of the presses changed
                if (leftPressedChanged || rightPressedChanged)
                {
                    this.grabTransform = this.transform.WorldTransform * Matrix4x4.Invert(controllerTransform);
                }

                // Calculate final transformation
                Matrix4x4 finalTransform = this.grabTransform * controllerTransform;

                // Update object transform
                float lerpAmount = this.GetLerpAmount(timeStep);

                if (this.rigidBody == null)
                {
                    this.transform.Position = Vector3.Lerp(this.transform.Position, finalTransform.Translation, lerpAmount);
                    this.transform.Orientation = Quaternion.Lerp(this.transform.Orientation, finalTransform.Orientation, lerpAmount);
                    this.transform.Scale = Vector3.Lerp(this.transform.Scale, finalTransform.Scale, lerpAmount);
                }
                else
                {
                    this.rigidBody.LinearVelocity = (finalTransform.Translation - this.transform.Position) / timeStep;
                    this.rigidBody.AngularVelocity = Quaternion.ToEuler(finalTransform.Orientation * Quaternion.Inverse(this.transform.Orientation)) / timeStep;
                    if (this.transform.Scale != finalTransform.Scale)
                    {
                        this.rigidBody.ResetTransform(finalTransform.Translation, finalTransform.Orientation, finalTransform.Scale);
                        this.transform.Scale = finalTransform.Scale;
                    }

                    this.rigidBody.WakeUp();
                }
            }

            if (this.rigidBody != null && (this.transform.Position - this.originalPosition).LengthSquared() > 0.5f)
            {
                this.rigidBody.ResetTransform(this.originalPosition, this.originalRotation, this.transform.Scale);
                this.rigidBody.LinearVelocity = Vector3.Zero;
                this.rigidBody.AngularVelocity = Vector3.Zero;
            }
        }

        private float GetLerpAmount(float timeStep)
        {
            if (!this.SmoothingActive || this.SmoothingAmount == 0)
            {
                return 1;
            }

            // www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp/
            return 1.0f - (float)Math.Pow(this.SmoothingAmount, timeStep);
        }
    }
}
