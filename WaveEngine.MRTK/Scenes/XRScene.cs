// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using WaveEngine.Bullet;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Input.Keyboard;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.Primitives;
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
        /// Gets cursors material Guid for released state.
        /// </summary>
        protected abstract Guid CursorMatReleased { get; }

        /// <summary>
        /// Gets cursors material Guid for pressed state.
        /// </summary>
        protected abstract Guid CursorMatPressed { get; }

        /// <summary>
        /// Gets holo hands material Guid.
        /// </summary>
        protected abstract Guid HoloHandsMat { get; }

        /// <summary>
        /// Gets spatial mapping material Guid.
        /// </summary>
        protected abstract Guid SpatialMappingMat { get; }

        /// <summary>
        /// Gets the texture for the hand rays.
        /// </summary>
        protected abstract Guid HandRayTexture { get; }

        /// <summary>
        /// Gets the Sampler for the hand rays.
        /// </summary>
        protected abstract Guid HandRaySampler { get; }

        /// <summary>
        /// Gets or sets the <see cref="CollisionCategory3D"/> used by the <see cref="Cursor"/> entities.
        /// </summary>
        protected virtual CollisionCategory3D CursorCollisionCategory { get; set; } = CollisionCategory3D.Cat2;

        /// <inheritdoc/>
        public override void RegisterManagers()
        {
            base.RegisterManagers();

            this.Managers.AddManager(new BulletPhysicManager3D());
        }

        /// <inheritdoc/>
        protected override void CreateScene()
        {
            var forwardRenderPass = this.Managers.RenderManager.RenderPipeline.DefaultRenderPath as ForwardRenderPath;
            if (forwardRenderPass != null)
            {
                forwardRenderPass.ZPrePassIsEnabled = false;
            }

            var skyboxEntities = this.Managers.EntityManager.FindAllByTag(tag: "Skybox").ToArray();
            foreach (var entity in skyboxEntities)
            {
                this.Managers.EntityManager.Remove(entity);
            }

            var assetsService = Application.Current.Container.Resolve<AssetsService>();

            InitHoloScene(
                this,
                this.CursorCollisionCategory,
                assetsService.Load<Material>(this.CursorMatReleased),
                assetsService.Load<Material>(this.CursorMatPressed),
                this.HoloHandsMat == Guid.Empty ? null : assetsService.Load<Material>(this.HoloHandsMat),
                this.SpatialMappingMat == Guid.Empty ? null : assetsService.Load<Material>(this.SpatialMappingMat),
                assetsService.Load<Texture>(this.HandRayTexture),
                assetsService.Load<SamplerState>(this.HandRaySampler));
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            // Create GazeProvider
            Camera3D cam = this.Managers.EntityManager.FindFirstComponentOfType<Camera3D>();
            cam.Owner.AddComponent(new GazeProvider() { CollisionCategoryMask = CollisionCategory3D.All & ~this.CursorCollisionCategory });
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

        private static Entity CreateCursor(Scene scene, CollisionCategory3D cursorCollisionCategory, Material releasedMaterial, Material pressedMaterial, XRHandedness handedness, Texture handRayTexture, SamplerState handRaySampler)
        {
            var mainCursor = new Entity($"{nameof(CursorTouch)}_{handedness}")
                .AddComponent(new Transform3D())
                .AddComponent(new CursorTouch() { PressedMaterial = pressedMaterial, ReleasedMaterial = releasedMaterial })
                .AddComponent(new SphereCollider3D() { Radius = 0.005f })
                .AddComponent(new StaticBody3D() { CollisionCategories = cursorCollisionCategory, IsSensor = true, MaskBits = CollisionCategory3D.All & ~cursorCollisionCategory })
                .AddChild(new Entity("visual")
                    .AddComponent(new Transform3D())
                    .AddComponent(new MaterialComponent())
                    .AddComponent(new PlaneMesh() { TwoSides = true, PlaneNormal = PlaneMesh.NormalAxis.ZNegative, Width = 0.01f, Height = 0.01f })
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new ProximityLight()));

            if (Application.Current.Container.IsRegistered<XRPlatform>())
            {
                // HoloLens 2
                mainCursor.AddComponent(new TrackXRJoint()
                {
                    Handedness = handedness,
                    SelectionStrategy = TrackXRDevice.SelectionDeviceStrategy.ByHandedness,
                    JointKind = XRHandJointKind.IndexTip,
                    TrackingLostMode = TrackXRDevice.XRTrackingLostMode.KeepLastPose,
                })
                          .AddComponent(new HoloLensControlBehavior());
            }
            else
            {
                // Windows
                var key = handedness == XRHandedness.RightHand ? Keys.LeftShift : Keys.Space;
                mainCursor.AddComponent(new MouseControlBehavior() { Key = key });
            }

            var ray = new Entity()
                .AddComponent(new Transform3D())
                .AddComponent(new LineMesh()
                {
                    IsCameraAligned = true,
                    LinePoints = new List<LinePointInfo>()
                        {
                            new LinePointInfo() { Position = Vector3.Zero, Thickness = 0.003f, Color = Color.White },
                            new LinePointInfo() { Position = -Vector3.UnitZ, Thickness = 0.003f, Color = Color.White },
                        },
                    DiffuseTexture = handRayTexture,
                    DiffuseSampler = handRaySampler,
                })
                .AddComponent(new LineMeshRenderer3D());

            var farCursor = new Entity($"{nameof(CursorRay)}_{handedness}")
                .AddComponent(new Transform3D())
                .AddComponent(new CursorRay()
                {
                    PressedMaterial = pressedMaterial,
                    ReleasedMaterial = releasedMaterial,
                    TouchCursorEntity = mainCursor,
                })
                .AddChild(ray)
                .AddChild(new Entity("visual")
                    .AddComponent(new Transform3D())
                    .AddComponent(new MaterialComponent())
                    .AddComponent(new PlaneMesh() { PlaneNormal = PlaneMesh.NormalAxis.ZNegative, Width = 0.01f, Height = 0.01f })
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new CameraDistanceScale() { UpdateOrder = 1 })
                    .AddComponent(new HoverLight()));

            var entityManager = scene.Managers.EntityManager;
            entityManager.Add(mainCursor);
            entityManager.Add(farCursor);

            return mainCursor;
        }

        /// <summary>
        /// Initializes scene for HoloLens.
        /// </summary>
        /// <param name="scene">Scene to add components to.</param>
        /// <param name="cursorCollisionCategory">The <see cref="CollisionCategory3D"/> used by the <see cref="Cursor"/> entities.</param>
        /// <param name="cursorMatReleased">Material for the cursor when it's released.</param>
        /// <param name="cursorMatPressed">Material for the cursor when it's pressed.</param>
        /// <param name="handMat">Material for the hands.</param>
        /// <param name="spatialMappingMat">Material for the spatial mapping.</param>
        /// <param name="handRayTexture">Texture for handrays.</param>
        /// <param name="handRaySampler">Sampler for the handrays texture.</param>
        public static void InitHoloScene(Scene scene, CollisionCategory3D cursorCollisionCategory, Material cursorMatReleased, Material cursorMatPressed, Material handMat, Material spatialMappingMat, Texture handRayTexture, SamplerState handRaySampler)
        {
            // Create cursors
            CreateCursor(scene, cursorCollisionCategory, cursorMatReleased, cursorMatPressed, XRHandedness.LeftHand, handRayTexture, handRaySampler);
            CreateCursor(scene, cursorCollisionCategory, cursorMatReleased, cursorMatPressed, XRHandedness.RightHand, handRayTexture, handRaySampler);

            // Create hand meshes
            if (handMat != null)
            {
                CreateXRHandMesh(scene, handMat, XRHandedness.LeftHand);
                CreateXRHandMesh(scene, handMat, XRHandedness.RightHand);
            }

            var entityManager = scene.Managers.EntityManager;

            // Create cursor position updater
            if (entityManager.FindFirstComponentOfType<CursorPosShaderUpdater>() == null)
            {
                entityManager.Add(new Entity(nameof(CursorPosShaderUpdater))
                    .AddComponent(new CursorPosShaderUpdater()));
            }

            // Create Holographic batching disabler
            if (entityManager.FindFirstComponentOfType<HolographicBatching>() == null)
            {
                entityManager.Add(new Entity(nameof(HolographicBatching))
                    .AddComponent(new HolographicBatching()));
            }

            // Create spatial mapping
            entityManager.Add(new Entity(nameof(SpatialMapping))
                         .AddComponent(new SpatialMapping()
                         {
                             GenerateColliders = true,
                             UpdateInterval = TimeSpan.FromSeconds(4),
                             Material = spatialMappingMat,
                         }));
        }
    }
}
