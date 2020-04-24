using System;
using System.Collections.Generic;
using System.Text;

namespace WaveEngine_MRTK_Demo.Emulation
{
    public interface IVoiceCommandService
    {
        event EventHandler<string> CommandRecognized;
    }
}
