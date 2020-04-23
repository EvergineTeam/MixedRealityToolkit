// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Audio;
using WaveEngine.Common.Graphics;
using WaveEngine.Common.Input.Keyboard;
using WaveEngine.Common.Media;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Components.Sound;
using WaveEngine.Components.XR;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Framework.XR;
using WaveEngine.Mathematics;
using WaveEngine_MRTK_Demo.Behaviors;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine.MRTK.SDK.Features
{
    /// <summary>
    /// Some shared Tools.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Gets a Component or adds it if doesn't exist.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="owner">The entity to add the component to.</param>
        /// <returns>The requested component.</returns>
        public static T GetOrAddComponent<T>(this Entity owner)
            where T : Component, new()
        {
            T t = owner.FindComponent<T>();
            if (t == null)
            {
                t = new T();
                owner.AddComponent(t);
            }

            return t;
        }

        /// <summary>
        /// /// Gets a Component in this entity or any of its children or adds it if doesn't exist.
        /// </summary>
        /// <typeparam name="T">The component type.</typeparam>
        /// <param name="owner">The entity to add the component to.</param>
        /// <returns>The requested component.</returns>
        public static T GetInChildrenOrAddComponent<T>(this Entity owner)
            where T : Component, new()
        {
            T t = owner.FindComponentInChildren<T>();
            if (t == null)
            {
                t = new T();
                owner.AddComponent(t);
            }

            return t;
        }

        /// <summary>
        /// Plays a sound using the passed soundEmitter.
        /// </summary>
        /// <param name="soundEmitter">The soundEmitter.</param>
        /// <param name="sound">The sound to play.</param>
        /// <param name="pitch">The pitch.</param>
        public static void PlaySound(SoundEmitter3D soundEmitter, AudioBuffer sound, float pitch = 1.0f)
        {
            if (soundEmitter != null && sound != null)
            {
                if (soundEmitter.PlayState == PlayState.Playing)
                {
                    soundEmitter.Stop();
                }

                soundEmitter.Audio = sound;
                soundEmitter.Pitch = pitch;

                soundEmitter.Play();
            }
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
                .AddComponent(new Transform3D() { Scale = new Vector3(0.010f, 0.010f, 0.010f) })
                .AddComponent(new MaterialComponent() { Material = material })
                .AddComponent(new SphereMesh())
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

            return cursor;
        }

        /// <summary>
        /// Initializes scene for holololens.
        /// </summary>
        /// <param name="scene">Scene to add components to.</param>
        /// <param name="cursorMat">Material for the cursosrs.</param>
        /// <param name="handMat">Material for the hands.</param>
        /// <param name="holographicsEffectId">Id for holographic effect.</param>
        public static void InitHoloScene(Scene scene, Material cursorMat, Material handMat, Guid holographicsEffectId)
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
            SpatialMapping spatialMapping = new SpatialMapping() { GenerateColliders = true/*, Material = assetsService.Load<Material>(WaveContent.Materials.DefaultMaterial)*/ };
            spatialMapping.UpdateInterval = new TimeSpan(0, 0, 1);
            scene.Managers.EntityManager.Add(new Entity("SpatialMapping")
                .AddComponent(new Transform3D())
                .AddComponent(spatialMapping));
        }
    }
}
