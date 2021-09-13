// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Framework;

namespace WaveEngine.MRTK.SDK.Features.Input.Handlers
{
    /// <summary>
    /// Condition to fire a <see cref="SpeechHandler"/> keyword recognition event.
    /// </summary>
    public enum SpeechHandlerFireCondition
    {
        /// <summary>
        /// The event will always be fired.
        /// </summary>
        Global,

        /// <summary>
        /// The event will be fired only if the owner <see cref="Entity"/> is enabled.
        /// </summary>
        Enabled,

        /// <summary>
        /// The event will be fired only if the owner <see cref="Entity"/> is enabled and it has focus.
        /// </summary>
        EnabledAndFocus,
    }
}
