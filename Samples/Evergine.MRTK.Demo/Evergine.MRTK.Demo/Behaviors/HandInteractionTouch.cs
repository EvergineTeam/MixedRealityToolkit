using System;
using Evergine.Common.Audio;
using Evergine.Common.Graphics;
using Evergine.Common.Media;
using Evergine.Components.Graphics3D;
using Evergine.Components.Sound;
using Evergine.Framework;
using Evergine.Framework.Physics3D;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.Effects;
using Evergine.MRTK.SDK.Features;

namespace Evergine.MRTK.Demo.Behaviors
{
    public class HandInteractionTouch : Behavior, IMixedRealityTouchHandler
    {
        [BindComponent(isRequired: true, source: BindComponentSource.Owner)]
        protected MaterialComponent materialComponent;

        public AudioBuffer sound { get; set; }

        private SoundEmitter3D soundEmitter;
        private HoloGraphic materialDecorator;
        private bool animating = false;
        private float animation_time;
        private Vector3 cachedColor;

        protected override bool OnAttached()
        {
            if (!Application.Current.IsEditor)
            {
                Owner.GetOrAddComponent<BoxCollider3D>();
                Owner.GetOrAddComponent<StaticBody3D>();
                soundEmitter = Owner.GetOrAddComponent<SoundEmitter3D>();
            }

            return base.OnAttached();
        }

        protected override void Start()
        {
            materialDecorator = new HoloGraphic(materialComponent.Material);
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            animating = false;
            materialDecorator.Parameters_Color = cachedColor;
        }

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            animating = true;
            animation_time = 0.0f;
            cachedColor = materialDecorator.Parameters_Color;

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

                LinearColor c = new LinearColor(materialDecorator.Parameters_Color);
                c.R = 0.5f + (float)Math.Sin(animation_time * 2.0f) * 0.5f;
                c.G = 1.0f - c.R;
                c.B = 0.0f;
                materialDecorator.Parameters_Color = c.AsVector3;
            }
        }
    }
}
