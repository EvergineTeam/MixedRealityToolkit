using WaveEngine.Bullet;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Services;
using WaveEngine.MRTK.SDK.Features;
using WaveEngine.MRTK.Services.InputSystem;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine_MRTK_Demo.Scenes
{
    public class DemoScene : Scene
    {
        public override void RegisterManagers()
        {
            base.RegisterManagers();

            this.Managers.AddManager(new BulletPhysicManager3D());
            this.Managers.AddManager(new CursorManager());
        }

        protected override void CreateScene()
        {
            //this.Managers.RenderManager.DebugLines = true;

            var assetsService = Application.Current.Container.Resolve<AssetsService>();

            Tools.InitHoloScene(this, assetsService.Load<Material>(WaveContent.Materials.CursorLeft), assetsService.Load<Material>(WaveContent.Materials.HoloHands), WaveContent.Effects.HoloGraphic);
        }

        protected override void Start()
        {
            base.Start();

            //Create GazeProvider
            Camera3D cam = this.Managers.EntityManager.FindFirstComponentOfType<Camera3D>();
            cam.Owner.AddComponent(new GazeProvider());
        }
    }
}