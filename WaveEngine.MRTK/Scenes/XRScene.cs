// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Bullet;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Input.Keyboard;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.XR;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.XR;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Behaviors;
using WaveEngine.MRTK.Emulation;
using WaveEngine.MRTK.Services.InputSystem;

namespace WaveEngine.MRTK.Scenes
{
    /// <summary>
    /// Base class for scenes using XR.
    /// </summary>
    public abstract class XRScene : Scene
    {
        /// <summary>
        /// Gets cursors material Guid.
        /// </summary>
        protected abstract Guid CursorMat { get; }

        /// <summary>
        /// Gets holo hands material Guid.
        /// </summary>
        protected abstract Guid HoloHandsMat { get; }

        /// <summary>
        /// Gets spatial mapping material Guid.
        /// </summary>
        protected abstract Guid SpatialMappingMat { get; }

        /// <summary>
        /// Gets holographics effect Guid.
        /// </summary>
        protected abstract Guid HolographicEffect { get; }

        /// <inheritdoc/>
        public override void RegisterManagers()
        {
            base.RegisterManagers();

            this.Managers.AddManager(new BulletPhysicManager3D());
            this.Managers.AddManager(new CursorManager());
        }

        /// <inheritdoc/>
        protected override void CreateScene()
        {
            ////this.Managers.RenderManager.DebugLines = true;

            var assetsService = Application.Current.Container.Resolve<AssetsService>();

            InitHoloScene(
                this,
                assetsService.Load<Material>(this.CursorMat),
                assetsService.Load<Material>(this.HoloHandsMat),
                this.SpatialMappingMat == Guid.Empty ? null : assetsService.Load<Material>(this.SpatialMappingMat),
                this.HolographicEffect);
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            // Create GazeProvider
            Camera3D cam = this.Managers.EntityManager.FindFirstComponentOfType<Camera3D>();
            cam.Owner.AddComponent(new GazeProvider());
        }

        private static void CreateXRHandMesh(Scene scene, Material material, XRHandedness handedness)
        {
            Entity handEntity = new Entity()
                .AddComponent(new Transform3D())
                .AddComponent(new MaterialComponent() { Material = material })
                .AddComponent(new XRDeviceMeshComponent()
                {
                    XRModelName = handedness.ToString(),
                })
                .AddComponent(new MeshRenderer())
                .AddComponent(new TrackModelNode()
                {
                    SelectionStrategy = TrackModelNode.SelectionNodeStrategy.RootNode,
                })
                .AddComponent(new HoloHandsUpdater() { Handedness = handedness })
                ;

            scene.Managers.EntityManager.Add(handEntity);
        }

        private static Entity CreateCursor(Scene scene, Material material, XRHandedness handedness)
        {
            Entity cursor = new Entity()
                .AddComponent(new Transform3D())
                .AddComponent(new MaterialComponent() { Material = material })
                .AddComponent(new SphereMesh() { Diameter = 0.010f })
                .AddComponent(new MeshRenderer())
                .AddComponent(new SphereCollider3D())
                .AddComponent(new StaticBody3D() { CollisionCategories = CollisionCategory3D.Cat1, IsSensor = true })
                .AddComponent(new Cursor() { PressedColor = new Color(255, 173, 128), ReleasedColor = new Color(255, 93, 0), UpdateOrder = 0.3f })
                .AddComponent(new ProximityLight())
                ;

            var xrPlatform = Application.Current.Container.Resolve<XRPlatform>();
            if (xrPlatform != null)
            {
                // HoloLens 2
                cursor.AddComponent(new TrackXRJoint()
                {
                    Handedness = handedness,
                    SelectionStrategy = TrackXRDevice.SelectionDeviceStrategy.ByHandedness,
                    JointKind = XRHandJointKind.IndexTip,
                    TrackingLostMode = TrackXRDevice.XRTrackingLostMode.KeepLastPose,
                })
                    .AddComponent(new HoloLensControlBehavior())
                    ;
            }
            else
            {
                // Windows
                cursor.AddComponent(new MouseControlBehavior() { key = handedness == XRHandedness.RightHand ? Keys.LeftShift : Keys.Space });
            }

            scene.Managers.EntityManager.Add(cursor);

            Entity cursorDist = new Entity()
                .AddComponent(new Transform3D())
                .AddComponent(new MaterialComponent() { Material = material })
                .AddComponent(new SphereMesh() { Diameter = 0.010f })
                .AddComponent(new MeshRenderer())
                .AddComponent(new SphereCollider3D())
                .AddComponent(new StaticBody3D() { CollisionCategories = CollisionCategory3D.Cat1, IsSensor = true })
                .AddComponent(new Cursor() { PressedColor = new Color(255, 173, 128), ReleasedColor = new Color(255, 93, 0), UpdateOrder = 0.3f })
                .AddComponent(new ProximityLight())
                .AddComponent(new CursorRay() { mainCursor = cursor.FindComponent<Cursor>() })
                ;
            scene.Managers.EntityManager.Add(cursorDist);

            return cursor;
        }

        /// <summary>
        /// Initializes scene for holololens.
        /// </summary>
        /// <param name="scene">Scene to add components to.</param>
        /// <param name="cursorMat">Material for the cursosrs.</param>
        /// <param name="handMat">Material for the hands.</param>
        /// <param name="spatialMappingMat">Maerial for the spatial mapping.</param>
        /// <param name="holographicsEffectId">Id for holographic effect.</param>
        public static void InitHoloScene(Scene scene, Material cursorMat, Material handMat, Material spatialMappingMat, Guid holographicsEffectId)
        {
            var assetsService = Application.Current.Container.Resolve<AssetsService>();

            // Create cursors
            CreateCursor(scene, cursorMat, XRHandedness.LeftHand);
            CreateCursor(scene, cursorMat, XRHandedness.RightHand);

            // Create hand meshes
            CreateXRHandMesh(scene, handMat, XRHandedness.LeftHand);
            CreateXRHandMesh(scene, handMat, XRHandedness.RightHand);

            // Create position updater
            scene.Managers.AddManager(new CursorPosShaderUpdater(holographicsEffectId));

            // Create spatial mapping
            SpatialMapping spatialMapping = new SpatialMapping() { GenerateColliders = true, Material = spatialMappingMat };
            spatialMapping.UpdateInterval = new TimeSpan(0, 0, 1);
            scene.Managers.EntityManager.Add(new Entity("SpatialMapping")
                .AddComponent(new Transform3D())
                .AddComponent(spatialMapping));
        }
    }
}
