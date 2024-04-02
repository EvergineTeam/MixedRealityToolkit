// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Graphics;
using Evergine.Components.Fonts;
using Evergine.Framework;
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
        /// Gets or sets text to be rendered.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets text color.
        /// </summary>
        public Color Color { get; set; } = Color.White;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextCellRenderer"/> class.
        /// </summary>
        private TextCellRenderer()
        {
        }

        /// <inheritdoc/>
        public override void Render(Entity parent)
        {
            parent
                .AddComponent(new Text3DMesh()
                {
                    Text = this.Text,
                    Color = this.Color,
                    Layer = this.Layer,
                    Size = new Vector2(this.Width, this.Height),
                    ScaleFactor = 0.006f,
                    VerticalAlignment = VerticalAlignment.Center,
                })
                .AddComponent(new Text3DRenderer()
                {
                    DebugMode = this.Debug,
                });
        }
    }
}
