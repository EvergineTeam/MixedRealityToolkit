using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Mathematics;

namespace Evergine.MRTK.SDK.Features.UX.Components.Scrolls
{
    public abstract class CellRenderer
    {
        public abstract Entity Render(string value, Vector3 position, float width, float height, RenderLayerDescription layer, Color color);
    }
}
