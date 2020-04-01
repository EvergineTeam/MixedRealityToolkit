using WaveEngine.Bullet;
using WaveEngine.Framework;
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

            Tools.CreateHands(this);
        }
    }
}