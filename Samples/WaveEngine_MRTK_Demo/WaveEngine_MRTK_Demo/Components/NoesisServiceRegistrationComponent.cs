using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.NoesisGUI;

namespace WaveEngine_MRTK_Demo.Components
{
    public class NoesisServiceRegistrationComponent : Component
    {
        [WaveIgnore]
        public NoesisService NoesisService;

        private string noesisStyle = null;

        public string NoesisStyle
        {
            get => this.noesisStyle;
            set
            {
                this.noesisStyle = value;
                if (this.NoesisService != null)
                {
                    this.NoesisService.Style = value;
                }
            }
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();

            this.NoesisService = Application.Current.Container.Resolve<NoesisService>();
            if (this.NoesisService == null)
            {
                this.NoesisService = new NoesisService()
                {
                    Style = this.noesisStyle,
                    IsEnabled = this.IsEnabled
                };

                Application.Current.Container.RegisterInstance(this.NoesisService);
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            this.NoesisService.IsEnabled = true;
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            // this.NoesisService.IsEnabled = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Application.Current.Container.Unregister<NoesisService>();
            this.NoesisService = null;
        }
    }
}
