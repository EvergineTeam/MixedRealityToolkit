﻿// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Interface to receive the feedback when a speech is recognized.
    /// </summary>
    public interface ISpeechHandler : IMixedRealityEventHandler
    {
        /// <summary>
        /// Notify when a keyworkd has been recognized.
        /// </summary>
        /// <param name="word">The workd rezognized.</param>
        void OnSpeechKeywordRecognized(string word);
    }
}
