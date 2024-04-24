// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.
using Evergine.Common.Attributes;
using Evergine.Common.Input;
using Evergine.Components.XR;
using Evergine.Framework;
using Evergine.Framework.Physics3D;
using Evergine.Framework.Prefabs;
using Evergine.Framework.Threading;
using Evergine.Mathematics;
using System;

namespace Evergine.MRTK.Emulation
{
    /// <summary>
    /// Ray for selecting where to teleport.
    /// </summary>
    public class TeleportRay : Behavior
    {
        //// Teleport Effect Area
        ////private Entity teleportAreaEntity = null;
        ////private StaticBody3D teleportAreaStaticBody = null;
        ////private SphereCollider3D sphereCollider = null;
        ////private Transform3D teleportAreaTransform = null;

        /// <summary>
        /// Prefab for teleportation area.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Teleport Area", Tooltip = "The teleport area prefab to be instantiated")]
        public Prefab TeleportationAreaPrefab = null;

        /// <summary>
        /// Prefab for teleportation line.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Teleport Area", Tooltip = "The teleport line prefab to be instantiated")]
        public Prefab TeleportationLinePrefab = null;

        /// <summary>
        /// Gets or sets the gravity to be applied to the pointer arc trace.
        /// </summary>
        [RenderProperty(CustomPropertyName = "Pointer Gravity", Tooltip = "The gravity to be applied to the pointer arc trace")]
        public float Gravity { get; set; } = -9.8f;

        /// <summary>
        /// Controller to track with ray.
        /// </summary>
        public TrackXRController controller = null;

        //// Prefab entities
        private Entity teleportationAreaEntity = null;
        private Entity teleportationLineEntity = null;

        //// Collisions
        ////private bool detectedCollisionArea = false;
        ////private bool groundDetected = false;
        ////private bool rayCollisionDetected = false;

        private bool isManipulationActive = false;

        /// <inheritdoc/>
        protected override async void Start()
        {
            base.Start();
            if (Application.Current.IsEditor)
            {
                return;
            }

            await EvergineForegroundTask.Run(() =>
            {
                this.teleportationAreaEntity = this.TeleportationAreaPrefab.Instantiate();
                if (this.teleportationAreaEntity != null)
                {
                    this.teleportationAreaEntity.IsEnabled = false;
                    this.Managers.EntityManager.Add(this.teleportationAreaEntity);
                }

                this.teleportationLineEntity = this.TeleportationLinePrefab.Instantiate();
                if (this.teleportationLineEntity != null)
                {
                    this.Managers.EntityManager.Add(this.teleportationAreaEntity);
                }

                var teleportationAreaStaticBody = this.teleportationAreaEntity.FindComponent<StaticBody3D>();
                if (teleportationAreaStaticBody != null)
                {
                    teleportationAreaStaticBody.BeginCollision += this.TeleportationAreaStaticBodyBeginCollision;
                    teleportationAreaStaticBody.EndCollision += this.TeleportationAreaStaticBodyEndCollision;
                }
            });
        }

        private void TeleportationAreaStaticBodyEndCollision(object sender, CollisionInfo3D e)
        {
            ////this.detectedCollisionArea = false;
        }

        private void TeleportationAreaStaticBodyBeginCollision(object sender, CollisionInfo3D e)
        {
            ////this.detectedCollisionArea = true;
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            if (this.controller == null)
            {
                return;
            }

            if (!this.isManipulationActive && !this.IsCursorCollision())
            {
                var btnState = this.controller.ControllerState.TriggerButton;
                if (btnState == ButtonState.Pressed)
                {
                    this.PressedBehavior();
                }
                else if (btnState == ButtonState.Releasing)
                {
                    this.ReleasingBehavior();
                }
            }
            else
            {
                this.teleportationAreaEntity.IsEnabled = false;
                this.DeleteLineMesh();
            }
        }

        private void DeleteLineMesh()
        {
            throw new NotImplementedException();
        }

        private void ReleasingBehavior()
        {
            throw new NotImplementedException();
        }

        private void PressedBehavior()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks if there is something too close to the cursor (the user could be trying to grab something).
        /// </summary>
        /// <returns>true if the user hand cursor is close to something.</returns>
        private bool IsCursorCollision()
        {
            Ray ray = this.controller.Pointer;
            var collisionResult = this.Managers.PhysicManager3D.RayCast(ref ray, 0.2f, CollisionCategory3D.All);

            return collisionResult.Succeeded;
        }
    }
}
