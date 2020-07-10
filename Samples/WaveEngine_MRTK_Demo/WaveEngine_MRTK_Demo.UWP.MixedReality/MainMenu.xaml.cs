using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace WaveEngine_MRTK_Demo.UWP.MixedReality
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainMenu : UserControl
    {
        public event EventHandler StartButtonClick;

        public MainMenu()
        {
            this.InitializeComponent();

            this.versionText.Text = $"v{this.GetAppVersion()}";
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            this.startButton.Click -= this.StartButton_Click;

            this.StartButtonClick?.Invoke(this, EventArgs.Empty);
        }

        private string GetAppVersion()
        {
            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
        }
    }
}
