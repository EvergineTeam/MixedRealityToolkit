using WaveEngine.Framework;
using WaveEngine.NoesisGUI;

namespace WaveEngine_MRTK_Demo.Components
{
    public class NoesisServiceRegistrationComponent : Component
    {
        protected override void OnLoaded()
        {
            base.OnLoaded();

            var noesisService = Application.Current.Container.Resolve<NoesisService>();
            if (noesisService == null)
            {
                noesisService = new NoesisService()
                {
                    Style = null,
                };

                Application.Current.Container.RegisterInstance(noesisService);
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Application.Current.Container.Unregister<NoesisService>();
        }
    }
}
