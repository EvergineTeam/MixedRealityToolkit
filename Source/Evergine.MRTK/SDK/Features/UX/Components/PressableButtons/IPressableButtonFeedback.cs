// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Mathematics;

namespace Evergine.MRTK.SDK.Features.UX.Components.PressableButtons
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
        /// <param name="pressRatio">The press ratio.</param>
        /// <param name="pressed">If the button is pressed.</param>
        void Feedback(Vector3 pushVector, float pressRatio, bool pressed);

        /// <summary>
        /// Notify when the button focus state changes.
        /// </summary>
        /// <param name="focus">Whether the button is in focus.</param>
        void FocusChanged(bool focus);

        /// <summary>
        /// Notify while the button is focused and need to apply a feedback.
        /// </summary>
        /// <param name="timeout">The timeout ratio. from [0-1] range, while 0 us at the begining of focus, and 1 when the button has been focused during button.FocusSelectionTime time.</param>
        void FocusTimeout(float timeout);
    }
}
