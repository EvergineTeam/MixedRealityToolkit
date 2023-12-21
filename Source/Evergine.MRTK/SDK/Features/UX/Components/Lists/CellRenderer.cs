// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;

namespace Evergine.MRTK.SDK.Features.UX.Components.Lists
{
    /// <summary>
    /// Render of a table cell.
    /// </summary>
    public abstract class CellRenderer
    {
        /// <summary>
        /// Gets cell rendering area width.
        /// </summary>
        public float Width { get; private set; }

        /// <summary>
        /// Gets cell rendering area height.
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// Gets proposed cell rendering layer.
        /// </summary>
        public RenderLayerDescription Layer { get; private set; }

        /// <summary>
        /// Render a cell.
        /// </summary>
        /// <param name="parent">Cell parent container.</param>
        public abstract void Render(Entity parent);

        internal Entity InternalRender(Vector3 position, float width, float height, RenderLayerDescription layer)
        {
            var entity = new Entity()
                .AddComponent(new Transform3D()
                {
                    LocalPosition = position,
                });

            this.Width = width;
            this.Height = height;
            this.Layer = layer;
            this.Render(entity);

            return entity;
        }
    }
}
