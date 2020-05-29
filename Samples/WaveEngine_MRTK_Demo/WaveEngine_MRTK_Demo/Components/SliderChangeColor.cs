using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Effects;
using WaveEngine.MRTK.SDK.Features.UX.Components.Sliders;
using WaveEngine.MRTK.Toolkit.Prefabs;
using WaveEngine_MRTK_Demo.Effects;
using WaveEngine_MRTK_Demo.Toolkit.Components.GUI;

namespace WaveEngine_MRTK_Demo.Components
{
    class SliderChangeColor : Component
    {
        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderRed")]
        protected ScenePrefab pinchSliderPrefabR = null;

        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderGreen")]
        protected ScenePrefab pinchSliderPrefabG = null;

        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderBlue")]
        protected ScenePrefab pinchSliderPrefabB = null;

        [BindComponent(isRequired: true, source: BindComponentSource.Owner)]
        protected MaterialComponent materialComponent = null;

        private PinchSlider[] pinchSliders = new PinchSlider[3];
        private HoloGraphic materialDecorator;

        protected override void Start()
        {
            if (Application.Current.IsEditor) return;

            materialDecorator = new HoloGraphic(materialComponent.Material);

            this.pinchSliders[0] = pinchSliderPrefabR.Owner.FindComponentInChildren<PinchSlider>();
            this.pinchSliders[1] = pinchSliderPrefabG.Owner.FindComponentInChildren<PinchSlider>();
            this.pinchSliders[2] = pinchSliderPrefabB.Owner.FindComponentInChildren<PinchSlider>();

            string[] titles = {"Red", "Green", "Blue"};
            for (int i = 0; i < 3; ++i)
            {
                PinchSlider p = pinchSliders[i];
                Entity title = p.Owner.FindChild("Title", true);
                if (title != null)
                {
                    Text3D text = title.FindComponent<Text3D>();
                    if (text != null)
                    {
                        text.Text = titles[i];
                    }
                }

                p.InitialValue = materialDecorator.Parameters_Color[i];
                p.SliderValue = materialDecorator.Parameters_Color[i];
                p.ValueUpdated += this.P_ValueUpdated;
            }
        }

        protected override void OnDestroy()
        {
            if (Application.Current.IsEditor) return;

            for (int i = 0; i < 3; ++i)
            {
                this.pinchSliders[i].ValueUpdated -= this.P_ValueUpdated;
            }
        }

        private void P_ValueUpdated(object sender, SliderEventData e)
        {
            for (int i = 0; i < this.pinchSliders.Length; ++i)
            {
                if (sender == this.pinchSliders[i])
                {
                    Vector3 c = this.materialDecorator.Parameters_Color;
                    c[i] = (e.NewValue);
                    this.materialDecorator.Parameters_Color = c;

                    break;
                }
            }
        }
    }
}
