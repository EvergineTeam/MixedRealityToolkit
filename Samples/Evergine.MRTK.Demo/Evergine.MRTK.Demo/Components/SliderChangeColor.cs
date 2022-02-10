using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.MRTK.Effects;
using Evergine.MRTK.SDK.Features.UX.Components.Sliders;

namespace Evergine.MRTK.Demo.Components
{
    class SliderChangeColor : Component
    {
        [BindEntity(isRequired: true, source: BindEntitySource.Scene, tag: "PinchSliderRed")]
        private Entity pinchSliderPrefabR = null;

        [BindEntity(isRequired: true, source: BindEntitySource.Scene, tag: "PinchSliderGreen")]
        private Entity pinchSliderPrefabG = null;

        [BindEntity(isRequired: true, source: BindEntitySource.Scene, tag: "PinchSliderBlue")]
        private Entity pinchSliderPrefabB = null;

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

            this.pinchSliders[0] = this.pinchSliderPrefabR.FindComponentInChildren<PinchSlider>();
            this.pinchSliders[1] = this.pinchSliderPrefabG.FindComponentInChildren<PinchSlider>();
            this.pinchSliders[2] = this.pinchSliderPrefabB.FindComponentInChildren<PinchSlider>();

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
