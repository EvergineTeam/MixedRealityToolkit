using System;
using Evergine.Common.Audio;
using Evergine.Common.Media;
using Evergine.Components.Sound;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.SDK.Features;

namespace Evergine.MRTK.Demo.Behaviors
{
    public class HandInteractionTouchRotate : Behavior, IMixedRealityTouchHandler
    {
        [BindComponent(isRequired: true, source: BindComponentSource.Children, tag: "Rotate")]
        protected Transform3D target;

        public float speed = 3.0f;

        public AudioBuffer sound { get; set; }

        private LockCounter rotate;
        private SoundEmitter3D soundEmitter;

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            rotate.Unlock();
        }

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            if (this.soundEmitter != null)
            {
                if (this.soundEmitter.PlayState == PlayState.Playing)
                {
                    this.soundEmitter.Stop();
                }

                this.soundEmitter.Audio = sound;

                this.soundEmitter.Play();
            }

            rotate.Lock();
        }

        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
        }

        protected override bool OnAttached()
        {
            if (!Application.Current.IsEditor)
            {
                soundEmitter = Owner.GetOrAddComponent<SoundEmitter3D>();
            }

            return base.OnAttached();
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (rotate.IsLocked)
            {
                target.Orientation *= Quaternion.CreateFromAxisAngle(Vector3.Down, speed * (float)gameTime.TotalSeconds);
            }
        }

        public void OnFocusEnter()
        {
            rotate.Lock();
        }

        public void OnFocusExit()
        {
            rotate.Unlock();
        }
    }
}
