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

        public override void Initialize()
        {
            base.Initialize();

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
