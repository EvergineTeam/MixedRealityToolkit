// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Represent a pressable button.
    /// </summary>
    public class PressableButton : PressableObject
    {
        /// <summary>
        /// The button feedback.
        /// </summary>
        [BindComponent(isExactType: false, isRequired: false, source: BindComponentSource.Children)]
        protected IPressableButtonFeedback movingVisualsFeedback;

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

        /// <inheritdoc/>
        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.currentPosition = this.StartPosition;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            float targetPosition;

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

                // Call feedback function for moving visuals
                if (this.movingVisualsFeedback != null)
                {
                    Vector3 pushVector = this.nearInteractionTouchable.LocalPressDirection * (this.currentPosition - 0.5f);
                    var colliderTransform = this.nearInteractionTouchable.BoxCollider3DTransform;
                    var pressRatio = (this.StartPosition - this.currentPosition) / (this.StartPosition - this.EndPosition);

                    Vector3 pushVectorWorld = Vector3.TransformNormal(Vector3.TransformNormal(pushVector, colliderTransform), this.transform.WorldTransform);
                    this.movingVisualsFeedback.Feedback(pushVectorWorld, colliderTransform, pressRatio, this.isPressing);
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
                }
                else
                {
                    this.ButtonReleased?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }
}
