using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.Threading;
using WaveEngine.NoesisGUI;
using WaveEngine.Platform;
using WaveEngine_MRTK_Demo.Scenes;

namespace WaveEngine_MRTK_Demo
{
    public class MyApplication : Application
    {
        public MyApplication()
        {
            this.Container.RegisterType<Clock>();
            this.Container.RegisterType<TimerFactory>();
            this.Container.RegisterType<Random>();
            this.Container.RegisterType<ErrorHandler>();
            this.Container.RegisterType<ScreenContextManager>();
            this.Container.RegisterType<GraphicsPresenter>();
            this.Container.RegisterType<AssetsDirectory>();
            this.Container.RegisterType<AssetsService>();
            this.Container.RegisterType<WorkActionScheduler>();
            this.Container.RegisterType<ForegroundTaskSchedulerService>();
            this.Container.RegisterType<NoesisService>();

            ForegroundTaskScheduler.Foreground.Configure(this.Container);
            BackgroundTaskScheduler.Background.Configure(this.Container);
        }

        public override void Initialize()
        {
            base.Initialize();

            // Get ScreenContextManager
            var screenContextManager = this.Container.Resolve<ScreenContextManager>();
            var assetsService = this.Container.Resolve<AssetsService>();

            // Navigate to scene
            var scene = assetsService.Load<DemoScene>(WaveContent.Scenes.DemoScene_wescene);
            //var scene = assetsService.Load<TestScene>(WaveContent.Scenes.TestScene_wescene);
            ScreenContext screenContext = new ScreenContext(scene);
            screenContextManager.To(screenContext);
        }
    }
}
