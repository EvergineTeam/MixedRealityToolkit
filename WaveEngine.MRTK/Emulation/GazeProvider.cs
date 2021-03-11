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
        /// Gets or sets the radius of the gaze hover light.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0, maxLimit: 1, Tooltip = "Specifies the radius of the gaze hover light.")]
        public float Radius { get; set; } = 0.15f;

        /// <summary>
        /// Gets or sets the highlight color.
        /// </summary>
        [RenderProperty(Tooltip = "Specifies the highlight color.")]
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

        private Transform3D hoverLightTransform;
        private Collider3D hoverLightCollider;
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
                this.hoverLightTransform = new Transform3D();
                this.hoverLightCollider = new SphereCollider3D() { Radius = this.Radius };
                var hoverLightEntity = new Entity("GazePointer")
                    .AddComponent(this.hoverLightTransform)
                    .AddComponent(this.hoverLightCollider)
                    .AddComponent(new HoverLight() { Radius = this.Radius, Color = this.Color });
                this.Managers.EntityManager.Add(hoverLightEntity);

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
                ray = this.xrPlatform.IsEyeGazeValid ? this.xrPlatform.EyeGaze : null;
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
                Ray r = ray.Value;
                var from = Matrix4x4.CreateTranslation(r.Position);
                var to = Matrix4x4.CreateTranslation(r.GetPoint(this.Distance));
                var result = this.Managers.PhysicManager3D.ConvexSweepTest(this.hoverLightCollider.InternalColliderShape, from, to, this.Mask);
                Entity hitEntity = result.Succeeded ? ((PhysicBody3D)result.PhysicBody.UserData).Owner : null;
                if (hitEntity != this.GazeTarget)
                {
                    this.GazeTarget = hitEntity;
                }

                // Update Hover light
                var position = result.Succeeded ? Vector3.Project(result.Point - r.Position, r.Direction) + r.Position : r.GetPoint(100000.0f);
                this.hoverLightTransform.Position = position;
            }
            else
            {
                this.GazeTarget = null;
            }
        }
    }
}
