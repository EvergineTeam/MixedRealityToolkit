using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.XR;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class HoloLensControlBehavior : Behavior
    {
        [BindService]
        protected XRPlatform xrPlatform;

        [BindComponent]
        protected TrackXRJoint trackXRJoint;

        [BindComponent]
        protected Cursor cursor;

        [RenderProperty(Tooltip = "First joint that will be used for the pinch gesture")]
        public XRHandJointKind PinchJoint1 { get; set; } = XRHandJointKind.IndexTip;

        [RenderProperty(Tooltip = "Second joint that will be used for the pinch gesture")]
        public XRHandJointKind PinchJoint2 { get; set; } = XRHandJointKind.ThumbTip;

        [RenderProperty(Tooltip = "The distance at which the cursor will make the pinch gesture")]
        public float PinchDistance { get; set; } = 0.03f;

        protected override void Update(TimeSpan gameTime)
        {
            if (this.trackXRJoint != null
                && this.trackXRJoint.TrackedDevice != null
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
