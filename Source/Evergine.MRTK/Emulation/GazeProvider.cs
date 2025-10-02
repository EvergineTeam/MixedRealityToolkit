// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Common.Attributes;
using Evergine.Common.Graphics;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Framework.Services;
using Evergine.Mathematics;
using Evergine.MRTK.Emulation;
using Evergine.MRTK.SDK.Features;

namespace Evergine.MRTK.Services.InputSystem
{
    /// <summary>
    /// The Gaze provider.
    /// </summary>
    public class GazeProvider : Behavior
    {
        /// <summary>
        /// The XR platform dependency.
        /// </summary>
        [BindService(isRequired: false)]
        protected XRPlatform xrPlatform;

        [BindSceneManager]
        private FocusProvider focusProvider = null;

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

        private Entity gazePointerEntity;
        private Transform3D gazePointerTransform;
        private HoverLight gazePointerLight;

        private ISphereColliderShape3D gazePointerShape;

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
            get => this.gazeTarget;

            private set
            {
                if (this.gazeTarget == value)
                {
                    return;
                }

                this.focusProvider.FocusExit(this.gazeTarget, null);

                this.gazeTarget = value;

                this.focusProvider.FocusEnter(this.gazeTarget, null);
            }
        }

        private Entity gazeTarget;

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

            if (!Tools.IsXRPlatformInputTrackingAvailable())
            {
                this.xrPlatform = null;
            }

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
        }

        /// <inheritdoc />
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.Managers.EntityManager.Detach(this.gazePointerEntity);
        }

        /// <inheritdoc />
        protected override void OnDetached()
        {
            base.OnDetached();

            this.gazePointerShape?.Dispose();
            this.gazePointerShape = null;

            this.gazePointerEntity.Destroy();
            this.gazePointerEntity = null;
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
