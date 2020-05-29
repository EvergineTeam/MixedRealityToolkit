using System.Diagnostics;
using System.Threading.Tasks;
using WaveEngine.Common.Graphics;
using WaveEngine.DirectX11;
using WaveEngine.Framework.Services;
using WaveEngine.MixedReality;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace WaveEngine_MRTK_Demo.UWP.MixedReality
{
    internal class AppView : IFrameworkView
    {
        private MixedRealityPlatform xrDevice;
        private MyApplication application;
        private MixedRealityWindowsSystem windowsSystem;

        private bool isApplicationInitilized;

        public void Initialize(CoreApplicationView applicationView)
        {
            applicationView.Activated += this.OnViewActivated;

            // Register event handlers for app lifecycle.
            CoreApplication.Suspending += this.OnSuspending;
            CoreApplication.Resuming += this.OnResuming;
        }

        public void SetWindow(CoreWindow window)
        {
            // At this point we have access to the device and we can create device-dependent
            // resources.
            // Create app
            application = new MyApplication();
            
            // Create Services
            xrDevice = new MixedRealityPlatform();
            application.Container.RegisterInstance(xrDevice);
            windowsSystem = new MixedRealityWindowsSystem(window);
            application.Container.RegisterInstance(windowsSystem);

            ConfigureGraphicsContext(application);
			
			// Creates XAudio device
            var xaudio = new WaveEngine.XAudio2.XAudioDevice();
            application.Container.RegisterInstance(xaudio);

            application.Container.RegisterType<VoiceCommandService>();
            VoiceCommandService voiceCommandService = application.Container.Resolve<VoiceCommandService>();
            voiceCommandService.ConfigureWords(new string[] {
                "select", "button"
            });
        }

        /// <summary>
        /// The Load method can be used to initialize scene resources or to load a
        /// previously saved app state.
        /// </summary>
        public void Load(string entryPoint)
        {
            if (!isApplicationInitilized)
            {
                isApplicationInitilized = true;
                application.Initialize();
                ConfigureMixedRealityDisplay(application, xrDevice);
            }
        }

        /// <summary>
        /// This method is called after the window becomes active. It oversees the
        /// update, draw, and present loop, and also oversees window message processing.
        /// </summary>
        public void Run()
        {
            Stopwatch clockTimer = Stopwatch.StartNew();
            windowsSystem.Run(null,
            () =>
            {
                var gameTime = clockTimer.Elapsed;
                clockTimer.Restart();

                xrDevice.Update();
                application.UpdateFrame(gameTime);
                application.DrawFrame(gameTime);
            });
        }

        /// <summary>
        /// Terminate events do not cause Uninitialize to be called. It will be called if your IFrameworkView
        /// class is torn down while the app is in the foreground.
        // This method is not often used, but IFrameworkView requires it and it will be called for
        // holographic apps.
        /// </summary>
        public void Uninitialize()
        {
        }

        /// <summary>
        /// Called when the app is prelaunched.Use this method to load resources ahead of time
        /// and enable faster launch times.
        /// </summary>
        public void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PrelaunchActivated)
            {
                //
                // TODO: Insert code to preload resources here.
                //
            }
        }

        /// <summary>
        /// Called when the app view is activated. Activates the app's CoreWindow.
        /// </summary>
        private void OnViewActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            // Run() won't start until the CoreWindow is activated.
            sender.CoreWindow.Activate();
        }

        private void OnSuspending(object sender, SuspendingEventArgs args)
        {
            // Save app state asynchronously after requesting a deferral. Holding a deferral
            // indicates that the application is busy performing suspending operations. Be
            // aware that a deferral may not be held indefinitely; after about five seconds,
            // the app will be forced to exit.
            var deferral = args.SuspendingOperation.GetDeferral();

            Task.Run(() =>
                {
                    //
                    // TODO: Insert code here to save your app state.
                    //
                    deferral.Complete();
                });
        }

        private void OnResuming(object sender, object args)
        {
            //
            // TODO: Insert code here to load your app state.
            //
        }

        private static void ConfigureGraphicsContext(MyApplication application)
        {
            GraphicsContext graphicsContext = new DX11GraphicsContext();
            graphicsContext.CreateDevice();
            application.Container.RegisterInstance(graphicsContext);
        }

        private static void ConfigureMixedRealityDisplay(MyApplication application, MixedRealityPlatform xrDevice)
        {
            var graphicsPresenter = application.Container.Resolve<GraphicsPresenter>();
            graphicsPresenter.AddDisplay("DefaultDisplay", xrDevice.Display);
        }
    }
}
