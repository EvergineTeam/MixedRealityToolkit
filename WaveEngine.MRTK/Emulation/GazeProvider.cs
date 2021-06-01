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
        protected Transform3D transform = null;

        /// <summary>
        /// The camera.
        /// </summary>
        [BindComponent]
        protected Camera3D camera;

        /// <summary>
        /// The XR platform dependency.
        /// </summary>
        [BindService(isRequired: false)]
        protected XRPlatform xrPlatform;

        private IVoiceCommandService voiceCommandService;

        private Entity gazePointerEntity;
        private Transform3D gazePointerTransform;
        private HoverLight gazePointerLight;

        private ISphereColliderShape3D gazePointerShape;

        private IFocusable currentFocusable;

        /// <summary>
        /// Gets or sets the max distance to capture.
        /// </summary>
        public float Distance { get; set; } = 10.0f;

        /// <summary>
        /// Gets or sets a value indicating whether the gaze pointer should have an hover light.
        /// </summary>
        [RenderProperty(Tooltip = "Whether the gaze pointer should have an hover light.")]
        public bool HasHoverLight
        {
            get => this.hasHoverLight;
            set
            {
                if (this.hasHoverLight != value)
                {
                    this.hasHoverLight = value;

                    if (this.gazePointerLight != null)
                    {
                        this.gazePointerLight.IsEnabled = value;
                    }
                }
            }
        }

        private bool hasHoverLight;

        /// <summary>
        /// Gets or sets the radius of the gaze pointer.
        /// </summary>
        [RenderPropertyAsFInput(minLimit: 0, maxLimit: 1, Tooltip = "Specifies the radius of the gaze pointer.")]
        public float Radius { get; set; } = 0.05f;

        /// <summary>
        /// Gets or sets the hover light color.
        /// </summary>
        [RenderProperty(Tooltip = "Specifies the hover light color.")]
        public Color Color { get; set; } = new Color(63, 63, 63, 255);

        /// <summary>
        /// Gets or sets the collision category bits.
        /// </summary>
        public CollisionCategory3D CollisionCategoryMask { get; set; } = CollisionCategory3D.All;

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
                if (this.gazeTarget == value)
                {
                    return;
                }

                this.currentFocusable?.OnFocusExit();
                this.currentFocusable = null;

                this.gazeTarget = value;

                if (this.gazeTarget != null)
                {
                    this.currentFocusable = this.FindComponent<IFocusable>(this.gazeTarget);
                    this.currentFocusable?.OnFocusEnter();
                }
            }
        }

        private Entity gazeTarget;

        private T FindComponent<T>(Entity entity)
            where T : class
        {
            foreach (Component c in entity.Components)
            {
                if (c.IsActivated && c is T typed)
                {
                    return typed;
                }
            }

            return null;
        }

        /// <inheritdoc />
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            if (Application.Current.IsEditor)
            {
                return true;
            }

            this.voiceCommandService = Application.Current.Container.Resolve<IVoiceCommandService>();

            this.gazePointerShape = this.Managers.PhysicManager3D.CreateColliderShape<ISphereColliderShape3D>();
            this.gazePointerShape.Radius = this.Radius;

            this.gazePointerTransform = new Transform3D();
            this.gazePointerLight = new HoverLight()
            {
                IsEnabled = this.hasHoverLight,
                Radius = this.Radius,
                Color = this.Color,
            };

            this.gazePointerEntity = new Entity("GazePointer")
                   .AddComponent(this.gazePointerTransform)
                   .AddComponent(this.gazePointerLight);

            return true;
        }

        /// <inheritdoc />
        protected override void OnActivated()
        {
            base.OnActivated();

            this.Managers.EntityManager.Add(this.gazePointerEntity);

            if (this.voiceCommandService != null)
            {
                this.voiceCommandService.CommandRecognized += this.VoiceCommandService_CommandRecognized;
            }
        }

        /// <inheritdoc />
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.Managers.EntityManager.Detach(this.gazePointerEntity);

            if (this.voiceCommandService != null)
            {
                this.voiceCommandService.CommandRecognized -= this.VoiceCommandService_CommandRecognized;
            }
        }

        /// <inheritdoc />
        protected override void OnDetach()
        {
            base.OnDetach();

            this.gazePointerShape?.Dispose();
            this.gazePointerShape = null;

            this.gazePointerEntity.Destroy();
            this.gazePointerEntity = null;
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

                var result = this.Managers.PhysicManager3D.RayCast(ref fromPosition, ref toPosition, this.CollisionCategoryMask);

                this.GazeTarget = result.Succeeded ? result.PhysicBody.BodyComponent.Owner : null;

                // Update gaze pointer
                if (this.hasHoverLight)
                {
                    var targetPosition = result.Succeeded ? Vector3.Project(result.Point - r.Position, r.Direction) + r.Position : r.GetPoint(100000.0f);
                    this.gazePointerTransform.Position = targetPosition;
                }
            }
            else
            {
                this.GazeTarget = null;
            }
        }
    }
}
