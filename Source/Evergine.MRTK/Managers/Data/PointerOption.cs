// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Framework.Prefabs;
using Evergine.MRTK.InputSystem.Controllers;

namespace Evergine.MRTK.Managers.Data
{
    /// <summary>
    /// An association between a controller and a pointer that will be spawned when the controller is used.
    /// </summary>
    public class PointerOption
    {
        /// <summary>
        /// Gets or sets the controller type that this pointer option requires.
        /// </summary>
        public ControllerType ControllerType { get; set; }

        /// <summary>
        /// Gets or sets the controller handedness that this pointer option requires.
        /// </summary>
        public ControllerHandedness Handedness { get; set; }

        /// <summary>
        /// Gets or sets the pointer that will be spawned when the specified controller is used.
        /// </summary>
        public Prefab Pointer { get; set; }
    }
}
