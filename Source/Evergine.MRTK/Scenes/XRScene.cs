// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Linq;
using Evergine.Bullet;
using Evergine.Common.Graphics;
using Evergine.Common.Input.Keyboard;
using Evergine.Components.Graphics3D;
using Evergine.Components.Primitives;
using Evergine.Components.XR;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Framework.Prefabs;
using Evergine.Framework.XR;
using Evergine.Mathematics;
using Evergine.MRTK.Behaviors;
using Evergine.MRTK.Emulation;
using Evergine.MRTK.SDK.Features;
using Evergine.MRTK.Services.InputSystem;

namespace Evergine.MRTK.Scenes
{
    /// <summary>
    /// Base class for scenes using XR.
    /// </summary>
    public abstract class XRScene : Scene
    {
        private const string BaseCursorTag = "BaseCursor";
        private const string CursorAnchorTag = "CursorAnchor";
        private const float CursorPlaneSize = 0.01f;

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
        /// Gets the prefab for the left controller.
        /// </summary>
        protected abstract Guid LeftControllerModelPrefab { get; }

        /// <summary>
        /// Gets the prefab for the right controller.
        /// </summary>
        protected abstract Guid RightControllerModelPrefab { get; }

        /// <summary>
        /// Gets or sets the <see cref="CollisionCategory3D"/> used by the <see cref="Cursor"/> entities.
        /// Default: <see cref="CollisionCategory3D.Cat2"/>.
        /// </summary>
        public virtual CollisionCategory3D CursorCollisionCategory { get; protected set; } = CollisionCategory3D.Cat2;

        /// <summary>
        /// Gets or sets the <see cref="CollisionCategory3D"/> mask used by the <see cref="Cursor"/> entities.
        /// Default: <see cref="CollisionCategory3D.All"/>.
        /// <para>It indicates with which <see cref="CollisionCategory3D"/> the cursors will collide.</para>
        /// </summary>
        public virtual CollisionCategory3D CursorCollisionCategoryMask { get; protected set; } = CollisionCategory3D.All;

        /// <inheritdoc/>
        public override void RegisterManagers()
        {
            base.RegisterManagers();

            this.Managers.AddManager(new BulletPhysicManager3D()
            {
                PhysicWorldResolution = 10,
            });
            this.Managers.AddManager(new FocusProvider());
            this.Managers.AddManager(new VoiceCommandsProvider());
        }

        /// <summary>
        /// Creates the XR scene.
        /// </summary>
        /// <remarks>
        /// This method is sealed in order to initialize the XR scene with required components and functionality. If you need additional configuration of the
        /// scene, override the <see cref="XRScene.OnPostCreateXRScene"/> method.
        /// </remarks>
        protected sealed override void CreateScene()
        {
            // Disable Z prepass
            var forwardRenderPath = this.Managers.RenderManager.RenderPipeline.DefaultRenderPath as ForwardRenderPath;
            if (forwardRenderPath != null)
            {
                forwardRenderPath.ZPrePassIsEnabled = false;
            }

            // Remove the Skybox entities
            var skyboxEntities = this.Managers.EntityManager.FindAllByTag(tag: "Skybox").ToArray();
            foreach (var entity in skyboxEntities)
            {
                this.Managers.EntityManager.Remove(entity);
            }

            this.InitXRScene();
            this.OnPostCreateXRScene();
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            base.Start();

            // Create GazeProvider
            var cam = this.Managers.EntityManager.FindFirstComponentOfType<Camera3D>();
            cam.Owner.AddComponent(new GazeProvider()
            {
                CollisionCategoryMask = this.CursorCollisionCategoryMask & ~this.CursorCollisionCategory,
            });
        }

        /// <summary>
        /// Initializes the XR scene.
        /// </summary>
        protected void InitXRScene()
        {
            var entityManager = this.Managers.EntityManager;
            var assetsManager = this.Managers.AssetSceneManager;

            // Get materials
            var cursorMatPressed = assetsManager.Load<Material>(this.CursorMatPressed);
            var cursorMatReleased = assetsManager.Load<Material>(this.CursorMatReleased);
            var handRayTexture = assetsManager.Load<Texture>(this.HandRayTexture);
            var handRaySampler = assetsManager.Load<SamplerState>(this.HandRaySampler);

            // Create hand controller entities
            var handMaterial = this.HoloHandsMat != Guid.Empty ? assetsManager.Load<Material>(this.HoloHandsMat) : null;
            var leftHandController = this.CreateXRHandController(XRHandedness.LeftHand, handMaterial);
            var rightHandController = this.CreateXRHandController(XRHandedness.RightHand, handMaterial);

            // Create cursors for hand controllers
            var leftHandControllerNearCursor = this.CreateNearCursor(XRHandedness.LeftHand, cursorMatPressed, cursorMatReleased);
            var rightHandControllerNearCursor = this.CreateNearCursor(XRHandedness.RightHand, cursorMatPressed, cursorMatReleased);
            var leftHandControllerFarCursor = this.CreateFarCursor(XRHandedness.LeftHand, cursorMatPressed, cursorMatReleased, handRayTexture, handRaySampler, leftHandControllerNearCursor);
            var rightHandControllerFarCursor = this.CreateFarCursor(XRHandedness.RightHand, cursorMatPressed, cursorMatReleased, handRayTexture, handRaySampler, rightHandControllerNearCursor);
            this.AddControlComponents(leftHandControllerNearCursor, XRHandedness.LeftHand, isHandTrackingCursor: true);
            this.AddControlComponents(rightHandControllerNearCursor, XRHandedness.RightHand, isHandTrackingCursor: true);

            // Add entities to entity manager
            entityManager.Add(this.CreateControllerHierarchy(leftHandController, leftHandControllerNearCursor, leftHandControllerFarCursor));
            entityManager.Add(this.CreateControllerHierarchy(rightHandController, rightHandControllerNearCursor, rightHandControllerFarCursor));

            // Add controller stuff in XR platforms
            if (Tools.IsXRPlatformInputTrackingAvailable())
            {
                // Create physical controller entities
                var leftPrefab = this.LeftControllerModelPrefab != Guid.Empty ? assetsManager.Load<Prefab>(this.LeftControllerModelPrefab) : null;
                var rightPrefab = this.RightControllerModelPrefab != Guid.Empty ? assetsManager.Load<Prefab>(this.RightControllerModelPrefab) : null;
                var leftPhysicalController = this.CreateXRPhysicalController(XRHandedness.LeftHand, leftPrefab);
                var rightPhysicalController = this.CreateXRPhysicalController(XRHandedness.RightHand, rightPrefab);

                // Create cursors for physical controllers
                var leftPhysicalControllerNearCursor = this.CreateNearCursor(XRHandedness.LeftHand, cursorMatPressed, cursorMatReleased);
                var rightPhysicalControllerNearCursor = this.CreateNearCursor(XRHandedness.RightHand, cursorMatPressed, cursorMatReleased);
                var leftPhysicalControllerFarCursor = this.CreateFarCursor(XRHandedness.LeftHand, cursorMatPressed, cursorMatReleased, handRayTexture, handRaySampler, leftPhysicalControllerNearCursor);
                var rightPhysicalControllerFarCursor = this.CreateFarCursor(XRHandedness.RightHand, cursorMatPressed, cursorMatReleased, handRayTexture, handRaySampler, rightPhysicalControllerNearCursor);
                this.AddControlComponents(leftPhysicalControllerNearCursor, XRHandedness.LeftHand, isHandTrackingCursor: false, leftPhysicalController);
                this.AddControlComponents(rightPhysicalControllerNearCursor, XRHandedness.RightHand, isHandTrackingCursor: false, rightPhysicalController);

                // Add entities to entity manager
                entityManager.Add(this.CreateControllerHierarchy(leftPhysicalController, leftPhysicalControllerNearCursor, leftPhysicalControllerFarCursor));
                entityManager.Add(this.CreateControllerHierarchy(rightPhysicalController, rightPhysicalControllerNearCursor, rightPhysicalControllerFarCursor));
            }

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
            var spatialMappingMat = this.SpatialMappingMat == Guid.Empty ? null : assetsManager.Load<Material>(this.SpatialMappingMat);
            entityManager.Add(new Entity(nameof(SpatialMapping))
                         .AddComponent(new SpatialMapping()
                         {
                             GenerateColliders = true,
                             UpdateInterval = TimeSpan.FromSeconds(4),
                             Material = spatialMappingMat,
                         }));
        }

        /// <summary>
        /// This method is called after the main XR scene creation in order for the inheriting class to perform additional changes to the scene, as one would with
        /// the <see cref="Scene.CreateScene"/> method.
        /// </summary>
        protected virtual void OnPostCreateXRScene()
        {
        }

        private Entity CreateXRHandController(XRHandedness handedness, Material material)
        {
            var handEntity = new Entity($"handController_{handedness}")
            {
                IsEnabled = false,
            }
            .AddComponent(new Transform3D());

            if (material != null)
            {
                handEntity
                    .AddComponent(new MaterialComponent()
                    {
                        Material = material,
                    })
                    .AddComponent(new TrackXRArticulatedHand()
                    {
                        Handedness = handedness,
                    })
                    .AddComponent(new XRDeviceRenderableModel())
                ////.AddComponent(new HoloHandsUpdater() { Handedness = handedness })
                ;
            }

            return handEntity;
        }

        private Entity CreateXRPhysicalController(XRHandedness handedness, Prefab prefab)
        {
            var controllerEntity = new Entity($"physicalController_{handedness}")
            {
                IsEnabled = false,
            }
            .AddComponent(new Transform3D())
            .AddComponent(new TrackXRController()
            {
                Handedness = handedness,
            })
            ;

            if (prefab != null)
            {
                var instance = prefab.Instantiate();

                controllerEntity.AddChild(instance);
            }

            return controllerEntity;
        }

        private Entity CreateNearCursor(XRHandedness handedness, Material pressedMaterial, Material releasedMaterial)
        {
            var nearCursor = new Entity()
            {
                Name = $"{nameof(CursorTouch)}_{handedness}",
                Tag = BaseCursorTag,
            }
            .AddComponent(new Transform3D())
            .AddComponent(new CursorTouch()
            {
                PressedMaterial = pressedMaterial,
                ReleasedMaterial = releasedMaterial,
            })
            .AddComponent(new SphereCollider3D() { Radius = 0.005f })
            .AddComponent(new StaticBody3D()
            {
                IsSensor = true,
                CollisionCategories = this.CursorCollisionCategory,
                MaskBits = this.CursorCollisionCategoryMask & ~this.CursorCollisionCategory,
            })
            .AddChild(new Entity("visual")
                .AddComponent(new Transform3D())
                .AddComponent(new MaterialComponent())
                .AddComponent(new PlaneMesh()
                {
                    TwoSides = true,
                    PlaneNormal = PlaneMesh.NormalAxis.ZNegative,
                    Width = CursorPlaneSize,
                    Height = CursorPlaneSize,
                })
                .AddComponent(new MeshRenderer())
                .AddComponent(new ProximityLight()));

            var rootCursor = new Entity()
                .AddComponent(new Transform3D())
                .AddChild(nearCursor);

            return rootCursor;
        }

        private Entity CreateFarCursor(XRHandedness handedness, Material pressedMaterial, Material releasedMaterial, Texture handRayTexture, SamplerState handRaySampler, Entity nearCursor)
        {
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
                    TouchCursorEntity = nearCursor,
                })
                .AddChild(ray)
                .AddChild(new Entity("visual")
                    .AddComponent(new Transform3D())
                    .AddComponent(new MaterialComponent())
                    .AddComponent(new PlaneMesh()
                    {
                        PlaneNormal = PlaneMesh.NormalAxis.ZNegative,
                        Width = CursorPlaneSize,
                        Height = CursorPlaneSize,
                    })
                    .AddComponent(new MeshRenderer())
                    .AddComponent(new CameraDistanceScale() { UpdateOrder = 1 })
                    .AddComponent(new HoverLight()));

            return farCursor;
        }

        private Entity CreateControllerHierarchy(Entity controller, Entity nearCursor, Entity farCursor)
        {
            return new Entity()
                .AddComponent(new Transform3D())
                .AddChild(controller)
                .AddChild(nearCursor)
                .AddChild(farCursor);
        }

        private void AddControlComponents(Entity entity, XRHandedness handedness, bool isHandTrackingCursor, Entity physicalController = null)
        {
            if (Tools.IsXRPlatformInputTrackingAvailable())
            {
                // XR platforms
                if (isHandTrackingCursor)
                {
                    entity
                        .AddComponent(new TrackXRJoint()
                        {
                            Handedness = handedness,
                            JointKind = XRHandJointKind.IndexTip,
                            TrackingLostMode = TrackXRDevice.XRTrackingLostMode.KeepLastPose,
                        })
                        .AddComponent(new HandTrackingControlBehavior());
                }
                else
                {
                    entity
                        .AddComponent(new TrackXRController()
                        {
                            Handedness = handedness,
                        })
                        .AddComponent(new PhysicalControllerControlBehavior());

                    if (physicalController != null)
                    {
                        var cursorAnchorTransform = physicalController.FindComponentInChildren<Transform3D>(isRecursive: true, tag: CursorAnchorTag);

                        if (cursorAnchorTransform != null)
                        {
                            var transform3D = entity.FindComponentInChildren<Transform3D>(tag: BaseCursorTag, isRecursive: true);
                            transform3D.LocalTransform = cursorAnchorTransform.LocalTransform;
                        }
                    }
                }
            }
            else
            {
                // Windows
                var key = handedness == XRHandedness.RightHand ? Keys.LeftShift : Keys.Space;
                entity.AddComponent(new MouseControlBehavior() { Key = key });
            }
        }
    }
}
