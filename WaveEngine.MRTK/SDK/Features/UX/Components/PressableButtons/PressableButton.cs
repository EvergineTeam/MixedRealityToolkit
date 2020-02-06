using System;
using System.Linq;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    public class PressableButton : PressableObject
    {
        [BindComponent(isExactType: false, isRequired: false, source: BindComponentSource.Children)]
        protected IPressableButtonFeedback movingVisualsFeedback;

        [RenderProperty(Tooltip = "Speed for retracting the moving button visuals on release.")]
        public float RetractSpeed { get; set; } = 1f;

        public event EventHandler ButtonPressed;
        public event EventHandler ButtonReleased;

        private bool isPressing;

        private float currentPosition;

        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.currentPosition = this.StartPosition;
        }

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

                    this.movingVisualsFeedback.Feedback(pushVector, colliderTransform, pressRatio, this.isPressing);
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
