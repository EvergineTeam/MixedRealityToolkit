using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Managers;
using WaveEngine.Framework.Physics3D;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;

namespace WaveEngine_MRTK_Demo.Emulation
{
    public class CursorManager : UpdatableSceneManager
    {
        public Cursor[] cursors;

        private Dictionary<Entity, Entity> cursorCollisions = new Dictionary<Entity, Entity>();
        private Dictionary<Entity, Entity> interactedEntities = new Dictionary<Entity, Entity>();

        protected override void OnActivated()
        {
            base.OnActivated();

            this.cursors = this.Managers.EntityManager
                .FindComponentsOfType<Cursor>()
                .ToArray()
               ;

            foreach (var cursor in this.cursors)
            {
                cursor.StaticBody3D.BeginCollision += this.Cursor_BeginCollision;
                cursor.StaticBody3D.UpdateCollision += this.Cursor_UpdateCollision;
                cursor.StaticBody3D.EndCollision += this.Cursor_EndCollision;
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            foreach (var cursor in this.cursors)
            {
                cursor.StaticBody3D.BeginCollision -= this.Cursor_BeginCollision;
                cursor.StaticBody3D.UpdateCollision -= this.Cursor_UpdateCollision;
                cursor.StaticBody3D.EndCollision -= this.Cursor_EndCollision;
            }
        }

        private void Cursor_BeginCollision(object sender, CollisionInfo3D info)
        {
            var cursorEntity = info.ThisBody.Owner;
            var interactedEntity = info.OtherBody.Owner;

            this.RunTouchHandlers(cursorEntity, interactedEntity, (h, e) => h?.OnTouchStarted(e));

            if (!this.cursorCollisions.ContainsKey(cursorEntity))
            {
                this.cursorCollisions[cursorEntity] = interactedEntity;
            }
        }

        private void Cursor_UpdateCollision(object sender, CollisionInfo3D info)
        {
            var cursorEntity = info.ThisBody.Owner;
            var interactedEntity = info.OtherBody.Owner;

            this.RunTouchHandlers(cursorEntity, interactedEntity, (h, e) => h?.OnTouchUpdated(e));
        }

        private void Cursor_EndCollision(object sender, CollisionInfo3D info)
        {
            var cursorEntity = info.ThisBody.Owner;
            var interactedEntity = info.OtherBody.Owner;

            this.RunTouchHandlers(cursorEntity, interactedEntity, (h, e) => h?.OnTouchCompleted(e));

            this.cursorCollisions.Remove(cursorEntity);
        }

        public override void Update(TimeSpan gameTime)
        {
            foreach (KeyValuePair<Entity, Entity> entry in this.cursorCollisions)
            {
                var cursorEntity = entry.Key;
                var interactedEntity = entry.Value;

                var cursorComponent = cursorEntity.FindComponent<Cursor>();

                if (!cursorComponent.PreviousPinch && cursorComponent.Pinch)
                {
                    // PointerDown when the cursor transitions to pinched while inside a collider
                    this.RunPointerHandlers(cursorEntity, interactedEntity, (h, e) => h?.OnPointerDown(e));
                    this.interactedEntities[cursorEntity] = interactedEntity;
                }
            }

            LinkedList<Entity> finishedInteractions = new LinkedList<Entity>();

            var cursors = this.interactedEntities.Keys.ToArray();
            foreach (Entity cursorEntity in cursors)
            {
                var interactedEntity = this.interactedEntities[cursorEntity];

                var cursorComponent = cursorEntity.FindComponent<Cursor>();

                if (cursorComponent.PreviousPinch)
                {
                    if (cursorComponent.Pinch)
                    {
                        // PointerDragged while the cursor is pinched
                        this.RunPointerHandlers(cursorEntity, interactedEntity, (h, e) => h?.OnPointerDragged(e));
                    }
                    else
                    {
                        // PointerUp when the cursor is unpinched
                        this.RunPointerHandlers(cursorEntity, interactedEntity, (h, e) => h?.OnPointerUp(e));

                        finishedInteractions.AddLast(cursorEntity);
                    }
                }
            }

            // Remove finished interactions
            foreach (Entity cursor in finishedInteractions)
            {
                this.interactedEntities.Remove(cursor);
            }
        }

        private void RunTouchHandlers(Entity cursor, Entity other, Action<IMixedRealityTouchHandler, HandTrackingInputEventData> action)
        {
            var position = cursor.FindComponent<Transform3D>().Position;

            var eventArgs = new HandTrackingInputEventData()
            {
                Cursor = cursor,
                Position = position,
            };

            var interactables = this.GatherComponents(other)
                .Select(c => c as IMixedRealityTouchHandler)
                .Where(c => c != null);

            foreach (var touchHandler in interactables)
            {
                action(touchHandler, eventArgs);
            }
        }

        private void RunPointerHandlers(Entity cursor, Entity other, Action<IMixedRealityPointerHandler, MixedRealityPointerEventData> action)
        {
            var transform = cursor.FindComponent<Transform3D>();

            var eventArgs = new MixedRealityPointerEventData()
            {
                Cursor = cursor,
                Position = transform.Position,
                Orientation = transform.Orientation
            };

            var interactables = this.GatherComponents(other)
                .Select(c => c as IMixedRealityPointerHandler)
                .Where(c => c != null);

            foreach (var touchHandler in interactables)
            {
                action(touchHandler, eventArgs);
            }
        }

        private IEnumerable<Component> GatherComponents(Entity entity)
        {
            List<Component> result = new List<Component>();

            Entity current = entity;

            while (current != null)
            {
                result.AddRange(current.Components);
                current = current.Parent;
            }

            return result;
        }
    }
}
