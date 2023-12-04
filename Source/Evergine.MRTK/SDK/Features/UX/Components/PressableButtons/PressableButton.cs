// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Evergine.Common.Attributes;
using Evergine.Framework;
using Evergine.Framework.Services;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.Emulation;

namespace Evergine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Represent a pressable button.
    /// </summary>
    public class PressableButton : PressableObject, IMixedRealityFocusHandler
    {
        private Clock clock;

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
        /// Gets or sets a value that allows to press a button by focusing during a period of time (in seconds). A value of 0 disable this feature.
        /// </summary>
        [RenderProperty(Tooltip = "Allow to press a button by focusing during a period of time (in seconds). A value of 0 disable this feature.")]
        public TimeSpan FocusSelectionTime { get; set; } = TimeSpan.Zero;

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

        private TimeSpan? focusPressTimeout;

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

            if (this.focusPressTimeout.HasValue)
            {
                var timeoutRemain = (float)(this.focusPressTimeout.Value.TotalSeconds - this.clock.TotalTime.TotalSeconds);
                var focusTimeout = 1 - (float)(timeoutRemain / this.FocusSelectionTime.TotalSeconds);
                this.RefreshFocusTimeoutState(focusTimeout);

                if (timeoutRemain <= 0)
                {
                    targetPosition = this.EndPosition;
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
            // In case that the button has enabled the focus selection, set the timeout.
            if (this.FocusSelectionTime.TotalSeconds > 0)
            {
                if (focused)
                {
                    this.clock = this.clock ?? Application.Current.Container.Resolve<Clock>();
                    this.focusPressTimeout = this.clock.TotalTime + this.FocusSelectionTime;
                }
                else
                {
                    this.focusPressTimeout = null;
                }
            }

            if (this.feedbackVisualsComponentsArray != null)
            {
                for (int i = 0; i < this.feedbackVisualsComponentsArray.Length; i++)
                {
                    this.feedbackVisualsComponentsArray[i].FocusChanged(focused);
                }
            }
        }

        private void RefreshFocusTimeoutState(float focusTimeout)
        {
            if (this.feedbackVisualsComponentsArray != null)
            {
                for (int i = 0; i < this.feedbackVisualsComponentsArray.Length; i++)
                {
                    this.feedbackVisualsComponentsArray[i].FocusTimeout(focusTimeout);
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
            this.focusPressTimeout = null;
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
