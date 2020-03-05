using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics.Materials;
using WaveEngine.MRTK.SDK.Features.UX.Components.Sliders;
using WaveEngine_MRTK_Demo.Behaviors;

namespace WaveEngine_MRTK_Demo.Components
{
    class SliderChangeColor : Component
    {
        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderRed")]
        protected ScenePrefab pinchSliderPrefabR;

        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderGreen")]
        protected ScenePrefab pinchSliderPrefabG;

        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderBlue")]
        protected ScenePrefab pinchSliderPrefabB;

        [BindComponent(isRequired: true, source: BindComponentSource.Owner)]
        protected MaterialComponent materialComponent;

        PinchSlider[] pinchSliders = new PinchSlider[3];
        StandardMaterial materialDecorator;

        protected override bool OnAttached()
        {
            materialDecorator = new StandardMaterial(materialComponent.Material);

            pinchSliders[0] = pinchSliderPrefabR.Owner.FindComponentInChildren<PinchSlider>();
            pinchSliders[1] = pinchSliderPrefabG.Owner.FindComponentInChildren<PinchSlider>();
            pinchSliders[2] = pinchSliderPrefabB.Owner.FindComponentInChildren<PinchSlider>();
            foreach(PinchSlider p in pinchSliders)
            {
                p.ValueUpdated += P_ValueUpdated;
                P_ValueUpdated(p, new SliderEventData(p.InitialValue, p.InitialValue));
            }

            return true;
        }

        private void P_ValueUpdated(object sender, SliderEventData e)
        {
            for (int i = 0; i < pinchSliders.Length; ++i)
            {
                if(sender == pinchSliders[i])
                {
                    Color c = materialDecorator.BaseColor;
                    c[i] = (byte)(255 * e.NewValue);
                    materialDecorator.BaseColor = c;

                    break;
                }
            }
        }
    }
}
