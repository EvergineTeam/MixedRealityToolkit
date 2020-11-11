using System;
using WaveEngine_MRTK_Demo.Common;
using WaveEngine_MRTK_Demo.UWP.MixedReality.Services;

namespace WaveEngine_MRTK_Demo.UWP.MixedReality
{
    public class MixedRealityApplication : UWPApplication
    {
        public MixedRealityApplication()
            : base()
        {
            // Add services specific to Mixed Reality
            this.Container.RegisterType<VoiceCommandService>();
            VoiceCommandService voiceCommandService = this.Container.Resolve<VoiceCommandService>();
            voiceCommandService.ConfigureVoiceCommands(new string[] {
                "select", "button"
            });
        }

        public async override void Initialize()
        {
            await base.InitializeAsync(forceCreateContextInWaveThread: false);
            this.initializationTCS.TrySetResult(true);
            this.OnApplicationStarted();
        }
    }
}
