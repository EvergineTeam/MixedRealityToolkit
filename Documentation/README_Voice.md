# Voice Commands
![Voice](Images/VoiceCommands/MRTK_Input_Speech.png)

The first step to add voice commands to your app is to register the VoiceCommandService on the AppView.cs of your application, at the end of the ***SetWindow*** method and configure the word that you want to be recognized by your app:

```csharp
public void SetWindow(CoreWindow window)
{
    ...

    application.Container.RegisterType<VoiceCommandService>();
    VoiceCommandService voiceCommandService = application.Container.Resolve<VoiceCommandService>();
    voiceCommandService.ConfigureWords(new string[] {
        "select", "button"
    });
}
```

You can then subscribe to the event CommandRecognized that will be fired when a new voice command is identified

```csharp
...
    this.xrPlatform = Application.Current.Container.Resolve<XRPlatform>();
    if (this.xrPlatform != null)
    {
        IVoiceCommandService voiceCommandService = Application.Current.Container.Resolve<IVoiceCommandService>();
        if (voiceCommandService != null)
        {
            voiceCommandService.CommandRecognized += this.VoiceCommandService_CommandRecognized;
        }
    }
...

private void VoiceCommandService_CommandRecognized(object sender, string e)
{
    // your code here
}
```

## Using the Gaze Provider and the ISpeechHandler interface
The **GazeProvider** is automatically added to your **XRScene** and will call the method **OnSpeechKeywordRecognized** if the target implements the interface **ISpeechHandler**

This behavior is implemented on **PressableButton**, using the field ***SpeechKeyWord*** which wil define the word that will make the button to be pressed by saying it
