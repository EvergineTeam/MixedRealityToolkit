// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Components.XR;
using Evergine.Framework;
using Evergine.Mathematics;

namespace Evergine.MRTK.InputSystem.Controllers
{
    /// <summary>
    /// Base class for every Controller type.
    /// </summary>
    public abstract class BaseXRController : Component
    {
        /// <summary>
        /// The <see cref="TrackXRController"/> component.
        /// </summary>
        [BindComponent(isExactType: false)]
        protected TrackXRController trackXRController = null;

        /// <summary>
        /// Gets the controller type.
        /// </summary>
        public abstract ControllerType ControllerType { get; }

        /// <summary>
        /// Get the near interaction transform for this controller. This can be used to place near pointers
        /// or visual aids for near interactions such as pressing a button using a physical controller.
        /// </summary>
        /// <param name="transform">The transform of the near interaction source.</param>
        /// <returns>Whether it was possible to find a near interaction transform.</returns>
        public abstract bool TryGetNearInteractionTransform(out Matrix4x4 transform);
    }
}
