// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Text;

namespace WaveEngine_MRTK.Emulation
{
    /// <summary>
    /// Interface for entities receiving command services.
    /// </summary>
    public interface IVoiceCommandService
    {
        /// <summary>
        /// Fired when an speech word has been recognized.
        /// </summary>
        event EventHandler<string> CommandRecognized;
    }
}
