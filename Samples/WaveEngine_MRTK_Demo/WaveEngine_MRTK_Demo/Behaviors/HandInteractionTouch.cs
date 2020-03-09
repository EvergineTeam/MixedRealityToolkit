using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Common.Audio;
using WaveEngine.Common.Media;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.Sound;
using WaveEngine.Framework;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;
using WaveEngine_MRTK_Demo.Effects;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class HandInteractionTouch : Behavior, IMixedRealityTouchHandler
    {
        [BindComponent]
        protected SoundEmitter3D soundEmitter;

        [BindComponent(isRequired: true, source: BindComponentSource.Owner)]
        protected MaterialComponent materialComponent;

        public AudioBuffer sound { get; set; }

        HoloGraphic materialDecorator;
        bool animating = false;
        float animation_time;
        Vector3 cachedColor;

        protected override void Start()
        {
            materialDecorator = new HoloGraphic(materialComponent.Material);
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            animating = false;
            materialDecorator.Matrices_Color = cachedColor;
        }

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            animating = true;
            animation_time = 0.0f;
            cachedColor = materialDecorator.Matrices_Color;

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
    }
}
