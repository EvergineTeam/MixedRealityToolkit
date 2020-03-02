// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;
using WaveEngine.Framework.Physics3D;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Interface to implement to react to focus enter/exit.
    /// </summary>
    /// <remarks>
    /// The events on this interface are related to those of <see cref="IMixedRealityFocusChangedHandler"/>, whose event have
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
    public interface IMixedRealityFocusHandler ////: IEventSystemHandler
    {
        /// <summary>
        /// The Focus Enter event is raised on this <see cref="Entity"/> whenever a <see cref="IMixedRealityPointer"/>'s focus enters this <see cref="Entity"/>'s <see cref="Collider3D"/>.
        /// </summary>
        /// <param name="eventData">The focus efent data.</param>
        void OnFocusEnter(FocusEventData eventData);

        /// <summary>
        /// The Focus Exit event is raised on this <see cref="Entity"/> whenever a <see cref="IMixedRealityPointer"/>'s focus leaves this <see cref="Entity"/>'s <see cref="Collider3D"/>.
        /// </summary>
        /// <param name="eventData">The focus efent data.</param>
        void OnFocusExit(FocusEventData eventData);
    }
}
