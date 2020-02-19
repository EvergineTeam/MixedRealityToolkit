using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;

namespace WaveEngine.MRTK.SDK.Features.Input.Handlers.Manipulation
{
    public class SimpleManipulationHandler : Behavior, IMixedRealityPointerHandler
    {
        [BindComponent]
        protected Transform3D transform = null;

        [RenderProperty(Tooltip = "Enable manipulation smoothing")]
        public bool SmoothingActive { get; set; } = true;

        [RenderPropertyAsFInput(Tooltip = "The amount of smoothing to apply to the movement, scale and rotation. 0 means no smoothing, 1 means no change to value", MinLimit = 0, MaxLimit = 1)]
        public float SmoothingAmount { get; set; } = 0.001f;

        public event EventHandler ManipulationStarted;
        public event EventHandler ManipulationEnded;

        private bool lastLeftPressed;
        private bool lastRightPressed;
        private bool leftPressed;
        private bool rightPressed;

        // Transform matrix of the grabbed object in controller space at the moment the grab is started
        private Matrix4x4 grabTransform;

        // Distance between the controllers at the moment the grab is started
        private float grabDistance;

        private Dictionary<Entity, Matrix4x4> activeCursors = new Dictionary<Entity, Matrix4x4>();

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.activeCursors.Clear();
        }

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

        public void OnPointerDragged(MixedRealityPointerEventData eventData)
        {
            var cursor = eventData.Cursor;

            if (this.activeCursors.ContainsKey(cursor))
            {
                this.activeCursors[cursor] = this.CreateCursorTransform(eventData);
            }
        }

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

        public void OnPointerClicked(MixedRealityPointerEventData eventData)
        {
            // Nothing to do
        }

        private Matrix4x4 CreateCursorTransform(MixedRealityPointerEventData eventData)
        {
            return Matrix4x4.CreateFromQuaternion(eventData.Orientation) * Matrix4x4.CreateTranslation(eventData.Position);
        }

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

                this.transform.Position = Vector3.Lerp(this.transform.Position, finalTransform.Translation, lerpAmount);
                this.transform.Orientation = Quaternion.Lerp(this.transform.Orientation, finalTransform.Orientation, lerpAmount);
                this.transform.Scale = Vector3.Lerp(this.transform.Scale, finalTransform.Scale, lerpAmount);
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
