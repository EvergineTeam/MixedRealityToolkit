// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Mathematics;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Interface to receive the feedback when a button is pressed.
    /// </summary>
    public interface IPressableButtonFeedback
    {
        /// <summary>
        /// Notify when the button is pressed and need to apply a feedback.
        /// </summary>
        /// <param name="pushVector">The push direction.</param>
        /// <param name="colliderTransform">The collider transform.</param>
        /// <param name="pressRatio">The press ratio.</param>
        /// <param name="pressed">If the button is pressed.</param>
        void Feedback(Vector3 pushVector, Matrix4x4 colliderTransform, float pressRatio, bool pressed);
    }
}
