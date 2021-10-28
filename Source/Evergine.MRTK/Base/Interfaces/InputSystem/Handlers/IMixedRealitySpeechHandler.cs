// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

namespace Evergine.MRTK.Base.Interfaces.InputSystem.Handlers
{
    /// <summary>
    /// Interface to receive feedback when speech is recognized.
    /// </summary>
    public interface IMixedRealitySpeechHandler : IMixedRealityEventHandler
    {
        /// <summary>
        /// Notify when a keyword has been recognized.
        /// </summary>
        /// <param name="word">The recognized word.</param>
        void OnSpeechKeywordRecognized(string word);
    }
}
