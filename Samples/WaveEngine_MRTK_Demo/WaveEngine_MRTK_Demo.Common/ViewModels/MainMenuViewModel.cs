using WaveEngine_MRTK_Demo.Common.ViewModels.Base;

namespace WaveEngine_MRTK_Demo.Common.ViewModels
{
    public class MainMenuViewModel : BaseViewModel
    {
        private bool applicationLoaded;
        private bool applicationCanStart;

        public bool ApplicationLoaded
        {
            get => this.applicationLoaded;
            set => this.SetProperty(ref this.applicationLoaded, value);
        }

        public bool ApplicationCanStart
        {
            get => this.applicationCanStart;
            set => this.SetProperty(ref this.applicationCanStart, value);
        }

        public MainMenuViewModel()
        {
            this.PropertyChanged += this.MainPageViewModel_PropertyChanged;
        }

        private void MainPageViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.ApplicationCanStart = this.ApplicationLoaded;// && !string.IsNullOrEmpty(this.ModelPath);
        }
    }
}
