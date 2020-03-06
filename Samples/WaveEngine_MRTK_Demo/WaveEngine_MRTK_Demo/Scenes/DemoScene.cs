using WaveEngine.Bullet;
using WaveEngine.Components.XR;
using WaveEngine.Framework;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.XR;
using WaveEngine.Framework.XR.Interaction;
using WaveEngine.Mathematics;
using WaveEngine_MRTK_Demo.Behaviors;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine_MRTK_Demo.Scenes
{
    public class DemoScene : Scene
    {
        public override void RegisterManagers()
        {
            base.RegisterManagers();

            this.Managers.AddManager(new BulletPhysicManager3D() { DrawFlags = WaveEngine.Framework.Physics3D.DebugDrawFlags.All, Gravity = Vector3.Zero });
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
                cursorLeftEntity?.AddComponent(new TrackXRJoint() { Handedness = XRHandedness.LeftHand, SelectionStrategy = TrackXRDevice.SelectionDeviceStrategy.ByHandedness, JointKind = XRHandJointKind.IndexTip });
                cursorRightEntity?.AddComponent(new TrackXRJoint() { Handedness = XRHandedness.RightHand, SelectionStrategy = TrackXRDevice.SelectionDeviceStrategy.ByHandedness, JointKind = XRHandJointKind.IndexTip });
            }
            else
            {
                // Windows
                cursorLeftEntity?.AddComponent(new MouseControlBehavior() { key = WaveEngine.Common.Input.Keyboard.Keys.LeftShift });
                cursorRightEntity?.AddComponent(new MouseControlBehavior() { key = WaveEngine.Common.Input.Keyboard.Keys.Space });
            }
        }

        protected override void Start()
        {
            base.Start();
            var rigidBody = this.Managers.EntityManager.FindFirstComponentOfType<RigidBody3D>();
            rigidBody.Transform3D.PositionChanged += Transform3D_PositionChanged;
        }

        private void Transform3D_PositionChanged(object sender, System.EventArgs e)
        {
        }
    }
}