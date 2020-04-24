using WaveEngine.MRTK.Scenes;

namespace WaveEngine_MRTK_Demo.Scenes
{
    public class DemoScene : XRScene
    {
        public DemoScene() : base(WaveContent.Materials.CursorLeft, WaveContent.Materials.HoloHands, WaveContent.Effects.HoloGraphic)
        {
        }
    }
}