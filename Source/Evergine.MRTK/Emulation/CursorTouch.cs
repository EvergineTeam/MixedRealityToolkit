// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using Evergine.Common.Attributes;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics;
using Evergine.Mathematics;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.Extensions;

namespace Evergine.MRTK.Emulation
{
    /// <summary>
    /// Finger cursor component.
    /// </summary>
    public class CursorTouch : Cursor
    {
        private const float ExternalTouchRadius = 0.15f;

        /// <summary>
        /// The <see cref="RigidBody"/> component dependency.
        /// </summary>
        [BindComponent]
        protected RigidBody rigidBody;

        private Entity externalTouchEntity;
        private RigidBody externalTouchBody;

        private Dictionary<PhysicsBody, Entity> externalCollisionEntitiesByBody = new Dictionary<PhysicsBody, Entity>();
        private Dictionary<PhysicsBody, Entity> nearCollisionEntitiesByBody = new Dictionary<PhysicsBody, Entity>();
        private HashSet<Entity> externalTouchCollisionEntities = new HashSet<Entity>();
        private HashSet<Entity> nearTouchCollisionEntities = new HashSet<Entity>();

        /// <summary>
        /// Gets a value indicating whether the cursor is close to a <see cref="IMixedRealityTouchHandler"/> entity.
        /// </summary>
        public bool IsExternalTouching => this.externalTouchCollisionEntities.Count > 0;

        /// <summary>
        /// Gets a value indicating whether the cursor is colliding with a <see cref="IMixedRealityPointerHandler"/>.
        /// </summary>
        [IgnoreEvergine]
        [DontRenderProperty]
        public bool IsTouching => this.nearTouchCollisionEntities.Count > 0;

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            if (this.externalTouchEntity == null)
            {
                this.externalTouchBody = new RigidBody()
                {
                    IsKinematic = true,
                    CollideKinematicVsNonDynamic = true,
                    IsSensor = true,
                    CollisionCategory = this.rigidBody.CollisionCategory,
                };

                this.externalTouchEntity = new Entity("ExternalTouch")
                                .AddComponent(new Transform3D())
                                .AddComponent(new SphereCollider() { Radius = ExternalTouchRadius })
                                .AddComponent(this.externalTouchBody);
            }

            this.rigidBody.CollisionStarted += this.InternalTouch_BeginCollision;
            this.rigidBody.CollisionUpdated += this.InternalTouch_UpdateCollision;
            this.rigidBody.CollisionEnded += this.InternalTouch_EndCollision;

            this.externalTouchBody.CollisionStarted += this.ExternalTouch_BeginCollision;
            this.externalTouchBody.CollisionEnded += this.ExternalTouch_EndCollision;
            this.Owner.AddChild(this.externalTouchEntity);
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            foreach (var item in this.nearTouchCollisionEntities)
            {
                this.RunTouchHandlers(item, (h, e) => h?.OnTouchCompleted(e));
            }

            this.rigidBody.CollisionStarted -= this.InternalTouch_BeginCollision;
            this.rigidBody.CollisionUpdated -= this.InternalTouch_UpdateCollision;
            this.rigidBody.CollisionEnded -= this.InternalTouch_EndCollision;
            this.nearCollisionEntitiesByBody.Clear();
            this.nearTouchCollisionEntities.Clear();

            if (this.externalTouchEntity != null)
            {
                this.externalTouchBody.CollisionStarted -= this.ExternalTouch_BeginCollision;
                this.externalTouchBody.CollisionEnded -= this.ExternalTouch_EndCollision;
                this.Owner.DetachChild(this.externalTouchEntity);
            }

            this.externalCollisionEntitiesByBody.Clear();
            this.externalTouchCollisionEntities.Clear();

            base.OnDeactivated();
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            base.Destroy();
            this.externalTouchEntity?.Destroy();
            this.externalTouchEntity = null;
        }

        private static Entity RegisterCollidedEntity(Dictionary<PhysicsBody, Entity> collisions, CollisionInfo info)
        {
            var otherEntity = info.OtherBody.Owner;
            collisions[info.OtherBody] = otherEntity;
            return otherEntity;
        }

        private static Entity UnregisterCollidedEntity(Dictionary<PhysicsBody, Entity> collisions, CollisionInfo info)
        {
            if (collisions.TryGetValue(info.OtherBody, out var otherEntity))
            {
                collisions.Remove(info.OtherBody);
                return otherEntity;
            }

            return null;
        }

        private void ExternalTouch_BeginCollision(object sender, CollisionInfo info)
        {
            var otherEntity = RegisterCollidedEntity(this.externalCollisionEntitiesByBody, info);
            otherEntity.AttachableStateChanged += this.OtherEntity_AttachableStateChanged_RemoveFromExternalTouch;

            var hasHandler = otherEntity.HasEventHandlers<IMixedRealityTouchHandler, IMixedRealityPointerHandler>();
            if (hasHandler)
            {
                this.externalTouchCollisionEntities.Add(otherEntity);
            }

            this.AddFocusableInteraction(otherEntity);
        }

        private void ExternalTouch_EndCollision(object sender, CollisionInfo info)
        {
            var otherEntity = UnregisterCollidedEntity(this.externalCollisionEntitiesByBody, info);

            this.RemoveEntityFromExternalTouch(otherEntity);
        }

        private void OtherEntity_AttachableStateChanged_RemoveFromExternalTouch(object sender, AttachableObjectState state)
        {
            var entity = (Entity)sender;

            if (state == AttachableObjectState.Deactivated)
            {
                this.RemoveEntityFromExternalTouch(entity);
                entity.AttachableStateChanged -= this.OtherEntity_AttachableStateChanged_RemoveFromExternalTouch;
            }
        }

        private void InternalTouch_BeginCollision(object sender, CollisionInfo info)
        {
            var interactedEntity = RegisterCollidedEntity(this.nearCollisionEntitiesByBody, info);

            this.AddPointerInteraction(interactedEntity);

            this.RunTouchHandlers(interactedEntity, (h, e) =>
            {
                this.nearTouchCollisionEntities.Add(interactedEntity);
                h?.OnTouchStarted(e);
            });
        }

        private void InternalTouch_UpdateCollision(object sender, CollisionInfo info)
        {
            if (this.nearCollisionEntitiesByBody.TryGetValue(info.OtherBody, out var interactedEntity) &&
                this.nearTouchCollisionEntities.Contains(interactedEntity))
            {
                this.RunTouchHandlers(interactedEntity, (h, e) => h?.OnTouchUpdated(e));
            }
        }

        private void InternalTouch_EndCollision(object sender, CollisionInfo info)
        {
            var interactedEntity = UnregisterCollidedEntity(this.nearCollisionEntitiesByBody, info);
            this.RemovePointerInteraction(interactedEntity);

            if (this.nearTouchCollisionEntities.Remove(interactedEntity))
            {
                this.RunTouchHandlers(interactedEntity, (h, e) => h?.OnTouchCompleted(e));
            }
        }

        private void RunTouchHandlers(Entity other, Action<IMixedRealityTouchHandler, HandTrackingInputEventData> action)
        {
            var eventArgs = new HandTrackingInputEventData()
            {
                Cursor = this,
                Position = this.transform.Position,
                PreviousPosition = this.PositionHistory.Count() > 1 ? this.PositionHistory[this.PositionHistory.Count - 2] : Vector3.Zero,
                CurrentTarget = other,
            };

            other.RunOnComponents<IMixedRealityTouchHandler>((x) => action(x, eventArgs));
        }

        private void RemoveEntityFromExternalTouch(Entity entity)
        {
            this.externalTouchCollisionEntities.Remove(entity);
            this.RemoveFocusableInteraction(entity);
        }
    }
}
