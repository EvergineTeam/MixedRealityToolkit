// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Interface to implement to react to focus changed events.
    /// </summary>
    /// <remarks>
    /// The events on this interface are related to those of <see cref="IMixedRealityFocusHandler"/>, whose event have
    /// a known ordering with this interface:
    ///
    /// IMixedRealityFocusChangedHandler::OnBeforeFocusChange
    /// IMixedRealityFocusHandler::OnFocusEnter
    /// IMixedRealityFocusHandler::OnFocusExit
    /// IMixedRealityFocusChangedHandler::OnFocusChanged
    ///
    /// Because these two interfaces are different, consumers must be wary about having nested
    /// hierarchies where some game objects will implement both interfaces, and more deeply nested
    /// object within the same parent-child chain that implement a single one of these - such
    /// a presence can lead to scenarios where one interface is invoked on the child object, and then
    /// the other interface is invoked on the parent object (thus, the parent would "miss" getting
    /// the event that the child had already processed).
    /// </remarks>
    public interface IMixedRealityFocusChangedHandler
    {
        /// <summary>
        /// Focus event that is raised before the focus is actually changed.
        /// </summary>
        /// <param name="eventData">The focus event data.</param>
        /// <remarks>Useful for logic that needs to take place before focus changes.</remarks>
        void OnBeforeFocusChange(FocusEventData eventData);

        /// <summary>
        /// Focus event that is raised when the focused object is changed.
        /// </summary>
        /// <param name="eventData">The focus event data.</param>
        void OnFocusChanged(FocusEventData eventData);
    }
}
