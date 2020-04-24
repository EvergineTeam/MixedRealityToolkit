using System;
using WaveEngine.MRTK.Scenes;

namespace WaveEngine_MRTK_Demo.Scenes
{
    public class TestScene : XRScene
    {
        protected override Guid CursorMat => WaveContent.Materials.CursorLeft;

        protected override Guid HoloHandsMat => WaveContent.Materials.HoloHands;

        protected override Guid HolographicEffect => WaveContent.Effects.HoloGraphic;
    }
}
