// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using Evergine.Common.Attributes;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
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
        /// The <see cref="StaticBody3D"/> component dependency.
        /// </summary>
        [BindComponent]
        protected StaticBody3D staticBody3D;

        private Entity externalTouchEntity;
        private StaticBody3D externalTouchStaticBody3D;

        private Dictionary<int, Entity> collisionEntitiesByCollisionId = new Dictionary<int, Entity>();
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
                this.externalTouchStaticBody3D = new StaticBody3D()
                {
                    IsSensor = true,
                    MaskBits = this.staticBody3D.MaskBits,
                    CollisionCategories = this.staticBody3D.CollisionCategories,
                };

                this.externalTouchEntity = new Entity("ExternalTouch")
                                .AddComponent(new Transform3D())
                                .AddComponent(new SphereCollider3D() { Radius = ExternalTouchRadius })
                                .AddComponent(this.externalTouchStaticBody3D);
            }

            this.staticBody3D.BeginCollision += this.InternalTouch_BeginCollision;
            this.staticBody3D.UpdateCollision += this.InternalTouch_UpdateCollision;
            this.staticBody3D.EndCollision += this.InternalTouch_EndCollision;

            this.externalTouchStaticBody3D.BeginCollision += this.ExternalTouch_BeginCollision;
            this.externalTouchStaticBody3D.EndCollision += this.ExternalTouch_EndCollision;
            this.Owner.AddChild(this.externalTouchEntity);
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            foreach (var item in this.nearTouchCollisionEntities)
            {
                this.RunTouchHandlers(item, (h, e) => h?.OnTouchCompleted(e));
            }

            this.staticBody3D.BeginCollision -= this.InternalTouch_BeginCollision;
            this.staticBody3D.UpdateCollision -= this.InternalTouch_UpdateCollision;
            this.staticBody3D.EndCollision -= this.InternalTouch_EndCollision;
            this.nearTouchCollisionEntities.Clear();

            if (this.externalTouchEntity != null)
            {
                this.externalTouchStaticBody3D.BeginCollision -= this.ExternalTouch_BeginCollision;
                this.externalTouchStaticBody3D.EndCollision -= this.ExternalTouch_EndCollision;
                this.Owner.DetachChild(this.externalTouchEntity);
            }

            base.OnDeactivated();
        }

        /// <inheritdoc/>
        public override void Destroy()
        {
            base.Destroy();
            this.externalTouchEntity?.Destroy();
            this.externalTouchEntity = null;
        }

        private Entity RegisterCollidedEntity(CollisionInfo3D info)
        {
            this.UnregisterCollidedEntity(info);

            var otherEntity = info.OtherBody.Owner;
            this.collisionEntitiesByCollisionId.Add(info.Id, otherEntity);
            return otherEntity;
        }

        private Entity UnregisterCollidedEntity(CollisionInfo3D info)
        {
            if (this.collisionEntitiesByCollisionId.TryGetValue(info.Id, out var otherEntity))
            {
                this.collisionEntitiesByCollisionId.Remove(info.Id);
                return otherEntity;
            }

            return null;
        }

        private void ExternalTouch_BeginCollision(object sender, CollisionInfo3D info)
        {
            var otherEntity = this.RegisterCollidedEntity(info);

            var hasHandler = otherEntity.HasEventHandlers<IMixedRealityTouchHandler, IMixedRealityPointerHandler>();
            if (hasHandler)
            {
                this.externalTouchCollisionEntities.Add(otherEntity);
            }

            this.AddFocusableInteraction(otherEntity);
        }

        private void ExternalTouch_EndCollision(object sender, CollisionInfo3D info)
        {
            var otherEntity = this.UnregisterCollidedEntity(info);

            this.externalTouchCollisionEntities.Remove(otherEntity);
            this.RemoveFocusableInteraction(otherEntity);
        }

        private void InternalTouch_BeginCollision(object sender, CollisionInfo3D info)
        {
            var interactedEntity = this.RegisterCollidedEntity(info);

            this.AddPointerInteraction(interactedEntity);

            this.RunTouchHandlers(interactedEntity, (h, e) =>
            {
                this.nearTouchCollisionEntities.Add(interactedEntity);
                h?.OnTouchStarted(e);
            });
        }

        private void InternalTouch_UpdateCollision(object sender, CollisionInfo3D info)
        {
            if (this.collisionEntitiesByCollisionId.TryGetValue(info.Id, out var interactedEntity) &&
                this.nearTouchCollisionEntities.Contains(interactedEntity))
            {
                this.RunTouchHandlers(interactedEntity, (h, e) => h?.OnTouchUpdated(e));
            }
        }

        private void InternalTouch_EndCollision(object sender, CollisionInfo3D info)
        {
            var interactedEntity = this.UnregisterCollidedEntity(info);
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
    }
}
