using System;
using WaveEngine.MRTK.Scenes;

namespace WaveEngine_MRTK_Demo.Scenes
{
    public class TestScene : XRScene
    {
        protected override Guid CursorMatPressed => WaveContent.MRTK.Materials.CursorLeftPinch;

        protected override Guid CursorMatReleased => WaveContent.MRTK.Materials.CursorLeft;

        protected override Guid HoloHandsMat => WaveContent.MRTK.Materials.HoloHands;

        protected override Guid SpatialMappingMat => Guid.Empty;

        protected override Guid HandRayTexture => WaveContent.MRTK.Textures.line_dots_png;

        protected override Guid HandRaySampler => WaveContent.MRTK.Samplers.LinearWrapSampler;
    }
}
