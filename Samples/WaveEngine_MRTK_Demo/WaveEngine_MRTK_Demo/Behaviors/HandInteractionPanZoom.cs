using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;
using WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons;
using WaveEngine.MRTK.Services.InputSystem;
using WaveEngine_MRTK_Demo.Effects;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class HandInteractionPanZoom : Behavior, IMixedRealityTouchHandler
    {
        [BindComponent]
        protected Transform3D transform = null;

        [BindComponent(isRequired: true, source: BindComponentSource.Owner)]
        protected MaterialComponent materialComponent;

        [BindComponent]
        protected NearInteractionTouchable nearInteractionTouchable;

        public float maxScale = 4.0f;
        public float minScale = 0.1f;

        private Slate slateDecorator;
        private CursorManager cursorManager;

        class TouchInfo
        {
            public Vector2 uv; // uv when the collision started
            public Entity cursor;
            public Transform3D transform;
        }

        private List<TouchInfo> touchInfos = new List<TouchInfo>();

        protected override void Start()
        {
            slateDecorator = new Slate(materialComponent.Material);
            cursorManager = Owner.Scene.Managers.FindManager<CursorManager>();
        }

        private Vector2 GetUVPos(Vector3 position)
        {
            var matrix = this.transform.WorldInverseTransform * this.nearInteractionTouchable.BoxCollider3DTransformInverse;
            Vector3 localPos =  Vector3.TransformCoordinate(position, matrix);

            Vector2 uv0 = slateDecorator.Matrices_Offset;
            Vector2 uv1 = uv0 + Vector2.One * slateDecorator.Matrices_Tiling;

            Vector2 t = new Vector2(localPos.X + 0.5f, localPos.Z + 0.5f);

            return new Vector2(
                MathHelper.Lerp(uv0.X, uv1.X, t.X),
                MathHelper.Lerp(uv0.Y, uv1.Y, t.Y)
            );
        }

        protected override void Update(TimeSpan gameTime)
        {
            if (touchInfos.Count > 0)
            {
                //Scale
                if(touchInfos.Count > 1)
                {
                    float d0 = (touchInfos[0].uv - touchInfos[1].uv).Length();
                    float d1 = (GetUVPos(touchInfos[0].transform.Position) - GetUVPos(touchInfos[1].transform.Position)).Length();

                    float scale = slateDecorator.Matrices_Tiling.X * d0 / d1;
                    if(scale < minScale || scale > maxScale)
                    {
                        scale = MathHelper.Clamp(scale, minScale, maxScale);
                        RemapTouches();
                    }

                    slateDecorator.Matrices_Tiling = new Vector2(scale, scale);
                }

                //Translate
                Vector2 uv = GetUVPos(touchInfos[0].transform.Position);
                Vector2 disp = uv - touchInfos[0].uv;
                slateDecorator.Matrices_Offset -= disp;
            }
        }

        private void RemapTouches()
        {
            foreach (TouchInfo t in touchInfos)
            {
                t.uv = GetUVPos(t.transform.Position);
            }
        }

        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            touchInfos.Add(new TouchInfo { cursor = eventData.Cursor, transform = eventData.Cursor.FindComponent< Transform3D >(), uv = GetUVPos(eventData.Position)});
        }

        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
        }

        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            var node = touchInfos[0];
            
            for(int i = 0; i < touchInfos.Count; ++ i)
            {
                if (touchInfos[i] .cursor == eventData.Cursor)
                {
                    touchInfos.RemoveAt(i);
                    break;
                }
            }

            RemapTouches();
        }
    }
}
