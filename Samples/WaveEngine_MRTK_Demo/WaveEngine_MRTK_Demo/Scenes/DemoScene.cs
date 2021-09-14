using System;
using WaveEngine.MRTK.Scenes;

namespace WaveEngine_MRTK_Demo.Scenes
{
    public class DemoScene : XRScene
    {
        protected override Guid CursorMatPressed => WaveContent.MRTK.Materials.Cursor.CursorPinch;

        protected override Guid CursorMatReleased => WaveContent.MRTK.Materials.Cursor.CursorBase;

        protected override Guid HoloHandsMat => WaveContent.MRTK.Materials.HoloHands;

        protected override Guid SpatialMappingMat => Guid.Empty;

        protected override Guid HandRayTexture => WaveContent.MRTK.Textures.line_dots_png;

        protected override Guid HandRaySampler => WaveContent.MRTK.Samplers.LinearWrapSampler;
    }
}