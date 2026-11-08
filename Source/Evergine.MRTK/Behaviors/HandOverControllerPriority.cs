// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Framework;
using Evergine.Framework.Services;
using Evergine.Framework.XR;
using Evergine.Framework.XR.TrackedDevices;

namespace Evergine.MRTK.Behaviors
{
    /// <summary>
    /// Hides the physical controller of a side, with its cursors, while the articulated hand of that side is tracked.
    /// </summary>
    /// <remarks>
    /// Some runtimes report a tracked hand as a controller too: Android XR binds the hands to
    /// /interaction_profiles/khr/simple_controller. Without this, the controller model and its cursors
    /// would follow the hand on top of the hand ones.
    /// </remarks>
    public class HandOverControllerPriority : Behavior
    {
        [BindService(isRequired: false)]
        private XRPlatform xrPlatform = null;

        /// <summary>
        /// Gets or sets the root entity of the left physical controller and its cursors.
        /// </summary>
        public Entity LeftControllerRoot { get; set; }

        /// <summary>
        /// Gets or sets the root entity of the right physical controller and its cursors.
        /// </summary>
        public Entity RightControllerRoot { get; set; }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            var inputTracking = this.xrPlatform?.InputTracking;
            if (inputTracking == null)
            {
                return;
            }

            Refresh(inputTracking, XRHandedness.LeftHand, this.LeftControllerRoot);
            Refresh(inputTracking, XRHandedness.RightHand, this.RightControllerRoot);
        }

        private static void Refresh(XRInputTracking inputTracking, XRHandedness handedness, Entity controllerRoot)
        {
            if (controllerRoot == null)
            {
                return;
            }

            var hand = inputTracking.GetDeviceByTypeAndHandedness(XRTrackedDeviceType.Hand, handedness);
            var controllerAllowed = hand == null || !hand.PoseIsValid;

            if (controllerRoot.IsEnabled != controllerAllowed)
            {
                controllerRoot.IsEnabled = controllerAllowed;
            }
        }
    }
}
