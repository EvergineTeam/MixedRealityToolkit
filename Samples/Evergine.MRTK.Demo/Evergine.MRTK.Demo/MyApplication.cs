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
            Container.RegisterType<Clock>();
            Container.RegisterType<TimerFactory>();
            Container.RegisterType<Random>();
            Container.RegisterType<ErrorHandler>();
            Container.RegisterType<ScreenContextManager>();
            Container.RegisterType<GraphicsPresenter>();
            Container.RegisterType<AssetsDirectory>();
            Container.RegisterType<AssetsService>();
            Container.RegisterType<ForegroundTaskSchedulerService>();            
            Container.RegisterType<WorkActionScheduler>();

            Container.RegisterType<DemoCommandService>();

            ForegroundTaskScheduler.Foreground.Configure(Container);
            BackgroundTaskScheduler.Background.Configure(Container);
        }

        public override void Initialize()
        {
            base.Initialize();

            var voiceCommandService = Container.Resolve<IVoiceCommandService>();
            if (voiceCommandService == null)
            {
                voiceCommandService = new FakeVoiceCommandService();
                Container.RegisterInstance(voiceCommandService);
            }

            voiceCommandService.ConfigureVoiceCommands(VoiceKeywords.ValidVoiceKeywords);

            // Get ScreenContextManager
            var screenContextManager = Container.Resolve<ScreenContextManager>();
            var assetsService = Container.Resolve<AssetsService>();

            // Navigate to scene
            var scene = assetsService.Load<DemoScene>(EvergineContent.Scenes.DemoScene_wescene);
            var screenContext = new ScreenContext(scene);
            screenContextManager.To(screenContext);
        }
    }
}
