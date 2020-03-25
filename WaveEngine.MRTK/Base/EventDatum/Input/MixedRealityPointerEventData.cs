// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;
using WaveEngine.Mathematics;

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
        /// Gets or sets the cursor entity.
        /// </summary>
        public Entity Cursor { get; set; }
    }
}
