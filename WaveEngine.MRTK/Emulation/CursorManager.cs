// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Managers;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;

namespace WaveEngine.MRTK.Emulation
{
    /// <summary>
    /// The Cursor Manager.
    /// </summary>
    public class CursorManager : UpdatableSceneManager
    {
        private static readonly int VELOCITY_HISTORY_SIZE = 10;

        private float historyElapsedTime;
        private List<float> gameTimeHistory = new List<float>(VELOCITY_HISTORY_SIZE);

        private Dictionary<Entity, List<Entity>> cursorCollisions = new Dictionary<Entity, List<Entity>>();
        private Dictionary<Entity, Entity> interactedEntities = new Dictionary<Entity, Entity>();

        private Dictionary<Entity, Cursor> cursorsByEntities = new Dictionary<Entity, Cursor>();
        private Dictionary<Entity, Vector3> cursorsLinearVelocity = new Dictionary<Entity, Vector3>();
        private Dictionary<Entity, Quaternion> cursorsAngularVelocity = new Dictionary<Entity, Quaternion>();

        private Dictionary<Cursor, List<Vector3>> cursorsPositionHistory = new Dictionary<Cursor, List<Vector3>>();
        private Dictionary<Cursor, List<Quaternion>> cursorsOrientationHistory = new Dictionary<Cursor, List<Quaternion>>();

        /// <summary>
        /// Gets the cursors.
        /// </summary>
        public IEnumerable<Cursor> Cursors => this.cursorsByEntities.Values;

        /// <summary>
        /// Adds a cursor.
        /// </summary>
        /// <param name="cursor">The cursor.</param>
        public void AddCursor(Cursor cursor)
        {
            this.cursorsByEntities.Add(cursor.Owner, cursor);

            if (this.IsActivated)
            {
                this.RegisterCursor(cursor);
            }
        }

        /// <summary>
        /// Removes a cursor.
        /// </summary>
        /// <param name="cursor">The cursor.</param>
        public void RemoveCursor(Cursor cursor)
        {
            this.cursorsByEntities.Remove(cursor.Owner);

            if (this.IsActivated)
            {
                this.UnregisterCursor(cursor);
            }
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            foreach (var cursor in this.Cursors)
            {
                this.RegisterCursor(cursor);
            }
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            foreach (var cursor in this.Cursors)
            {
                this.UnregisterCursor(cursor);
            }
        }

        private void RegisterCursor(Cursor cursor)
        {
            if (cursor.IsTouch)
            {
                cursor.StaticBody3D.BeginCollision += this.TouchCursor_BeginCollision;
                cursor.StaticBody3D.UpdateCollision += this.TouchCursor_UpdateCollision;
                cursor.StaticBody3D.EndCollision += this.TouchCursor_EndCollision;
            }

            cursor.StaticBody3D.BeginCollision += this.PointerCursor_BeginCollision;
            cursor.StaticBody3D.EndCollision += this.PointerCursor_EndCollision;

            this.cursorsPositionHistory[cursor] = new List<Vector3>(VELOCITY_HISTORY_SIZE);
            this.cursorsOrientationHistory[cursor] = new List<Quaternion>(VELOCITY_HISTORY_SIZE);
        }

        private void UnregisterCursor(Cursor cursor)
        {
            cursor.StaticBody3D.UpdateCollision -= this.TouchCursor_UpdateCollision;
            cursor.StaticBody3D.EndCollision -= this.TouchCursor_EndCollision;
            cursor.StaticBody3D.BeginCollision -= this.PointerCursor_BeginCollision;
            cursor.StaticBody3D.EndCollision -= this.PointerCursor_EndCollision;
            cursor.StaticBody3D.BeginCollision -= this.TouchCursor_BeginCollision;

            this.cursorsPositionHistory.Remove(cursor);
            this.cursorsOrientationHistory.Remove(cursor);
        }

        private void TouchCursor_BeginCollision(object sender, CollisionInfo3D info)
        {
            var cursorEntity = info.ThisBody.Owner;
            var interactedEntity = info.OtherBody.Owner;

            this.RunTouchHandlers(cursorEntity, interactedEntity, (h, e) =>
            {
                this.cursorsByEntities[cursorEntity].IsTouching = true;
                h?.OnTouchStarted(e);
            });
        }

        private void TouchCursor_UpdateCollision(object sender, CollisionInfo3D info)
        {
            var cursorEntity = info.ThisBody.Owner;
            var interactedEntity = info.OtherBody.Owner;

            this.RunTouchHandlers(cursorEntity, interactedEntity, (h, e) => h?.OnTouchUpdated(e));
        }

        private void TouchCursor_EndCollision(object sender, CollisionInfo3D info)
        {
            var cursorEntity = info.ThisBody.Owner;
            var interactedEntity = info.OtherBody.Owner;

            this.cursorsByEntities[cursorEntity].IsTouching = false;
            this.RunTouchHandlers(cursorEntity, interactedEntity, (h, e) => h?.OnTouchCompleted(e));
        }

        private void PointerCursor_BeginCollision(object sender, CollisionInfo3D info)
        {
            var cursorEntity = info.ThisBody.Owner;
            var interactedEntity = info.OtherBody.Owner;
            if (!this.cursorCollisions.TryGetValue(cursorEntity, out var collisions))
            {
                collisions = new List<Entity>();
                this.cursorCollisions[cursorEntity] = collisions;
            }

            collisions.Insert(0, interactedEntity);
        }

        private void PointerCursor_EndCollision(object sender, CollisionInfo3D info)
        {
            var cursorEntity = info.ThisBody.Owner;
            var interactedEntity = info.OtherBody.Owner;
            if (this.cursorCollisions.TryGetValue(cursorEntity, out var collisions))
            {
                collisions.Remove(interactedEntity);
            }
        }

        /// <inheritdoc/>
        public override void Update(TimeSpan gameTime)
        {
            // Update gameTime history and compute history elapsed time
            var elapsed = (float)gameTime.TotalSeconds;
            this.gameTimeHistory.Add(elapsed);
            this.historyElapsedTime += elapsed;

            if (this.gameTimeHistory.Count > VELOCITY_HISTORY_SIZE)
            {
                this.historyElapsedTime -= this.gameTimeHistory[0];
                this.gameTimeHistory.RemoveAt(0);
            }

            // Update cursors velocities
            foreach (var entry in this.cursorsByEntities)
            {
                var cursorEntity = entry.Key;
                var cursor = entry.Value;
                var positionHistory = this.cursorsPositionHistory[cursor];
                var orientationHistory = this.cursorsOrientationHistory[cursor];

                this.AddToHistoryList(positionHistory, cursor.Transform.Position);
                this.AddToHistoryList(orientationHistory, cursor.Transform.Orientation);

                var linearVelocity = (positionHistory[positionHistory.Count - 1] - positionHistory[0]) / this.historyElapsedTime;
                var angularVelocity = (orientationHistory[orientationHistory.Count - 1] * Quaternion.Inverse(orientationHistory[0])) * (1 / this.historyElapsedTime);

                this.cursorsLinearVelocity[cursorEntity] = linearVelocity;
                this.cursorsAngularVelocity[cursorEntity] = angularVelocity;
            }

            foreach (var entry in this.cursorCollisions)
            {
                if (entry.Value.Count == 0)
                {
                    continue;
                }

                var cursorEntity = entry.Key;
                var interactedEntity = entry.Value[0];

                var cursorComponent = cursorEntity.FindComponent<Cursor>();
                if (!cursorComponent.PreviousPinch && cursorComponent.Pinch)
                {
                    // PointerDown when the cursor transitions to pinched while inside a collider
                    this.RunPointerHandlers(cursorEntity, interactedEntity, (h, e) => h?.OnPointerDown(e));
                    this.interactedEntities[cursorEntity] = interactedEntity;
                }
            }

            var finishedInteractions = new List<Entity>();
            foreach (var entry in this.interactedEntities)
            {
                var cursorEntity = entry.Key;
                var interactedEntity = entry.Value;

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

                        finishedInteractions.Add(cursorEntity);
                    }
                }
            }

            // Remove finished interactions
            for (int i = 0; i < finishedInteractions.Count; i++)
            {
                this.interactedEntities.Remove(finishedInteractions[i]);
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

            this.RunOnComponents<IMixedRealityTouchHandler>(other, (x) => action(x, eventArgs));
        }

        private void RunPointerHandlers(Entity cursor, Entity other, Action<IMixedRealityPointerHandler, MixedRealityPointerEventData> action)
        {
            var transform = cursor.FindComponent<Transform3D>();

            var eventArgs = new MixedRealityPointerEventData()
            {
                Cursor = cursor,
                CurrentTarget = other,
                Position = transform.Position,
                Orientation = transform.Orientation,
                LinearVelocity = this.cursorsLinearVelocity[cursor],
                AngularVelocity = this.cursorsAngularVelocity[cursor],
            };

            this.RunOnComponents<IMixedRealityPointerHandler>(other, (x) => action(x, eventArgs));
        }

        private void RunOnComponents<T>(Entity entity, Action<T> action)
        {
            var current = entity;
            while (current != null)
            {
                foreach (var c in current.Components)
                {
                    if (c.IsActivated && c is T interactable)
                    {
                        action(interactable);
                    }
                }

                current = current.Parent;
            }
        }

        private void AddToHistoryList<T>(List<T> list, T newItem)
        {
            list.Add(newItem);

            if (list.Count > VELOCITY_HISTORY_SIZE)
            {
                list.RemoveAt(0);
            }
        }
    }
}
