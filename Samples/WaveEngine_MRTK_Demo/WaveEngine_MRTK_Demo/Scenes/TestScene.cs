using WaveEngine.Bullet;
using WaveEngine.Components.XR;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.XR;
using WaveEngine.Framework.XR.Interaction;
using WaveEngine_MRTK_Demo.Behaviors;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine_MRTK_Demo.Scenes
{
    public class TestScene : Scene
    {
        public override void RegisterManagers()
        {
            base.RegisterManagers();

            this.Managers.AddManager(new BulletPhysicManager3D());
            this.Managers.AddManager(new CursorManager());
        }

        protected override void CreateScene()
        {
            //this.Managers.RenderManager.DebugLines = true;

            var xrPlatform = Application.Current.Container.Resolve<XRPlatform>();

            var cursorLeftEntity = this.Managers.EntityManager.Find("cursors.left");
            var cursorRightEntity = this.Managers.EntityManager.Find("cursors.right");

            if (xrPlatform != null)
            {
                // HoloLens 2
                cursorLeftEntity.AddComponent(new TrackXRJoint() { Handedness = XRHandedness.LeftHand, SelectionStrategy = TrackXRDevice.SelectionDeviceStrategy.ByHandedness, JointKind = XRHandJointKind.IndexTip });
                cursorRightEntity.AddComponent(new TrackXRJoint() { Handedness = XRHandedness.RightHand, SelectionStrategy = TrackXRDevice.SelectionDeviceStrategy.ByHandedness, JointKind = XRHandJointKind.IndexTip });
            }
            else
            {
                // Windows
                cursorLeftEntity.AddComponent(new KeyboardControlBehavior() { Speed = 0.05f, UseShift = true });
                cursorRightEntity.AddComponent(new KeyboardControlBehavior() { Speed = 0.05f, UseShift = false });
            }
        }
    }
}
