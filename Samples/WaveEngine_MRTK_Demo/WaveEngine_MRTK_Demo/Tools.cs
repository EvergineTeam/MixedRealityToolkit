using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
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

namespace WaveEngine_MRTK_Demo
{
    public static class Tools
    {
        public static T GetOrAddComponent<T>(this Entity Owner) where T : Component, new()
        {
            T t = Owner.FindComponent<T>();
            if (t == null)
            {
                t = new T();
                Owner.AddComponent(t);
            }
            return t;
        }

        public static T GetInChildrenOrAddComponent<T>(this Entity Owner) where T : Component, new()
        {
            T t = Owner.FindComponentInChildren<T>();
            if (t == null)
            {
                t = new T();
                Owner.AddComponent(t);
            }
            return t;
        }

        public static unsafe Material Clone(this Material material)
        {
            Material copy = new Material(material.Effect);
            copy.ActiveDirectivesNames = (string[])material.ActiveDirectivesNames.Clone();
            copy.LayerDescription = material.LayerDescription;
            copy.OrderBias = material.OrderBias;
            copy.AllowInstancing = material.AllowInstancing;

            for (int c = 0; c < material.CBuffers.Length; c++)
            {
                void* copyData = (void*)copy.CBuffers[c].Data;
                void* data = (void*)material.CBuffers[c].Data;
                uint size = material.CBuffers[c].Size;

                Unsafe.CopyBlock(copyData, data, size);
                copy.CBuffers[c].Dirty = true;
            }

            for (int t = 0; t < material.TextureSlots.Length; t++)
            {
                copy.TextureSlots[t] = material.TextureSlots[t];
            }

            for (int s = 0; s < material.SamplerSlots.Length; s++)
            {
                copy.SamplerSlots[s] = material.SamplerSlots[s];
            }

            return copy;
        }

        public static void PlaySound(SoundEmitter3D soundEmitter,  AudioBuffer sound)
        {
            if (soundEmitter != null && sound != null)
            {
                if (soundEmitter.PlayState == PlayState.Playing)
                {
                    soundEmitter.Stop();
                }

                soundEmitter.Audio = sound;

                soundEmitter.Play();
            }
        }

        private static void CreateXRHandMesh(Scene scene, AssetsService assetsService, XRHandedness handedness)
        {
            var material = assetsService.Load<Material>(WaveContent.Materials.HoloHands);

            Entity handEntity = new Entity()
                .AddComponent(new Transform3D())
                .AddComponent(new MaterialComponent() { Material = material })
                .AddComponent(new XRDeviceMeshComponent()
                {
                    XRModelName = handedness.ToString()
                })
                .AddComponent(new MeshRenderer())
                .AddComponent(new TrackModelNode()
                {
                    SelectionStrategy = TrackModelNode.SelectionNodeStrategy.RootNode
                })
                .AddComponent(new HoloHandsUpdater() { Handedness = handedness })
                ;

            scene.Managers.EntityManager.Add(handEntity);
        }

        private static Entity CreateCursor(Scene scene, AssetsService assetsService, XRHandedness handedness)
        {
            var material = assetsService.Load<Material>(WaveContent.Materials.CursorLeft);

            Entity cursor = new Entity()
                .AddComponent(new Transform3D() { Scale = new Vector3(0.010f, 0.010f, 0.010f) })
                .AddComponent(new MaterialComponent() { Material = material })
                .AddComponent(new SphereMesh())
                .AddComponent(new MeshRenderer())
                .AddComponent(new SphereCollider3D())
                .AddComponent(new StaticBody3D() { CollisionCategories = CollisionCategory3D.Cat1, IsSensor = true })
                .AddComponent(new Cursor() { PressedColor = new Color(255, 173, 128), ReleasedColor = new Color(255, 93, 0), UpdateOrder = 0.3f })
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

        public static void CreateHands(Scene scene)
        {
            var assetsService = Application.Current.Container.Resolve<AssetsService>();

            CreateCursor(scene, assetsService, XRHandedness.LeftHand);
            CreateCursor(scene, assetsService, XRHandedness.RightHand);

            CreateXRHandMesh(scene, assetsService, XRHandedness.LeftHand);
            CreateXRHandMesh(scene, assetsService, XRHandedness.RightHand);
        }
    }
}
