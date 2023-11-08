// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Graphics;
using Evergine.Components.Fonts;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;

namespace Evergine.MRTK.SDK.Features.UX.Components.Lists
{
    /// <summary>
    /// Render a cell as a text.
    /// </summary>
    public class TextCellRenderer : CellRenderer
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly TextCellRenderer Instance = new TextCellRenderer();

        /// <summary>
        /// Gets or sets a value indicating whether the debug is enable or not.
        /// </summary>
        public bool Debug { get; set; } = false;

        /// <summary>
        /// Gets or sets the text's font.
        /// </summary>
        public Font Font { get; set; } = null;

        /// <summary>
        /// Gets or sets the font's scale factor.
        /// </summary>
        public float ScaleFactor { get; set; } = 0.006f;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextCellRenderer"/> class.
        /// </summary>
        private TextCellRenderer()
        {
        }

        /// <inheritdoc/>
        public override Entity Render(string value, Vector3 position, float width, float height, RenderLayerDescription layer, Color color)
        {
            Entity entity = new Entity()
                .AddComponent(new Transform3D()
                {
                    LocalPosition = position,
                })
                .AddComponent(new Text3DMesh()
                {
                    Text = value,
                    Color = color,
                    Layer = layer,
                    Size = new Vector2(width, height),
                    ScaleFactor = this.ScaleFactor,
                    VerticalAlignment = VerticalAlignment.Center,
                    Font = this.Font,
                })
                .AddComponent(new Text3DRenderer()
                {
                    DebugMode = this.Debug,
                });

            return entity;
        }
    }
}
