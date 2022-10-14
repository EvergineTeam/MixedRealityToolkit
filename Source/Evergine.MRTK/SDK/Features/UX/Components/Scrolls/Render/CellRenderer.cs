// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Mathematics;

namespace Evergine.MRTK.SDK.Features.UX.Components.Scrolls
{
    /// <summary>
    /// Render of a table cell.
    /// </summary>
    public abstract class CellRenderer
    {
        /// <summary>
        /// Render a cell.
        /// </summary>
        /// <param name="value">string value.</param>
        /// <param name="position">Cell position.</param>
        /// <param name="width">Cell width.</param>
        /// <param name="height">Cell height.</param>
        /// <param name="layer">Cell layer.</param>
        /// <param name="color">Cell color.</param>
        /// <returns>Return a entity.</returns>
        public abstract Entity Render(string value, Vector3 position, float width, float height, RenderLayerDescription layer, Color color);
    }
}
