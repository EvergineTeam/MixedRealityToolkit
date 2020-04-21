// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Text;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Interface to receive the feedback when a speech is recognized.
    /// </summary>
    public interface ISpeechHandler
    {
        /// <summary>
        /// Notify when a keyworkd has been recognized.
        /// </summary>
        /// <param name="word">The workd rezognized.</param>
        void OnSpeechKeywordRecognized(string word);
    }
}
