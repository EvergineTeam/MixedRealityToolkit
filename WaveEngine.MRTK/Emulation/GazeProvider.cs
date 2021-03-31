// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Graphics;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Emulation;
using WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons;

namespace WaveEngine.MRTK.Services.InputSystem
{
    /// <summary>
    /// The Gaze provider.
    /// </summary>
    public class GazeProvider : Behavior
    {
        /// <summary>
        /// The transform.
        /// </summary>
        [BindComponent]
        public Transform3D transform = null;

        /// <summary>
        /// The camera.
        /// </summary>
        [BindComponent]
        public Camera3D camera;

        /// <summary>
        /// Gets or sets the max distance to capture.
        /// </summary>
        public float Distance { get; set; } = 1000.0f;

        /// <summary>
        /// Gets or sets a value indicating whether the gaze pointer should have an hover light.
        /// </summary>
        [RenderProperty(Tooltip = "Whether the gaze pointer should have an hover light.")]
        public bool HasHoverLight { get; set; }

        /// <summary>
        /// Gets or sets the radius of the gaze pointer.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0, maxLimit: 1, Tooltip = "Specifies the radius of the gaze pointer.")]
        public float Radius { get; set; } = 0.15f;

        /// <summary>
        /// Gets or sets the hover light color.
        /// </summary>
        [RenderProperty(Tooltip = "Specifies the hover light color.")]
        public Color Color { get; set; } = new Color(63, 63, 63, 255);

        /// <summary>
        /// Gets or sets the collision category bits.
        /// </summary>
        public CollisionCategory3D Mask { get; set; } = CollisionCategory3D.All;

        private XRPlatform xrPlatform;

        /// <summary>
        /// Gets the target aimed by the gaze or null.
        /// </summary>
        public Entity GazeTarget
        {
            get
            {
                return this.gazeTarget;
            }

            private set
            {
                if (this.gazeTarget != null)
                {
                    IFocusable focusable = this.FindComponent<IFocusable>(this.gazeTarget);
                    focusable?.OnFocusExit();
                }

                this.gazeTarget = value;

                if (this.gazeTarget != null)
                {
                    IFocusable focusable = this.FindComponent<IFocusable>(this.gazeTarget);
                    focusable?.OnFocusEnter();
                }
            }
        }

        private Transform3D gazePointerTransform;
        private Collider3D gazePointerCollider;
        private Entity gazeTarget;

        private T FindComponent<T>(Entity entity)
            where T : class
        {
            foreach (Component c in entity.Components)
            {
                if (c is T)
                {
                    return c as T;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            var assetsService = Application.Current.Container.Resolve<AssetsService>();

            if (!Application.Current.IsEditor)
            {
                this.gazePointerTransform = new Transform3D();
                this.gazePointerCollider = new SphereCollider3D() { Radius = this.Radius };
                var gazePointerEntity = new Entity("GazePointer")
                    .AddComponent(this.gazePointerTransform)
                    .AddComponent(this.gazePointerCollider);

                if (this.HasHoverLight)
                {
                    gazePointerEntity.AddComponent(new HoverLight() { Radius = this.Radius, Color = this.Color });
                }

                this.Managers.EntityManager.Add(gazePointerEntity);

                this.xrPlatform = Application.Current.Container.Resolve<XRPlatform>();
                if (this.xrPlatform != null)
                {
                    IVoiceCommandService voiceCommandService = Application.Current.Container.Resolve<IVoiceCommandService>();
                    if (voiceCommandService != null)
                    {
                        voiceCommandService.CommandRecognized += this.VoiceCommandService_CommandRecognized;
                    }
                }
            }
        }

        private void VoiceCommandService_CommandRecognized(object sender, string e)
        {
            if (this.GazeTarget != null)
            {
                this.FindComponent<ISpeechHandler>(this.GazeTarget)?.OnSpeechKeywordRecognized(e);
            }
        }

        /// <summary>
        /// Update.
        /// </summary>
        /// <param name="gameTime">The time.</param>
        protected override void Update(TimeSpan gameTime)
        {
            Ray? ray;
            if (this.xrPlatform != null)
            {
                ray = this.xrPlatform.EyeGaze;
                if (ray == null)
                {
                    ray = this.xrPlatform.HeadGaze;
                }
            }
            else
            {
                ray = new Ray(this.transform.Position, this.transform.LocalTransform.Forward);
            }

            if (ray.HasValue)
            {
                var r = ray.Value;
                var fromPosition = r.Position;
                var toPosition = r.GetPoint(this.Distance);
                Matrix4x4.CreateTranslation(ref fromPosition, out var from);
                Matrix4x4.CreateTranslation(ref toPosition, out var to);
                var result = this.Managers.PhysicManager3D.ConvexSweepTest(this.gazePointerCollider.InternalColliderShape, from, to, this.Mask);

                var hitEntity = result.Succeeded ? ((PhysicBody3D)result.PhysicBody.UserData).Owner : null;
                if (hitEntity != this.GazeTarget)
                {
                    this.GazeTarget = hitEntity;
                }

                // Update gaze pointer
                var targetPosition = result.Succeeded ? Vector3.Project(result.Point - r.Position, r.Direction) + r.Position : r.GetPoint(100000.0f);
                this.gazePointerTransform.Position = targetPosition;
            }
            else
            {
                this.GazeTarget = null;
            }
        }
    }
}
