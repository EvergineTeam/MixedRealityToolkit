// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;

namespace Evergine.MRTK.Emulation
{
    /// <summary>
    /// Interface for entities receiving command services.
    /// </summary>
    public interface IVoiceCommandService
    {
        /// <summary>
        /// Configure a set of commands to be used by the service for recognition.
        /// </summary>
        /// <param name="voiceCommands">A sets of voice commands to be used by the service for recognition.</param>
        void ConfigureVoiceCommands(string[] voiceCommands);

        /// <summary>
        /// Fired when an speech word has been recognized.
        /// </summary>
        event EventHandler<string> CommandRecognized;
    }
}
