using Evergine.Framework;
using Evergine.Framework.Services;
using Evergine.Framework.Threading;
using Evergine.MRTK.Demo.Components.Commands;
using Evergine.MRTK.Demo.Scenes;
using Evergine.MRTK.Demo.VoiceCommands;
using Evergine.MRTK.Emulation;
using Evergine.Platform;

namespace Evergine.MRTK.Demo
{
    public partial class MyApplication : Application
    {
        public MyApplication()
        {
            this.Container.Register<Clock>();
            this.Container.Register<TimerFactory>();
            this.Container.Register<Random>();
            this.Container.Register<ErrorHandler>();
            this.Container.Register<ScreenContextManager>();
            this.Container.Register<GraphicsPresenter>();
            this.Container.Register<AssetsDirectory>();
            this.Container.Register<AssetsService>();
            this.Container.Register<ForegroundTaskSchedulerService>();            
            this.Container.Register<WorkActionScheduler>();

            ForegroundTaskScheduler.Foreground.Configure(this.Container);
            BackgroundTaskScheduler.Background.Configure(this.Container);
        }

        public bool HasPassthroughSupport { get; set; } = false;

        public override void Initialize()
        {
            base.Initialize();

            this.Container.Register<DemoCommandService>();

            var voiceCommandService = this.Container.Resolve<IVoiceCommandService>();
            if (voiceCommandService == null)
            {
                voiceCommandService = new FakeVoiceCommandService();
                this.Container.RegisterInstance(voiceCommandService);
            }

            voiceCommandService.ConfigureVoiceCommands(VoiceKeywords.ValidVoiceKeywords);

            // Get ScreenContextManager
            var screenContextManager = this.Container.Resolve<ScreenContextManager>();
            var assetsService = this.Container.Resolve<AssetsService>();

            // Navigate to scene
            var scene = assetsService.Load<DemoScene>(EvergineContent.Scenes.DemoScene_wescene);
            var screenContext = new ScreenContext(scene);
            screenContextManager.To(screenContext);
        }
    }
}
