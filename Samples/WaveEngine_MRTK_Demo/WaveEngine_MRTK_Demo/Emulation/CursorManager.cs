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
    public class CursorManager : SceneManager
    {
        public Cursor[] cursors { private set; get; }

        protected override void OnActivated()
        {
            base.OnActivated();

            this.cursors = this.Managers.EntityManager
                .FindComponentsOfType<Cursor>()
                .ToArray()
               ;

            foreach (var cursor in this.cursors)
            {
                cursor.StaticBody3D.BeginCollision += this.StaticBody3D_BeginCollision;
                cursor.StaticBody3D.UpdateCollision += this.StaticBody3D_UpdateCollision;
                cursor.StaticBody3D.EndCollision += this.StaticBody3D_EndCollision;
            }
        }

        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            foreach (var cursor in this.cursors)
            {
                cursor.StaticBody3D.BeginCollision -= this.StaticBody3D_BeginCollision;
                cursor.StaticBody3D.UpdateCollision -= this.StaticBody3D_UpdateCollision;
                cursor.StaticBody3D.EndCollision -= this.StaticBody3D_EndCollision;
            }
        }

        private void StaticBody3D_BeginCollision(object sender, CollisionInfo3D info)
        {
            this.RunTouchHandlers(info, (h, e) => h?.OnTouchStarted(e));
            this.DoPointerEmulation(info, true, false);
        }

        private void StaticBody3D_UpdateCollision(object sender, CollisionInfo3D info)
        {
            this.RunTouchHandlers(info, (h, e) => h?.OnTouchUpdated(e));
            this.DoPointerEmulation(info, false, false);
        }

        private void StaticBody3D_EndCollision(object sender, CollisionInfo3D info)
        {
            this.RunTouchHandlers(info, (h, e) => h?.OnTouchCompleted(e));
            this.DoPointerEmulation(info, false, true);
        }

        private void RunTouchHandlers(CollisionInfo3D info, Action<IMixedRealityTouchHandler, HandTrackingInputEventData> action)
        {
            var cursorEntity = info.ThisBody.Owner;
            var position = cursorEntity.FindComponent<Transform3D>().Position;

            var eventArgs = new HandTrackingInputEventData()
            {
                Cursor = cursorEntity,
                Position = position,
            };

            var interactables = this.GatherComponents(info.OtherBody.Owner)
                .Select(c => c as IMixedRealityTouchHandler)
                .Where(c => c != null);

            foreach (var touchHandler in interactables)
            {
                action(touchHandler, eventArgs);
            }
        }

        private void RunPointerHandlers(CollisionInfo3D info, Action<IMixedRealityPointerHandler, MixedRealityPointerEventData> action)
        {
            var cursorEntity = info.ThisBody.Owner;
            var transform = cursorEntity.FindComponent<Transform3D>();

            var eventArgs = new MixedRealityPointerEventData()
            {
                Cursor = cursorEntity,
                Position = transform.Position,
                Orientation = transform.Orientation
            };

            var interactables = this.GatherComponents(info.OtherBody.Owner)
                .Select(c => c as IMixedRealityPointerHandler)
                .Where(c => c != null);

            foreach (var touchHandler in interactables)
            {
                action(touchHandler, eventArgs);
            }
        }

        private void DoPointerEmulation(CollisionInfo3D info, bool first, bool last)
        {
            var cursorComponent = info.ThisBody.Owner.FindComponent<Cursor>();

            if (cursorComponent.Pinch != cursorComponent.PreviousPinch)
            {
                if (cursorComponent.Pinch)
                {
                    this.RunPointerHandlers(info, (h, e) => h?.OnPointerDown(e));
                }
                else
                {
                    this.RunPointerHandlers(info, (h, e) => h?.OnPointerUp(e));
                }
            }
            else if (cursorComponent.Pinch)
            {
                if (first)
                {
                    this.RunPointerHandlers(info, (h, e) => h?.OnPointerDown(e));
                }
                else if (last)
                {
                    this.RunPointerHandlers(info, (h, e) => h?.OnPointerUp(e));
                }
                else
                {
                    this.RunPointerHandlers(info, (h, e) => h?.OnPointerDragged(e));
                }
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
