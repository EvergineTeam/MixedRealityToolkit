using Evergine.Framework;
using Evergine.Framework.Services;
using Evergine.Framework.Threading;
using Evergine.MRTK.Demo.Components.Commands;
using Evergine.MRTK.Demo.Scenes;
using Evergine.MRTK.Demo.VoiceCommands;
using Evergine.MRTK.Emulation;
using Evergine.NoesisGUI;
using Evergine.Platform;
using System.Threading.Tasks;

namespace Evergine.MRTK.Demo
{
    public partial class MyApplication : Application
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
            this.Container.RegisterType<ForegroundTaskSchedulerService>();            
            this.Container.RegisterType<WorkActionScheduler>();
            this.Container.RegisterType<NoesisService>();

            this.Container.RegisterType<DemoCommandService>();

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

            var voiceCommandService = this.Container.Resolve<IVoiceCommandService>();
            if (voiceCommandService == null)
            {
                this.Container.RegisterType<FakeVoiceCommandService>();
                voiceCommandService = this.Container.Resolve<FakeVoiceCommandService>();
            }

            voiceCommandService.ConfigureVoiceCommands(VoiceKeywords.ValidVoiceKeywords);

            // Get ScreenContextManager
            var screenContextManager = this.Container.Resolve<ScreenContextManager>();

            ScreenContext screenContext;
            if (forceCreateContextInWaveThread)
            {
                screenContext = await EvergineForegroundTask.Run(() => this.CreateScreenContext());
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

            var scene = assetsService.Load<DemoScene>(EvergineContent.Scenes.DemoScene_wescene);
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
