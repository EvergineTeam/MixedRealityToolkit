// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.ToggleButtons
{
    /// <summary>
    /// Adds an element to a toggle group. This will take effect only if owner entity has a
    /// ToggleButton instance.
    /// </summary>
    public class ToggleGroup : Component
    {
        /// <summary>
        /// Gets or sets toggle group name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether all toggle elements in a group could be off.
        /// If true, you can pass from on to off for activated toggle element. If false, if you try
        /// to do the same, that item state will not change.
        /// </summary>
        public bool AllowOff { get; set; }
    }
}
