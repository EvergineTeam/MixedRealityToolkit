using System;
using System.Collections.Generic;
using System.Text;
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

        bool rotate;

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            rotate = false;
        }

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            rotate = true;
        }

        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
        }

        protected override bool OnAttached()
        {
            return base.OnAttached();
        }


        protected override void Update(TimeSpan gameTime)
        {
            if (rotate)
            {
                target.Orientation *= Quaternion.CreateFromAxisAngle(Vector3.Up, speed * (float)gameTime.TotalSeconds);
            }
        }
    }
}
