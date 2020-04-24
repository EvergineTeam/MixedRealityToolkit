using WaveEngine.MRTK.Scenes;

namespace WaveEngine_MRTK_Demo.Scenes
{
    public class TestScene : XRScene
    {
        public TestScene() : base(WaveContent.Materials.CursorLeft, WaveContent.Materials.HoloHands, WaveContent.Effects.HoloGraphic)
        {
        }
    }
}
