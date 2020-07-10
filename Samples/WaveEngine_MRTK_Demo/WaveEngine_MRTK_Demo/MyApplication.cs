using System.Threading.Tasks;
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
        private TaskCompletionSource<bool> applicationGotFocusTCS = new TaskCompletionSource<bool>();

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

        public async override void Initialize()
        {
            await this.InitializeAsync(forceCreateContextInWaveThread: true);
        }

        protected async Task InitializeAsync(bool forceCreateContextInWaveThread)
        {
            base.Initialize();

            // Get ScreenContextManager
            var screenContextManager = this.Container.Resolve<ScreenContextManager>();

            ScreenContext screenContext;
            if (forceCreateContextInWaveThread)
            {
                screenContext = await WaveForegroundTask.Run(() => this.CreateScreenContext());
            }
            else
            {
                screenContext = this.CreateScreenContext();
            }

            // Navigate to scene
            screenContextManager.To(screenContext, false);
        }

        private ScreenContext CreateScreenContext()
        {
            var assetsService = this.Container.Resolve<AssetsService>();

            var scene = assetsService.Load<DemoScene>(WaveContent.Scenes.DemoScene_wescene);
            //var scene = assetsService.Load<TestScene>(WaveContent.Scenes.TestScene_wescene);
            scene.Initialize();

            return new ScreenContext(scene);
        }

        public Task WaitFocusAsync()
        {
            return this.applicationGotFocusTCS.Task;
        }

        public void GetFocus()
        {
            this.applicationGotFocusTCS.SetResult(true);
        }
    }
}
