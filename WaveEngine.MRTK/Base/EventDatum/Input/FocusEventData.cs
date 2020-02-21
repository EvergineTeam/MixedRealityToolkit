// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    // TODO: BaseEventData

    /// <summary>
    /// Describes an Input Event associated with a specific pointer's focus state change.
    /// </summary>
    public class FocusEventData ////: BaseEventData
    {
        /// <summary>
        /// Gets the pointer associated with this event.
        /// </summary>
        public IMixedRealityPointer Pointer { get; private set; }

        /// <summary>
        /// Gets the old focused object.
        /// </summary>
        public Entity OldFocusedObject { get; private set; }

        /// <summary>
        /// Gets the new focused object.
        /// </summary>
        public Entity NewFocusedObject { get; private set; }

        // TODO: BaseEventData
        /////// <inheritdoc />
        ////public FocusEventData(EventSystem eventSystem) : base(eventSystem) { }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        public void Initialize(IMixedRealityPointer pointer)
        {
            // TODO: BaseEventData
            ////this.Reset();
            this.Pointer = pointer;
        }

        /// <summary>
        /// Used to initialize/reset the event and populate the data.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="oldFocusedObject">The old focused object.</param>
        /// <param name="newFocusedObject">The new focused object.</param>
        public void Initialize(IMixedRealityPointer pointer, Entity oldFocusedObject, Entity newFocusedObject)
        {
            // TODO: BaseEventData
            ////this.Reset();
            this.Pointer = pointer;
            this.OldFocusedObject = oldFocusedObject;
            this.NewFocusedObject = newFocusedObject;
        }
    }
}
