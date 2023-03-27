// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

namespace Evergine.MRTK.Managers.Data
{
    /// <summary>
    /// The controller type for a pointer option.
    /// </summary>
    public enum ControllerType
    {
        /// <summary>
        /// Default type
        /// </summary>
        None = 0,

        /// <summary>
        /// Controller for an OpenXR physical controller
        /// </summary>
        OpenXRPhysicalController,

        /// <summary>
        /// Controller for an OpenXR articulated hand
        /// </summary>
        OpenXRArticulatedHand,
    }
}
