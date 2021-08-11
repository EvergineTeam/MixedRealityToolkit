// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.MRTK.Base.EventDatum.Input;

namespace WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers
{
    /// <summary>
    /// Implementation of this interface causes a component to receive notifications of focus change events.
    /// </summary>
    public interface IMixedRealityFocusHandler : IMixedRealityEventHandler
    {
        /// <summary>
        /// When a focus change happens, this method is used to notify that the object has gained focus.
        /// </summary>
        /// <param name="eventData">The focus event data.</param>
        void OnFocusEnter(MixedRealityFocusEventData eventData);

        /// <summary>
        /// When a focus change happens, this method is used to notify that the object has lost focus.
        /// </summary>
        /// <param name="eventData">The focus event data.</param>
        void OnFocusExit(MixedRealityFocusEventData eventData);
    }
}
