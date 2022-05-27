// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Common.Attributes;
using Evergine.Framework;
using Evergine.Framework.XR;
using Evergine.Mathematics;
using Evergine.MRTK.Emulation;
using Evergine.MRTK.SDK.Features;

namespace Evergine.MRTK.Behaviors
{
    /// <summary>
    /// HoloLens pinch.
    /// </summary>
    public class HandTrackingControlBehavior : Behavior
    {
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
        /// Gets or sets the distance between the index finger tip and the thumb tip required to enter the pinch/air tap gesture.
        /// The pinch gesture enter will be registered for all values less than the <see cref="EnterPinchDistance"/>.
        /// Default: <c>0.02f</c>.
        /// </summary>
        [RenderProperty(Tooltip = "The distance at which the cursor will enter the pinch gesture")]
        public float EnterPinchDistance { get; set; } = 0.02f;

        /// <summary>
        /// Gets or sets the distance between the index finger tip and the thumb tip required to exit the pinch/air tap gesture.
        /// The pinch gesture exit will be registered for all values greater than the <see cref="ExitPinchDistance"/>.
        /// Default: <c>0.05f</c>.
        /// </summary>
        [RenderProperty(Tooltip = "The distance at which the cursor will exit the pinch gesture")]
        public float ExitPinchDistance { get; set; } = 0.05f;

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            if (Tools.IsJointValid(this.trackXRJoint)
                && this.trackXRJoint.TrackedDevice.TryGetArticulatedHandJoint(XRHandJointKind.IndexTip, out var indexTip)
                && this.trackXRJoint.TrackedDevice.TryGetArticulatedHandJoint(XRHandJointKind.ThumbTip, out var thumbTip))
            {
                float distance = Vector3.Distance(indexTip.Pose.Position, thumbTip.Pose.Position);

                var isPinching = this.cursor.Pinch;
                if (isPinching && distance > this.ExitPinchDistance)
                {
                    this.cursor.Pinch = false;
                }
                else if (!isPinching && distance < this.EnterPinchDistance)
                {
                    this.cursor.Pinch = true;
                }
            }
            else
            {
                this.cursor.Pinch = false;
            }
        }
    }
}
