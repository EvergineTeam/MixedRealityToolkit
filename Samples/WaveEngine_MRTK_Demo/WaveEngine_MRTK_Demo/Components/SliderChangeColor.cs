using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Effects;
using WaveEngine.MRTK.SDK.Features.UX.Components.Sliders;
using WaveEngine.MRTK.Toolkit.GUI;
using WaveEngine.MRTK.Toolkit.Prefabs;

namespace WaveEngine_MRTK_Demo.Components
{
    class SliderChangeColor : Component
    {
        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderRed")]
        private ScenePrefab pinchSliderPrefabR = null;

        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderGreen")]
        private ScenePrefab pinchSliderPrefabG = null;

        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderBlue")]
        private ScenePrefab pinchSliderPrefabB = null;

        [BindComponent(isRequired: true, source: BindComponentSource.Owner)]
        private MaterialComponent materialComponent = null;

        private PinchSlider[] pinchSliders = new PinchSlider[3];
        private HoloGraphic materialDecorator;

        protected override void Start()
        {
            if (Application.Current.IsEditor)
            {
                return;
            }

            this.materialDecorator = new HoloGraphic(materialComponent.Material);

            this.pinchSliders[0] = this.pinchSliderPrefabR.Owner.FindComponentInChildren<PinchSlider>();
            this.pinchSliders[1] = this.pinchSliderPrefabG.Owner.FindComponentInChildren<PinchSlider>();
            this.pinchSliders[2] = this.pinchSliderPrefabB.Owner.FindComponentInChildren<PinchSlider>();

            for (int i = 0; i < 3; ++i)
            {
                var p = pinchSliders[i];

                p.InitialValue = this.materialDecorator.Parameters_Color[i];
                p.SliderValue = this.materialDecorator.Parameters_Color[i];
                p.ValueUpdated += this.PinchSlider_ValueUpdated;
            }
        }

        protected override void OnDestroy()
        {
            if (Application.Current.IsEditor)
            {
                return;
            }

            for (int i = 0; i < 3; ++i)
            {
                this.pinchSliders[i].ValueUpdated -= this.PinchSlider_ValueUpdated;
            }
        }

        private void PinchSlider_ValueUpdated(object sender, SliderEventData e)
        {
            for (int i = 0; i < this.pinchSliders.Length; ++i)
            {
                if (sender == this.pinchSliders[i])
                {
                    var c = this.materialDecorator.Parameters_Color;
                    c[i] = e.NewValue;
                    this.materialDecorator.Parameters_Color = c;

                    break;
                }
            }
        }
    }
}
