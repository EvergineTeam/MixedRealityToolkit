// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Emulation;

namespace WaveEngine.MRTK.Base.EventDatum.Input
{
    /// <summary>
    /// Hand tracking input event data.
    /// </summary>
    public class HandTrackingInputEventData
    {
        /// <summary>
        /// Gets or sets the position.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the previous position.
        /// </summary>
        public Vector3 PreviousPosition { get; set; }

        /// <summary>
        /// Gets or sets the cursor component.
        /// </summary>
        public CursorTouch Cursor { get; set; }
    }
}
