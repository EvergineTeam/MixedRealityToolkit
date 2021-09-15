using System;
using WaveEngine.MRTK.Emulation;
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
            this.Container.RegisterType<IVoiceCommandService, VoiceCommandService>();
        }

        public async override void Initialize()
        {
            await base.InitializeAsync(forceCreateContextInWaveThread: false);
            this.initializationTCS.TrySetResult(true);
            this.OnApplicationStarted();
        }
    }
}
