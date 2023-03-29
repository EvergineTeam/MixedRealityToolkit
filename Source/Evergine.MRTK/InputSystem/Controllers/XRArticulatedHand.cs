// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Components.XR;
using Evergine.Framework.XR;
using Evergine.Mathematics;

namespace Evergine.MRTK.InputSystem.Controllers
{
    /// <summary>
    /// MRTK controller for an articulated hand.
    /// </summary>
    public class XRArticulatedHand : BaseXRController
    {
        /// <inheritdoc/>
        public override ControllerType ControllerType => ControllerType.XRArticulatedHand;

        /// <inheritdoc/>
        public override bool TryGetNearInteractionTransform(out Matrix4x4 transform)
        {
            if (this.trackXRController is TrackXRArticulatedHand trackXRArticulatedHand)
            {
                if (trackXRArticulatedHand.TryGetArticulatedHandJoint(XRHandJointKind.IndexTip, out var joint))
                {
                    transform = Matrix4x4.CreateFromTR(joint.Pose.Position, joint.Pose.Orientation);
                    return true;
                }
            }

            transform = Matrix4x4.Identity;
            return false;
        }
    }
}
