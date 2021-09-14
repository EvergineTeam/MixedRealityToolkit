using System;
using System.Diagnostics;
using WaveEngine.Common.Input;
using WaveEngine.Common.Input.Keyboard;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WaveEngine.MRTK.Emulation;

namespace WaveEngine_MRTK_Demo.VoiceCommands
{
    public class FakeVoiceCommandService : UpdatableService, IVoiceCommandService
    {
        [BindService]
        protected GraphicsPresenter graphicsPresenter;

        private KeyboardDispatcher keyboardDispatcher;

        public event EventHandler<string> CommandRecognized;

        private string[] voiceCommands;

        private int? currentFakeCommand;

        public void ConfigureVoiceCommands(string[] voiceCommands)
        {
            this.voiceCommands = voiceCommands;
        }

        public override void Update(TimeSpan gameTime)
        {
            if (this.keyboardDispatcher == null)
            {
                this.keyboardDispatcher = this.graphicsPresenter.FocusedDisplay?.KeyboardDispatcher;
            }

            if (this.keyboardDispatcher == null)
            {
                return;
            }

            if (this.keyboardDispatcher.IsKeyDown(Keys.Tab))
            {
                for (int i = 0; i < 10; i++)
                {
                    if (this.keyboardDispatcher.ReadKeyState(Keys.D0 + i) == ButtonState.Pressing)
                    {
                        this.currentFakeCommand = (10 * this.currentFakeCommand ?? 0) + i;
                    }
                }
            }
            else if (this.currentFakeCommand.HasValue)
            {
                var index = this.currentFakeCommand.Value;
                this.currentFakeCommand = null;

                if (index < this.voiceCommands.Length)
                {
                    var command = this.voiceCommands[index];
                    Debug.WriteLine($"[{nameof(FakeVoiceCommandService)}] Fake command recognized: [{index}] {command}");
                    this.CommandRecognized?.Invoke(this, command);
                }
                else
                {
                    Debug.WriteLine($"[{nameof(FakeVoiceCommandService)}] Invalid fake command index: {index}");
                }
            }
        }
    }
}
