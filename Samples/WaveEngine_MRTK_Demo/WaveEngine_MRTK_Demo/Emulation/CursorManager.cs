using System;
using System.Linq;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Managers;
using WaveEngine.Framework.Physics3D;
using WaveEngine.MRTK.EventDatum.Input;
using WaveEngine.MRTK.Interfaces.InputSystem.Handlers;

namespace WaveEngine_MRTK_Demo.Emulation
{
    public class CursorManager : SceneManager
    {
        private Cursor[] cursors;

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
        }

        private void StaticBody3D_UpdateCollision(object sender, CollisionInfo3D info)
        {
            this.RunTouchHandlers(info, (h, e) => h?.OnTouchUpdated(e));
        }

        private void StaticBody3D_EndCollision(object sender, CollisionInfo3D info)
        {
            this.RunTouchHandlers(info, (h, e) => h?.OnTouchCompleted(e));
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

            var interactables = info.OtherBody.Owner.Components
                .Select(c => c as IMixedRealityTouchHandler)
                .Where(c => c != null);

            foreach (var touchHandler in interactables)
            {
                action(touchHandler, eventArgs);
            }
        }
    }
}
