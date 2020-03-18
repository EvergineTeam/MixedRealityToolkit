using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Common.Audio;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Media;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.Sound;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics.Materials;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;
using WaveEngine.MRTK.SDK.Features.UX.Components.Sliders;
using WaveEngine_MRTK_Demo.Behaviors;
using WaveEngine_MRTK_Demo.Effects;

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
        HoloGraphic materialDecorator;

        protected override void Start()
        {
            materialDecorator = new HoloGraphic(materialComponent.Material);

            pinchSliders[0] = pinchSliderPrefabR.Owner.FindComponentInChildren<PinchSlider>();
            pinchSliders[1] = pinchSliderPrefabG.Owner.FindComponentInChildren<PinchSlider>();
            pinchSliders[2] = pinchSliderPrefabB.Owner.FindComponentInChildren<PinchSlider>();

            string[] titles = {"Red", "Green", "Blue"};
            for (int i = 0; i < 3; ++i)
            {
                PinchSlider p = pinchSliders[i];
                Entity title = p.Owner.FindChild("Title", true);
                if(title != null)
                {
                    Text3D text = title.FindComponent<Text3D>();
                    if(text != null)
                    {
                        text.Text = titles[i];
                    }
                }

                p.SliderValue = materialDecorator.Parameters_Color[i];
                p.ValueUpdated += P_ValueUpdated;
            }
        }

        protected override void OnDestroy()
        {
            for (int i = 0; i < 3; ++i)
            {
                pinchSliders[i].ValueUpdated -= P_ValueUpdated;
            }
        }

        private void P_ValueUpdated(object sender, SliderEventData e)
        {
            for (int i = 0; i < pinchSliders.Length; ++i)
            {
                if(sender == pinchSliders[i])
                {
                    Vector3 c = materialDecorator.Parameters_Color;
                    c[i] = (e.NewValue);
                    materialDecorator.Parameters_Color = c;

                    break;
                }
            }
        }
    }
}
