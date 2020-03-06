// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using WaveEngine.EventSystems;
using WaveEngine.Framework;

namespace WaveEngine.MixedReality.Toolkit
{
    /// <summary>
    /// Interface used to implement an Event System that is compatible with the Mixed Reality Toolkit.
    /// </summary>
    public interface IMixedRealityEventSystem : IMixedRealityService
    {
        /// <summary>
        /// Gets the list of event listeners that are registered to this Event System.
        /// </summary>
        /// <remarks>
        /// This collection is obsolete and is replaced by handler-based internal storage. It will be removed in a future release.
        /// </remarks>
        List<Entity> EventListeners { get; }

        /// <summary>
        /// The main function for handling and forwarding all events to their intended recipients.
        /// </summary>
        /// <typeparam name="T">Event Handler Interface Type.</typeparam>
        /// <param name="eventData">Event Data.</param>
        /// <param name="eventHandler">Event Handler delegate.</param>
        void HandleEvent<T>(BaseEventData eventData, ExecuteEvents.EventFunction<T> eventHandler)
            where T : IEventSystemHandler;

        /// <summary>
        /// Registers a <see cref="Entity"/> to listen for events from this Event System.
        /// </summary>
        /// <param name="listener"><see cref="Entity"/> to add to <see cref="EventListeners"/>.</param>
        [Obsolete("Register using a game object causes all components of this object to receive global events of all types. " +
            "Use RegisterHandler<> methods instead to avoid unexpected behavior.")]
        void Register(Entity listener);

        /// <summary>
        /// Unregisters a <see cref="Entity"/> from listening for events from this Event System.
        /// </summary>
        /// <param name="listener"><see cref="Entity"/> to remove from <see cref="EventListeners"/>.</param>
        [Obsolete("Unregister using a game object will disable listening of global events for all components of this object. " +
            "Use UnregisterHandler<> methods instead to avoid unexpected behavior.")]
        void Unregister(Entity listener);

        /// <summary>
        /// Registers the given handler as a global listener for all events handled via the T interface.
        /// T must be an interface type, not a class type, derived from IEventSystemHandler.
        /// </summary>
        /// <remarks>
        /// If you want to register a single C# object as global handler for several event handling interfaces,
        /// you must call this function for each interface type.
        /// </remarks>
        /// <param name="handler">Handler to receive global input events of specified handler type.</param>
        /// <typeparam name="T">The event system type.</typeparam>
        void RegisterHandler<T>(IEventSystemHandler handler)
            where T : IEventSystemHandler;

        /// <summary>
        /// Unregisters the given handler as a global listener for all events handled via the T interface.
        /// T must be an interface type, not a class type, derived from IEventSystemHandler.
        /// </summary>
        /// <remarks>
        /// If a single C# object listens to global input events for several event handling interfaces,
        /// you must call this function for each interface type.
        /// </remarks>
        /// <param name="handler">Handler to stop receiving global input events of specified handler type.</param>
        /// <typeparam name="T">The event system type.</typeparam>
        void UnregisterHandler<T>(IEventSystemHandler handler)
            where T : IEventSystemHandler;
    }
}
