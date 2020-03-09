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
    class SliderChangeColor : Behavior, IMixedRealityTouchHandler
    {
        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderRed")]
        protected ScenePrefab pinchSliderPrefabR;

        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderGreen")]
        protected ScenePrefab pinchSliderPrefabG;

        [BindComponent(isRequired: true, source: BindComponentSource.Scene, tag: "PinchSliderBlue")]
        protected ScenePrefab pinchSliderPrefabB;

        [BindComponent(isRequired: true, source: BindComponentSource.Owner)]
        protected MaterialComponent materialComponent;

        [BindComponent]
        protected SoundEmitter3D soundEmitter;
        
        public AudioBuffer sound { get; set; }

        PinchSlider[] pinchSliders = new PinchSlider[3];
        HoloGraphic materialDecorator;
        
        bool animating = false;
        float animation_time;

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            animating = false;
            materialDecorator.Matrices_Color = new Vector3(pinchSliders[0].SliderValue, pinchSliders[1].SliderValue, pinchSliders[2].SliderValue);
        }

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            animating = true;
            animation_time = 0.0f;
            materialDecorator.Matrices_Color = new Vector3(pinchSliders[0].SliderValue, pinchSliders[1].SliderValue, pinchSliders[2].SliderValue);

            if (this.soundEmitter != null)
            {
                if (this.soundEmitter.PlayState == PlayState.Playing)
                {
                    this.soundEmitter.Stop();
                }

                this.soundEmitter.Audio = sound;

                this.soundEmitter.Play();
            }
        }

        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
        }

        protected override void Start()
        {
            materialDecorator = new HoloGraphic(materialComponent.Material);

            pinchSliders[0] = pinchSliderPrefabR.Owner.FindComponentInChildren<PinchSlider>();
            pinchSliders[1] = pinchSliderPrefabG.Owner.FindComponentInChildren<PinchSlider>();
            pinchSliders[2] = pinchSliderPrefabB.Owner.FindComponentInChildren<PinchSlider>();

            for (int i = 0; i < 3; ++i)
            {
                PinchSlider p = pinchSliders[i];
                p.SliderValue = materialDecorator.Matrices_Color[i];
                p.ValueUpdated += P_ValueUpdated;
            }
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (animating)
            {
                animation_time += (float)gameTime.TotalSeconds;

                Vector3 c = materialDecorator.Matrices_Color;
                c[0] = 0.5f + (float)Math.Sin(animation_time * 2.0f) * 0.5f;
                c[1] = 1.0f - c[0];
                c[2] = 0.0f;
                materialDecorator.Matrices_Color = c;
            }
        }

        private void P_ValueUpdated(object sender, SliderEventData e)
        {
            for (int i = 0; i < pinchSliders.Length; ++i)
            {
                if(sender == pinchSliders[i])
                {
                    Vector3 c = materialDecorator.Matrices_Color;
                    c[i] = (e.NewValue);
                    materialDecorator.Matrices_Color = c;

                    break;
                }
            }
        }
    }
}
