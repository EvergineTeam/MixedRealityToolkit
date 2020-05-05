using System;
using WaveEngine.MRTK.Scenes;

namespace WaveEngine_MRTK_Demo.Scenes
{
    public class DemoScene : XRScene
    {
        protected override Guid CursorMat => WaveContent.Materials.CursorLeft;

        protected override Guid HoloHandsMat => WaveContent.Materials.HoloHands;

        protected override Guid HolographicEffect => WaveContent.Effects.HoloGraphic;

        protected override Guid SpatialMappingMat => Guid.Empty;

        protected override Guid HandRayTexture => WaveContent.Textures.line_dots_png;
    }
}