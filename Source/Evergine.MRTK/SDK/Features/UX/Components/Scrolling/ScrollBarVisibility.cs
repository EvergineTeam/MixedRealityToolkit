// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

namespace Evergine.MRTK.SDK.Features.UX.Components.Scrolling
{
    /// <summary>
    /// Scroll bar visibility options.
    /// </summary>
    public enum ScrollBarVisibility
    {
        /// <summary>
        /// Automatic. Bar will be visible or hidden depending on content size. If content size is lower
        /// than available space, then bar will not be displayed.
        /// </summary>
        Auto,

        /// <summary>
        /// Visible. Bar will be always visible, even if the content size is lower than available space.
        /// </summary>
        Visible,

        /// <summary>
        /// Hidden. Bar will never be visible.
        /// </summary>
        Hidden,
    }
}
