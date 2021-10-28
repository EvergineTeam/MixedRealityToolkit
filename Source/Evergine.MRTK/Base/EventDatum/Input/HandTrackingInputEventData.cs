// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Framework;
using Evergine.Mathematics;
using Evergine.MRTK.Emulation;

namespace Evergine.MRTK.Base.EventDatum.Input
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

        /// <summary>
        /// Gets or sets the current target.
        /// </summary>
        public Entity CurrentTarget { get; set; }
    }
}
