// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Components.XR;
using WaveEngine.Framework.XR.Interaction;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    /// <summary>
    /// Track an XR joint
    /// </summary>
    public class TrackXRJoint : TrackXRDevice
    {
        //public SpatialHandJointKind JointKind { get; set; } = SpatialHandJointKind.IndexTip;

        ///// <inheritdoc/>
        //protected override void InternalUpdate()
        //{
        //    base.InternalUpdate();

        //    if (this.TrackedDevice.TryGetArticulatedHandJoint(this.JointKind, out var joint))
        //    {
        //        this.transform.LocalPosition = joint.Pose.Position;
        //        this.transform.LocalOrientation = joint.Pose.Orientation;
        //    }
        //}
    }
}
