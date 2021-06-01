// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.MRTK.Base.EventDatum.Input;

namespace WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers
{
    /// <summary>
    /// Implementation of this interface causes a component to receive notifications of Pointer events.
    /// </summary>
    public interface IMixedRealityPointerHandler : IMixedRealityEventHandler
    {
        /// <summary>
        /// When a pointer down event is raised, this method is used to pass along the event data to the input handler.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        void OnPointerDown(MixedRealityPointerEventData eventData);

        /// <summary>
        /// Called every frame a pointer is down. Can be used to implement drag-like behaviors.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        void OnPointerDragged(MixedRealityPointerEventData eventData);

        /// <summary>
        /// When a pointer up event is raised, this method is used to pass along the event data to the input handler.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        void OnPointerUp(MixedRealityPointerEventData eventData);

        /// <summary>
        /// When a pointer clicked event is raised, this method is used to pass along the event data to the input handler.
        /// </summary>
        /// <param name="eventData">The pointer event data.</param>
        void OnPointerClicked(MixedRealityPointerEventData eventData);
    }
}
