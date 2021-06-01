// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Emulation;

namespace WaveEngine.MRTK.Base.EventDatum.Input
{
    /// <summary>
    /// The pointer event data.
    /// </summary>
    public class MixedRealityPointerEventData
    {
        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the orientation.
        /// </summary>
        public Quaternion Orientation { get; set; }

        /// <summary>
        /// Gets or sets the cursor linear velocity.
        /// </summary>
        public Vector3 LinearVelocity;

        /// <summary>
        /// Gets or sets the cursor angular velocity.
        /// </summary>
        public Quaternion AngularVelocity;

        /// <summary>
        /// Gets or sets the cursor component.
        /// </summary>
        public Cursor Cursor { get; set; }

        /// <summary>
        /// Gets or sets the current target.
        /// </summary>
        public Entity CurrentTarget { get; set; }

        /// <summary>
        /// Gets a value indicating whether this event was already handled.
        /// </summary>
        public bool EventHandled { get; private set; }

        /// <summary>
        /// Mark the event as handled.
        /// </summary>
        public void SetHandled()
        {
            this.EventHandled = true;
        }
    }
}
