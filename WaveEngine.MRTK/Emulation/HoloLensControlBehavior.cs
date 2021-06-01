// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.XR;
using WaveEngine.MRTK.Emulation;
using WaveEngine.MRTK.SDK.Features;

namespace WaveEngine.MRTK.Behaviors
{
    /// <summary>
    /// Hololens pinch.
    /// </summary>
    public class HoloLensControlBehavior : Behavior
    {
        /// <summary>
        /// The xrPlatform.
        /// </summary>
        [BindService]
        protected XRPlatform xrPlatform;

        /// <summary>
        /// The joint.
        /// </summary>
        [BindComponent]
        protected TrackXRJoint trackXRJoint;

        /// <summary>
        /// The cursor.
        /// </summary>
        [BindComponent(isExactType: false, source: BindComponentSource.Children)]
        protected Cursor cursor;

        /// <summary>
        /// Gets or sets the first joint.
        /// </summary>
        [RenderProperty(Tooltip = "First joint that will be used for the pinch gesture")]
        public XRHandJointKind PinchJoint1 { get; set; } = XRHandJointKind.IndexTip;

        /// <summary>
        /// Gets or sets the second joint.
        /// </summary>
        [RenderProperty(Tooltip = "Second joint that will be used for the pinch gesture")]
        public XRHandJointKind PinchJoint2 { get; set; } = XRHandJointKind.ThumbTip;

        /// <summary>
        /// Gets or sets the pinch distance.
        /// </summary>
        [RenderProperty(Tooltip = "The distance at which the cursor will make the pinch gesture")]
        public float PinchDistance { get; set; } = 0.03f;

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            if (Tools.IsJointValid(this.trackXRJoint)
                && this.trackXRJoint.TrackedDevice.TryGetArticulatedHandJoint(this.PinchJoint1, out var joint1)
                && this.trackXRJoint.TrackedDevice.TryGetArticulatedHandJoint(this.PinchJoint2, out var joint2))
            {
                var distance = joint1.Pose.Position - joint2.Pose.Position;
                this.cursor.Pinch = distance.Length() < this.PinchDistance;
            }
            else
            {
                this.cursor.Pinch = false;
            }
        }
    }
}
