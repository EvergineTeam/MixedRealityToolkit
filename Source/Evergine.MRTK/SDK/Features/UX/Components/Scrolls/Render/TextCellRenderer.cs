using Evergine.Common.Graphics;
using Evergine.Components.Fonts;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;

namespace Evergine.MRTK.SDK.Features.UX.Components.Scrolls
{
    public class TextCellRenderer : CellRenderer
    {
        public static readonly TextCellRenderer Instance = new TextCellRenderer();

        public bool Debug { get; set; } = false;

        private TextCellRenderer()
        { }

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
                            ScaleFactor = 0.01f,
                            VerticalAlignment = VerticalAlignment.Center,
                        })
                        .AddComponent(new Text3DRenderer()
                        {
                            DebugMode = this.Debug,
                        });

            return entity;
        }
    }
}
