using System;
using System.Threading.Tasks;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.Threading;
using WaveEngine.Platform;
using WaveEngine.XAudio2;
using WaveEngine_MRTK_Demo.Scenes;

namespace WaveEngine_MRTK_Demo.Common
{
    public class UWPApplication : MyApplication
    {
        protected TaskCompletionSource<bool> initializationTCS;

        public event EventHandler ApplicationStarted;

        private bool isInitializationFinished;

        public UWPApplication()
            : base()
        {
            this.initializationTCS = new TaskCompletionSource<bool>();

            this.Container.RegisterInstance(new XAudioDevice());

            // Register platform specific service implementations
        }

        public async override void Initialize()
        {
            await this.InitializeAsync(forceCreateContextInWaveThread: true);

            this.isInitializationFinished = true;
            this.OnApplicationStarted();
        }

        protected void OnApplicationStarted()
        {
            this.ApplicationStarted?.Invoke(this, EventArgs.Empty);
        }

        public override void DrawFrame(TimeSpan gameTime)
        {
            base.DrawFrame(gameTime);

            if (this.isInitializationFinished &&
                !this.initializationTCS.Task.IsCompleted)
            {
                this.initializationTCS.TrySetResult(true);
            }
        }

        public Task WaitInitializationAsync()
        {
            return this.initializationTCS.Task;
        }
    }
}
