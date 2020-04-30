// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Components.XR;
using WaveEngine.Framework.XR;
using WaveEngine.Framework.XR.Interaction;

namespace WaveEngine.MRTK.Behaviors
{
    /// <summary>
    /// Track an XR joint.
    /// </summary>
    public class TrackXRJoint : TrackXRDevice
    {
        /// <summary>
        /// Gets or sets the joint.
        /// </summary>
        public XRHandJointKind JointKind { get; set; } = XRHandJointKind.IndexTip;

        /// <inheritdoc/>
        protected override void InternalUpdate()
        {
            base.InternalUpdate();

            if (this.TrackedDevice.TryGetArticulatedHandJoint(this.JointKind, out var joint))
            {
                this.transform.LocalPosition = joint.Pose.Position;
                this.transform.LocalOrientation = joint.Pose.Orientation;
            }
        }
    }
}
