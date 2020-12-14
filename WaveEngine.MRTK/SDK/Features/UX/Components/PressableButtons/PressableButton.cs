// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Emulation;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Represent a pressable button.
    /// </summary>
    public class PressableButton : PressableObject, ISpeechHandler, IFocusable
    {
        /// <summary>
        /// The button feedback.
        /// </summary>
        [BindComponent(isExactType: false, isRequired: false, source: BindComponentSource.Children)]
        protected List<IPressableButtonFeedback> feedbackVisualsComponents;

        /// <summary>
        /// The "See It Say It" label entity. It should be marked with the tag "SeeItSayItLabel".
        /// </summary>
        [BindEntity(tag: "SeeItSayItLabel", isRequired: false, source: BindEntitySource.Children)]
        protected Entity seeItSayItLabel;

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

        /// <summary>
        /// Event fired when any button is pressed
        /// </summary>
        public static event EventHandler AnyButtonReleased;

        /// <summary>
        ///  Gets or sets the word that will make this object be pressed.
        /// </summary>
        public string SpeechKeyWord { get; set; } = string.Empty;

        private bool isPressing;

        private float currentPosition;

        private IPressableButtonFeedback[] feedbackVisualsComponentsArray;

        private bool speechWordRecognized = false;

        /// <inheritdoc/>
        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.currentPosition = this.StartPosition;
        }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.feedbackVisualsComponentsArray = this.feedbackVisualsComponents?.ToArray();

                if (this.seeItSayItLabel != null)
                {
                    this.seeItSayItLabel.IsEnabled = false;
                }
            }

            return attached;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            float targetPosition;

            if (this.speechWordRecognized)
            {
                if (this.currentPosition == this.EndPosition)
                {
                    this.speechWordRecognized = false;
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
                    Vector3 pushVector = this.nearInteractionTouchable.LocalPressDirection * (this.currentPosition - 0.5f);
                    var colliderTransform = this.nearInteractionTouchable.BoxCollider3DTransform;
                    var pressRatio = (this.StartPosition - this.currentPosition) / (this.StartPosition - this.EndPosition);

                    Vector3 pushVectorCollider = Vector3.TransformNormal(pushVector, colliderTransform);
                    Vector3 pushVectorWorld = Vector3.TransformNormal(pushVectorCollider, this.transform.WorldTransform);

                    for (int i = 0; i < this.feedbackVisualsComponentsArray.Length; i++)
                    {
                        this.feedbackVisualsComponentsArray[i].Feedback(pushVectorWorld, pressRatio, this.isPressing);
                    }
                }
            }
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
                    AnyButtonReleased?.Invoke(this, EventArgs.Empty);
                    this.ButtonReleased?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void PulseProximityLight()
        {
            // Pulse each proximity light on pointer cursors' interacting with this button.
            if (this.cursorDistances.Keys.Count != 0)
            {
                foreach (var pointer in this.cursorDistances.Keys)
                {
                    var proximityLights = pointer.FindComponentsInChildren<ProximityLight>();
                    foreach (var proximityLight in proximityLights)
                    {
                        proximityLight.Pulse();
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void OnSpeechKeywordRecognized(string word)
        {
            this.speechWordRecognized = this.SpeechKeyWord == word;
        }

        /// <inheritdoc/>
        public void OnFocusEnter()
        {
            if (this.seeItSayItLabel != null)
            {
                this.seeItSayItLabel.IsEnabled = true;
            }
        }

        /// <inheritdoc/>
        public void OnFocusExit()
        {
            if (this.seeItSayItLabel != null)
            {
                this.seeItSayItLabel.IsEnabled = false;
            }
        }
    }
}
