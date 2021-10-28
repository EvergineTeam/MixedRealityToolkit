// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Framework;
using Evergine.MRTK.Emulation;

namespace Evergine.MRTK.Base.EventDatum.Input
{
    /// <summary>
    /// Focus event data.
    /// </summary>
    public class MixedRealityFocusEventData
    {
        /// <summary>
        /// Gets or sets the cursor component.
        /// </summary>
        public Cursor Cursor { get; set; }

        /// <summary>
        /// Gets or sets the current target.
        /// </summary>
        public Entity CurrentTarget { get; set; }
    }
}
