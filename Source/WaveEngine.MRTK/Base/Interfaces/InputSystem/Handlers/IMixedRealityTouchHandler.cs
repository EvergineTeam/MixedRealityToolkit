// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.MRTK.Base.EventDatum.Input;

namespace WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers
{
    /// <summary>
    /// Implementation of this interface causes a component to receive notifications of Touch events from HandTrackingInputSources.
    /// </summary>
    public interface IMixedRealityTouchHandler : IMixedRealityEventHandler
    {
        /// <summary>
        /// When a Touch motion has occurred, this handler receives the event.
        /// </summary>
        /// <remarks>
        /// A Touch motion is defined as occurring within the bounds of an object (transitive).
        /// </remarks>
        /// <param name="eventData">Contains information about the HandTrackingInputSource.</param>
        void OnTouchStarted(HandTrackingInputEventData eventData);

        /// <summary>
        /// When a Touch motion is updated, this handler receives the event.
        /// </summary>
        /// <remarks>
        /// A Touch motion is defined as occurring within the bounds of an object (transitive).
        /// </remarks>
        /// <param name="eventData">Contains information about the HandTrackingInputSource.</param>
        void OnTouchUpdated(HandTrackingInputEventData eventData);

        /// <summary>
        /// When a Touch motion ends, this handler receives the event.
        /// </summary>
        /// <remarks>
        /// A Touch motion is defined as occurring within the bounds of an object (transitive).
        /// </remarks>
        /// <param name="eventData">Contains information about the HandTrackingInputSource.</param>
        void OnTouchCompleted(HandTrackingInputEventData eventData);
    }
}
