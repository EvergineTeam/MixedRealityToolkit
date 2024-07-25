// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Mathematics;
using Evergine.MRTK.Emulation;

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
        /// <param name="cursor">Cursor that is provoking the focus change.</param>
        /// <param name="focus">Whether the button is in focus.</param>
        void FocusChanged(Cursor cursor, bool focus);
    }
}
