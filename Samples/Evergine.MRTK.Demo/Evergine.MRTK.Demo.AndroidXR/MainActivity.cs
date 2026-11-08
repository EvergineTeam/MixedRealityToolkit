using Android.Content.PM;
using Android.Views;
using Android.Widget;
using Evergine.Android;
using Evergine.Common.Graphics;
using Evergine.Framework.Services;
using Evergine.OpenXR;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Activity = Android.App.Activity;
using Display = Evergine.Framework.Graphics.Display;
using Surface = Evergine.Common.Graphics.Surface;

namespace Evergine.MRTK.Demo.AndroidXR
{
    [Activity(Label = "@string/app_name",
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.ScreenLayout | ConfigChanges.UiMode | ConfigChanges.Navigation | ConfigChanges.Keyboard,
        ScreenOrientation = ScreenOrientation.Landscape,
        LaunchMode = LaunchMode.SingleTask,
        MainLauncher = true,
        Exported = true,
        Theme = "@style/SplashTheme")]
    public class MainActivity : Activity
    {
        private const string HandTrackingPermission = "android.permission.HAND_TRACKING";
        private const int HandTrackingPermissionRequestCode = 1001;
        private OpenXRPlatform? openXRPlatform;
        private MyApplication? application;
        private AndroidWindowsSystem? windowsSystem;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.RequestHandTrackingPermissionAndStart();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode != HandTrackingPermissionRequestCode)
            {
                return;
            }

            this.StartApplication();
        }

        private void RequestHandTrackingPermissionAndStart()
        {
            if (this.CheckSelfPermission(HandTrackingPermission) == Permission.Granted)
            {
                this.StartApplication();
                return;
            }

            this.RequestPermissions(new[] { HandTrackingPermission }, HandTrackingPermissionRequestCode);
        }

        private void StartApplication()
        {
            if (this.application is not null)
            {
                return;
            }

            // Set fullscreen surface
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            this.Window!.AddFlags(WindowManagerFlags.Fullscreen);

            // Set Main layout
            this.SetContentView(Resource.Layout.Main);

            // Create app
            this.application = new MyApplication();

            // Create Services
            this.windowsSystem = new global::Evergine.Android.AndroidWindowsSystem(this);
            this.application.Container.RegisterInstance(windowsSystem);
            var surface = this.windowsSystem.CreateSurface(0, 0) as global::Evergine.Android.AndroidSurface;

            var view = this.FindViewById<RelativeLayout>(Resource.Id.evergineContainer);
            view!.AddView(surface!.NativeSurface);

            // Creates audio device
            var audioDevice = new global::Evergine.OpenAL.ALAudioDevice();
            this.application.Container.RegisterInstance(audioDevice);

            Stopwatch clockTimer = Stopwatch.StartNew();
            this.windowsSystem.Run(
            () =>
            {
                ConfigureGraphicsContext(this.application, surface);
                this.application.Initialize();
            },
            () =>
            {
                var gameTime = clockTimer.Elapsed;
                clockTimer.Restart();

                this.openXRPlatform?.Update();
                this.application.UpdateFrame(gameTime);
                this.application.DrawFrame(gameTime);
            });
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            this.windowsSystem?.Dispose();
            this.application?.Dispose();
            this.windowsSystem = null;
            this.application = null;
            this.openXRPlatform = null;
        }

        private void ConfigureGraphicsContext(MyApplication application, Surface surface)
        {
            // Create mirror display...
            FrameBuffer frameBuffer = null!;
            var mirrorDisplay = new Display(surface, frameBuffer);

            var requestedOpenXRExtensions = new List<string>();
            if (this.CheckSelfPermission(HandTrackingPermission) == Permission.Granted)
            {
                requestedOpenXRExtensions.Add("XR_EXT_hand_tracking"); // Enable hand tracking in OpenXR application
                requestedOpenXRExtensions.Add("XR_FB_hand_tracking_aim"); // System aim ray and pinch strength for each hand
                requestedOpenXRExtensions.Add("XR_FB_hand_tracking_mesh"); // Skinned hand mesh driven by the hand joints
            }

            this.openXRPlatform = new OpenXRPlatform(
                requestedOpenXRExtensions.ToArray(),
                new OpenXRInteractionProfile[]
                {
                    DefaultInteractionProfiles.KhronosSimpleProfile,
                })
            {
                ApplicationName = "Evergine.MRTK.Demo",
                RenderMirrorTexture = false,
                ReferenceSpace = ReferenceSpaceType.Stage,
                MirrorDisplay = mirrorDisplay,
                // For AR, uncomment to prefer the first supported mode below; otherwise the runtime preference is used.
                // Opaque mode may dim optical see-through lenses (e.g. XREAL Aura).
                PreferredEnvironmentBlendModes = new[] { EnvironmentBlendMode.AlphaBlend, EnvironmentBlendMode.Additive },
            };

            // OpenXR is initialized first: the runtime takes part in the creation of the Vulkan instance and device
            var openXRVulkan = this.openXRPlatform.InitializeForVulkan();

            // Enable Vulkan extensions required by Android XR and OpenXR
            var graphicsContext = new global::Evergine.Vulkan.VKGraphicsContext(new global::Evergine.Vulkan.VKGraphicsContextOptions()
            {
                DeviceExtensions = new[]
                {
                    "VK_KHR_multiview",
                    "VK_ANDROID_external_memory_android_hardware_buffer",
                    "VK_KHR_bind_memory2",
                    "VK_KHR_dedicated_allocation",
                    "VK_KHR_external_memory",
                    "VK_KHR_external_memory_fd",
                    "VK_KHR_get_memory_requirements2",
                    "VK_KHR_sampler_ycbcr_conversion",
                }.Union(openXRVulkan.DeviceExtensions),
                InstanceExtensions = new[]
                {
                    "VK_KHR_get_physical_device_properties2",
                    "VK_KHR_external_memory_capabilities",
                }.Union(openXRVulkan.InstanceExtensions),
                CreateInstance = openXRVulkan.CreateInstance,
                SelectPhysicalDevice = openXRVulkan.SelectPhysicalDevice,
                CreateDevice = openXRVulkan.CreateDevice,
            });

            graphicsContext.CreateDevice();
            application.Container.RegisterInstance<GraphicsContext>(graphicsContext);
            application.Container.RegisterInstance(this.openXRPlatform);

            // Register the displays...
            var graphicsPresenter = application.Container.Resolve<GraphicsPresenter>();
            graphicsPresenter.AddDisplay("DefaultDisplay", this.openXRPlatform.Display);
            graphicsPresenter.AddDisplay("MirrorDisplay", mirrorDisplay);
        }
    }
}
