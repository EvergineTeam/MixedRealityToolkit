// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;
using WaveEngine.MRTK.Emulation;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Represent a pressable button.
    /// </summary>
    public class PressableButton : PressableObject, IMixedRealityFocusHandler
    {
        /// <summary>
        /// The button feedback.
        /// </summary>
        [BindComponent(isExactType: false, isRequired: false, source: BindComponentSource.Children)]
        protected List<IPressableButtonFeedback> feedbackVisualsComponents;

        /// <summary>
        /// Gets or sets the speed for retracting the moving button visuals on release.
        /// </summary>
        [RenderProperty(Tooltip = "Speed for retracting the moving button visuals on release.")]
        public float RetractSpeed { get; set; } = 1f;

        /// <summary>
        /// Event fired when the button is pressed.
        /// </summary>
        public event EventHandler ButtonPressed;

        /// <summary>
        /// Event fired when the button is released.
        /// </summary>
        public event EventHandler ButtonReleased;

        private bool isPressing;

        private float currentPosition;

        private IPressableButtonFeedback[] feedbackVisualsComponentsArray;

        private bool simulatePressRequested = false;

        /// <inheritdoc/>
        public void OnFocusEnter(MixedRealityFocusEventData eventData)
        {
            this.RefreshFocusedState(true);
        }

        /// <inheritdoc/>
        public void OnFocusExit(MixedRealityFocusEventData eventData)
        {
            this.RefreshFocusedState(false);
        }

        /// <inheritdoc/>
        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.currentPosition = this.StartPosition;
        }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            this.feedbackVisualsComponentsArray = this.feedbackVisualsComponents?.ToArray();

            return true;
        }

        /// <inheritdoc/>
        protected override void OnDetach()
        {
            base.OnDetach();
            this.feedbackVisualsComponentsArray = null;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            base.Update(gameTime);

            float targetPosition;

            if (this.simulatePressRequested)
            {
                if (this.currentPosition == this.EndPosition)
                {
                    this.simulatePressRequested = false;
                    targetPosition = this.StartPosition;
                }
                else
                {
                    targetPosition = this.EndPosition;
                }
            }
            else
            {
                if (this.IsTouching)
                {
                    // Get the farthest pushed distance from the initial position
                    var farthestPosition = this.cursorDistances.Values.Min();
                    targetPosition = MathHelper.Clamp(farthestPosition, this.EndPosition, this.StartPosition);
                }
                else
                {
                    targetPosition = this.StartPosition;
                }
            }

            if (this.currentPosition != targetPosition)
            {
                // Calculate the error from the current to the target position
                var error = targetPosition - this.currentPosition;

                // Clamp the error to the max retract speed
                var returnDistance = Math.Abs(this.RetractSpeed * (float)gameTime.TotalSeconds);
                float difference = Math.Min(error, returnDistance);

                // Modify the current position
                this.currentPosition += difference;

                // Update the internal pressed state and raise events
                this.UpdatePressedState();

                // Call feedback function for feedback visuals
                if (this.feedbackVisualsComponentsArray != null && this.feedbackVisualsComponentsArray.Length > 0)
                {
                    var colliderWorldTransform = this.nearInteractionTouchable.BoxCollider3DTransform * this.transform.WorldTransform;
                    var pushVector = this.nearInteractionTouchable.LocalPressDirection * (this.currentPosition - 0.5f);
                    var pushVectorWorld = Vector3.TransformNormal(pushVector, colliderWorldTransform);

                    var pressRatio = (this.StartPosition - this.currentPosition) / (this.StartPosition - this.EndPosition);
                    for (int i = 0; i < this.feedbackVisualsComponentsArray.Length; i++)
                    {
                        this.feedbackVisualsComponentsArray[i].Feedback(pushVectorWorld, pressRatio, this.isPressing);
                    }
                }
            }
        }

        private void RefreshFocusedState(bool focused)
        {
            if (this.feedbackVisualsComponentsArray != null)
            {
                for (int i = 0; i < this.feedbackVisualsComponentsArray.Length; i++)
                {
                    this.feedbackVisualsComponentsArray[i].FocusChanged(focused);
                }
            }
        }

        /// <summary>
        /// Simulates a button press.
        /// </summary>
        public void SimulatePress()
        {
            this.simulatePressRequested = true;
        }

        /// <summary>
        /// Fires the <see cref="ButtonPressed"/> and <see cref="ButtonReleased"/> events without triggering animation changes.
        /// </summary>
        public void ForceFireEvents()
        {
            this.ButtonPressed?.Invoke(this, EventArgs.Empty);
            this.ButtonReleased?.Invoke(this, EventArgs.Empty);
        }

        private void UpdatePressedState()
        {
            if (this.GetNewPressedState(this.isPressing, this.currentPosition, out var newPressedState))
            {
                this.isPressing = newPressedState;

                if (newPressedState)
                {
                    this.ButtonPressed?.Invoke(this, EventArgs.Empty);
                    this.PulseProximityLight();
                }
                else
                {
                    this.ButtonReleased?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void PulseProximityLight()
        {
            // Pulse each proximity light on pointer cursors interacting with this button.
            if (this.cursorDistances.Keys.Count != 0)
            {
                foreach (var pointer in this.cursorDistances.Keys)
                {
                    var proximityLights = pointer.Owner.FindComponentsInChildren<ProximityLight>();
                    foreach (var proximityLight in proximityLights)
                    {
                        proximityLight.Pulse();
                    }
                }
            }
        }
    }
}
