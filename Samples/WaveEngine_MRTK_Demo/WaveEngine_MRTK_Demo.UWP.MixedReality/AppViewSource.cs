using Windows.ApplicationModel.Core;

namespace WaveEngine_MRTK_Demo.UWP.MixedReality
{
    // The entry point for the app.
    internal class AppViewSource : IFrameworkViewSource
    {
        private MyApplication application;

        public AppView AppView { get; private set; }

        public AppViewSource(MyApplication app)
        {
            this.application = app;
        }

        public IFrameworkView CreateView()
        {
            this.AppView = new AppView(this.application);
            return this.AppView;
        }
    }
}
