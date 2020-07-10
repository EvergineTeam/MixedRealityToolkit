using System;
using System.Threading.Tasks;
using WaveEngine_MRTK_Demo.Common.ViewModels;
using WaveEngine_MRTK_Demo.Common.Extensions;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WaveEngine_MRTK_Demo.UWP.MixedReality
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CoreApplicationView viewInmmersive;

        private AppViewSource appViewSource;
        private MixedRealityApplication application;

        private MainMenuViewModel mainMenuViewModel;

        public MainPage()
        {
            // Create app
            this.application = new MixedRealityApplication();
            this.application.ApplicationStarted += this.Application_ApplicationStarted;

            // Create view model
            this.mainMenuViewModel = new MainMenuViewModel();

            this.DataContext = this.mainMenuViewModel;

            this.InitializeComponent();
            this.Loaded += this.MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.viewInmmersive != null)
            {
                return;
            }

            this.appViewSource = new AppViewSource(this.application);
            this.viewInmmersive = CoreApplication.CreateNewView(this.appViewSource);

            await this.viewInmmersive.Dispatcher.RunAsync(() =>
            {
                this.application.Initialize();
            });
        }

        private async void CoreApplication_LeavingBackground(object sender, LeavingBackgroundEventArgs e)
        {
            await this.SwitchToInmmersiveMode(false);
        }

        private async Task SwitchToInmmersiveMode(bool resetState)
        {
            await this.viewInmmersive.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                var toViewId = ApplicationView.GetForCurrentView().Id;

                await this.application.WaitInitializationAsync();

                CoreWindow.GetForCurrentThread().Activate();

                await CoreApplication.MainView.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    var fromViewId = ApplicationView.GetForCurrentView().Id;
                    await ApplicationViewSwitcher.SwitchAsync(toViewId, fromViewId);
                });

                if (resetState)
                {
                    this.appViewSource.AppView.ConfigureMixedRealityDisplay();
                }
            });
        }

        private async void Application_ApplicationStarted(object sender, System.EventArgs e)
        {
            this.application.ApplicationStarted -= this.Application_ApplicationStarted;

            await Dispatcher.RunAsync(() => this.mainMenuViewModel.ApplicationLoaded = true);
        }

        private async void MainMenu_StartButtonClick(object sender, System.EventArgs e)
        {
            this.application.GetFocus();

            await this.SwitchToInmmersiveMode(true);

            CoreApplication.LeavingBackground -= this.CoreApplication_LeavingBackground;
            CoreApplication.LeavingBackground += this.CoreApplication_LeavingBackground;
        }

        /*private async void MainMenu_SetPathButtonClick(object sender, System.EventArgs e)
        {
            await this.application.FolderPickerHelper.RequestModelPath();

            this.mainMenuViewModel.ModelPath = this.application.FolderPickerHelper.StorageFolderPath;
        }*/
    }
}
