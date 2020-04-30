// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Audio;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.Sound;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;
using WaveEngine.MRTK.Effects;
using WaveEngine.MRTK.Emulation;
using WaveEngine.MRTK.SDK.Features;
using WaveEngine.MRTK.Services.InputSystem;

namespace WaveEngine_MRTK_Demo.Behaviors
{
    public class HandInteractionPanZoom : Behavior, IMixedRealityTouchHandler
    {
        /// <summary>
        /// The transform.
        /// </summary>
        [BindComponent]
        protected Transform3D transform = null;

        /// <summary>
        /// The material component.
        /// </summary>
        [BindComponent]
        protected MaterialComponent materialComponent;

        /// <summary>
        /// If zoom is allowed or not
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "Allow zoom or not")]
        public bool EnableZoom { get; set; } = true;

        /// <summary>
        /// Maximum zoom allowed
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "Maximum zoom value")]
        public float MaxZoom { get; set; } = 4.0f;

        /// <summary>
        /// Minimum zoom allowed
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "Minimum zoom value")]
        public float MinZoom { get; set; } = 0.1f;

        /// <summary>
        /// Lock horizontal pan
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "Lock horizontal pan")]
        public bool LockHorizontal { get; set; } = false;

        /// <summary>
        /// Lock vertical pan
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "Lock vertical pan")]
        public bool LockVertical { get; set; } = false;

        /// <summary>
        /// Pan drag
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "Pan drag", MinLimit = 0, MaxLimit = 1)]
        public float Drag { get; set; } = 0.15f;

        /// <summary>
        /// Gets or sets the sound to be played when the pan starts.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the pan starts")]
        public AudioBuffer PanStartedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when the pan ends.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the pan ends")]
        public AudioBuffer PanEndedSound { get; set; }

        private NearInteractionTouchable nearInteractionTouchable;
        private HoloGraphic slateDecorator;
        private CursorManager cursorManager;
        private Vector2 speed;
        private SoundEmitter3D soundEmitter;

        /// <summary>
        /// Internal class to store touches information
        /// </summary>
        private class TouchInfo
        {
            /// <summary>
            /// uv when the collision started
            /// </summary>
            public Vector2 UV { get; set; }

            /// <summary>
            /// The cursor
            /// </summary>
            public Entity Cursor { get; set; }

            /// <summary>
            /// The cursor's transform
            /// </summary>
            public Transform3D Transform { get; set; }
        }

        private List<TouchInfo> touchInfos = new List<TouchInfo>();

        /// <inheritdoc/>
        protected override void Start()
        {
            this.slateDecorator = new HoloGraphic(materialComponent.Material);
            this.cursorManager = this.Owner.Scene.Managers.FindManager<CursorManager>();

            if (!Application.Current.IsEditor)
            {
                nearInteractionTouchable = this.Owner.GetOrAddComponent<NearInteractionTouchable>();
                this.soundEmitter = this.Owner.GetOrAddComponent<SoundEmitter3D>();
                this.Owner.GetOrAddComponent<StaticBody3D>();
                this.Owner.GetInChildrenOrAddComponent<BoxCollider3D>();
            }
        }

        /// <summary>
        /// Given a world position it returns the uv
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector2 GetUVPos(Vector3 position)
        {
            // Local postion
            var matrix = this.transform.WorldInverseTransform * this.nearInteractionTouchable.BoxCollider3DTransformInverse;
            Vector3 localPos =  Vector3.TransformCoordinate(position, matrix);

            // Corners
            Vector2 uv0 = this.slateDecorator.Parameters_Offset;
            Vector2 uv1 = uv0 + Vector2.One * this.slateDecorator.Parameters_Tiling;

            // Calculate normalized local position
            Vector2 t = new Vector2(localPos.X + 0.5f, localPos.Z + 0.5f);

            return new Vector2(
                MathHelper.Lerp(uv0.X, uv1.X, t.X),
                MathHelper.Lerp(uv0.Y, uv1.Y, t.Y)
            );
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            this.speed *= Vector2.One * (1.0f - this.Drag);

            if (touchInfos.Count > 0)
            {
                //Scale
                if(this.EnableZoom && this.touchInfos.Count > 1)
                {
                    float d0 = (touchInfos[0].UV - touchInfos[1].UV).Length();
                    float d1 = (GetUVPos(touchInfos[0].Transform.Position) - GetUVPos(touchInfos[1].Transform.Position)).Length();

                    float scale = this.slateDecorator.Parameters_Tiling.X * d0 / d1;
                    if(scale < MinZoom || scale > MaxZoom)
                    {
                        scale = MathHelper.Clamp(scale, MinZoom, MaxZoom);
                        RemapTouches();
                    }

                    this.slateDecorator.Parameters_Tiling = new Vector2(scale, scale);
                }

                //Translate
                Vector2 uv = GetUVPos(touchInfos[0].Transform.Position);
                Vector2 disp = uv - touchInfos[0].UV;
                this.speed = -disp;
            }

            if (LockHorizontal)
                this.speed.X = 0;
            if (LockVertical)
                this.speed.Y = 0;
            slateDecorator.Parameters_Offset += speed;
        }

        /// <summary>
        /// Recalculate touches uvs ref
        /// </summary>
        private void RemapTouches()
        {
            foreach (TouchInfo t in touchInfos)
            {
                t.UV = GetUVPos(t.Transform.Position);
            }
        }

        /// <inheritdoc/>
        public void OnTouchStarted(HandTrackingInputEventData eventData)
        {
            this.touchInfos.Add(new TouchInfo { Cursor = eventData.Cursor, Transform = eventData.Cursor.FindComponent< Transform3D >(), UV = GetUVPos(eventData.Position)});
            Tools.PlaySound(soundEmitter, PanStartedSound);
        }

        /// <inheritdoc/>
        public void OnTouchUpdated(HandTrackingInputEventData eventData)
        {
            // Nothing to do
        }

        /// <inheritdoc/>
        public void OnTouchCompleted(HandTrackingInputEventData eventData)
        {
            var node = this.touchInfos[0];
            
            for(int i = 0; i < touchInfos.Count; ++ i)
            {
                if (this.touchInfos[i].Cursor == eventData.Cursor)
                {
                    this.touchInfos.RemoveAt(i);
                    break;
                }
            }

            RemapTouches();

            Tools.PlaySound(soundEmitter, PanEndedSound);
        }
    }
}
