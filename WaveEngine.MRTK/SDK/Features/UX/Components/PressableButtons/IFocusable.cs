// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Ifocusable interface.
    /// </summary>
    public interface IFocusable
    {
        /// <summary>
        /// Called when focus is gained.
        /// </summary>
        void OnFocusEnter();

        /// <summary>
        /// Called when focus is lost.
        /// </summary>
        void OnFocusExit();
    }
}
