// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System.Collections.Generic;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.MixedReality.Toolkit.Physics;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Delegate type used to handle primary pointer changes.
    /// Old and new pointer values can be null to indicate transition from or to no primary pointer, but they won't both be null simultaneously.
    /// </summary>
    public delegate void PrimaryPointerChangedHandler(IMixedRealityPointer oldPointer, IMixedRealityPointer newPointer);

    // TODO: IMixedRealityService
    // TODO: IMixedRealitySourceStateHandler
    // TODO: IMixedRealitySpeechHandler

    /// <summary>
    /// Implements the Focus Provider for handling focus of pointers.
    /// </summary>
    public interface IMixedRealityFocusProvider ////: IMixedRealityService, IMixedRealitySourceStateHandler, IMixedRealitySpeechHandler
    {
        /// <summary>
        /// Gets the maximum distance at which all pointers can collide with a <see href="https://docs.unity3d.com/ScriptReference/Entity.html">Entity</see>, unless it has an override extent.
        /// </summary>
        float GlobalPointingExtent { get; }

        /// <summary>
        /// Gets the layer masks for the focus pointers to raycast against.
        /// </summary>
        CollisionCategory3D FocusLayerMasks { get; }

        /// <summary>
        /// Gets the Camera the <see href="https://docs.unity3d.com/ScriptReference/EventSystems.EventSystem.html">EventSystem</see> uses to raycast against.
        /// </summary>
        /// <remarks>Every uGUI canvas in your scene should use this camera as its event camera.</remarks>
        Camera RaycastCamera { get; }

        /// <summary>
        /// Gets the current primary pointer. Determined by the primary pointer selector in use (see MixedRealityPointerProfile.PrimaryPointerSelector).
        /// </summary>
        IMixedRealityPointer PrimaryPointer { get; }

        /// <summary>
        /// Gets the currently focused object for the pointing source.
        /// </summary>
        /// <remarks>If the pointing source is not registered, then the Gaze's Focused <see cref="Entity"/> is returned.</remarks>
        /// <param name="pointingSource">The pointer.</param>
        /// <returns>Currently Focused Object.</returns>
        Entity GetFocusedEntity(IMixedRealityPointer pointingSource);

        /// <summary>
        /// Gets the currently focused object for the pointing source.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="focusDetails">The focus details.</param>
        /// <returns>If there is a valid object focused.</returns>
        bool TryGetFocusDetails(IMixedRealityPointer pointer, out FocusDetails focusDetails);

        /// <summary>
        /// Sets the FocusDetails of the specified pointer, overriding the focus point that was currently set. This can be used to change
        /// the FocusDetails of a specific pointer even if focus is locked.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <param name="focusDetails">The focus details.</param>
        /// <returns>
        /// True if the FocusDetails were set successfully. False if the pointer is not associated with the FocusProvider.
        /// </returns>
        bool TryOverrideFocusDetails(IMixedRealityPointer pointer, FocusDetails focusDetails);

        /// <summary>
        /// Generate a new unique pointer id.
        /// </summary>
        /// <returns>the new Id.</returns>
        uint GenerateNewPointerId();

        /// <summary>
        /// Checks if the pointer is registered with the Focus Manager.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <returns>True, if registered, otherwise false.</returns>
        bool IsPointerRegistered(IMixedRealityPointer pointer);

        /// <summary>
        /// Registers the pointer with the Focus Manager.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <returns>True, if the pointer was registered, false if the pointer was previously registered.</returns>
        bool RegisterPointer(IMixedRealityPointer pointer);

        /// <summary>
        /// Unregisters the pointer with the Focus Manager.
        /// </summary>
        /// <param name="pointer">The pointer.</param>
        /// <returns>True, if the pointer was unregistered, false if the pointer was not registered.</returns>
        bool UnregisterPointer(IMixedRealityPointer pointer);

        /// <summary>
        /// Provides access to all registered pointers of a specified type.
        /// </summary>
        /// <typeparam name="T">The type of pointers to request. Use IMixedRealityPointer to access all pointers.</typeparam>
        /// <returns>The pointer collection.</returns>
        IEnumerable<T> GetPointers<T>()
            where T : class, IMixedRealityPointer;

        /// <summary>
        /// Subscribes to primary pointer changes.
        /// </summary>
        /// <param name="handler">Handler to be called when the primary pointer changes.</param>
        /// <param name="invokeHandlerWithCurrentPointer">When true, the passed in handler will be invoked immediately with the current primary pointer
        /// before subscribing. This is useful to avoid having to manually poll the current value.</param>
        void SubscribeToPrimaryPointerChanged(PrimaryPointerChangedHandler handler, bool invokeHandlerWithCurrentPointer);

        /// <summary>
        /// Unsubscribes from primary pointer changes.
        /// </summary>
        /// <param name="handler">Handler to unsubscribe.</param>
        void UnsubscribeFromPrimaryPointerChanged(PrimaryPointerChangedHandler handler);
    }
}
