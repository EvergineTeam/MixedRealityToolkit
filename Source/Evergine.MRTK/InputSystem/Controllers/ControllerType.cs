// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

namespace Evergine.MRTK.InputSystem.Controllers
{
    /// <summary>
    /// The controller type.
    /// </summary>
    public enum ControllerType
    {
        /// <summary>
        /// Default type
        /// </summary>
        None = 0,

        /// <summary>
        /// Controller for an XR physical controller
        /// </summary>
        XRPhysicalController,

        /// <summary>
        /// Controller for an XR articulated hand
        /// </summary>
        XRArticulatedHand,
    }
}
