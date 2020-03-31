using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Common.Audio;
using WaveEngine.Common.Media;
using WaveEngine.Components.Sound;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class HandInteractionTouchRotate : Behavior, IMixedRealityTouchHandler
    {
        [BindComponent(isRequired: true, source: BindComponentSource.Children, tag: "Rotate")]
        protected Transform3D target;

        public float speed = 3.0f;

        public AudioBuffer sound { get; set; }

        private bool rotate;
        private SoundEmitter3D soundEmitter;

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            rotate = false;
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

            rotate = true;
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
            if (rotate)
            {
                target.Orientation *= Quaternion.CreateFromAxisAngle(Vector3.Down, speed * (float)gameTime.TotalSeconds);
            }
        }
    }
}
