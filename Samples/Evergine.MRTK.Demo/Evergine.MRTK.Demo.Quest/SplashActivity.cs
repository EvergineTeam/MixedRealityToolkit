using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Transitions;
using Android.Views;
using System.Threading.Tasks;

namespace Evergine.MRTK.Demo.Quest
{
    [Activity(Label = "@string/app_name",
        ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Orientation,
        ScreenOrientation = ScreenOrientation.SensorLandscape,
        MainLauncher = true,
        Theme = "@style/SplashTheme")]
    public class SplashActivity : Activity
    {
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set fullscreen surface
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            this.RequestWindowFeature(WindowFeatures.ContentTransitions);
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            this.Window.ExitTransition = new Fade(FadingMode.Out);

            // Create your application here
            await Task.Delay(100);
            this.StartActivity(new Intent(this, typeof(MainActivity)), ActivityOptions.MakeSceneTransitionAnimation(this).ToBundle());
            this.Finish();
        }
    }
}