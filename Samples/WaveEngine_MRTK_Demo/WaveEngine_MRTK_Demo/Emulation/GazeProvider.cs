// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons;
using WaveEngine_MRTK_Demo;
using WaveEngine_MRTK_Demo.Emulation;

namespace WaveEngine.MRTK.Services.InputSystem
{
    /// <summary>
    /// The Gaze provider.
    /// </summary>
    public class GazeProvider : Behavior
    {
        /// <summary>
        /// The transform.
        /// </summary>
        [BindComponent]
        public Transform3D transform = null;

        /// <summary>
        /// The camera.
        /// </summary>
        [BindComponent]
        public Camera3D camera;

        /// <summary>
        /// Gets or sets the max distance to capture.
        /// </summary>
        public float Distance { get; set; } = 1000.0f;

        /// <summary>
        /// Gets or sets the collision category bits.
        /// </summary>
        public CollisionCategory3D Mask { get; set; } = CollisionCategory3D.All;

        private XRPlatform xrPlatform;

        /// <summary>
        /// Gets the target aimed by the gaze or null.
        /// </summary>
        public Entity GazeTarget { 
            get
            {
                return _gazeTarget;
            }
            private set
            {
                if (this._gazeTarget != null)
                {
                    IFocusable focusable = FindComponent<IFocusable>(this._gazeTarget);
                    focusable?.OnFocusExit();
                }

                _gazeTarget = value;

                if (_gazeTarget != null)
                {
                    IFocusable focusable = FindComponent<IFocusable>(this._gazeTarget);
                    focusable?.OnFocusEnter();
                }
            }
        }

        private Transform3D hoverLight;
        private Entity _gazeTarget;

        T FindComponent<T>(Entity entity) where T : class
        {
            foreach (Component c in entity.Components)
            {
                if (c is T)
                    return c as T;
            }
            return null;
        }

        protected override void Start()
        {
            var assetsService = Application.Current.Container.Resolve<AssetsService>();

            if (!Application.Current.IsEditor)
            {
                Entity hoverLight = new Entity("GazePointer")
                .AddComponent(new Transform3D() { Scale = Vector3.One * 0.01f })
                .AddComponent(new HoverLight())
                //.AddComponent(new MaterialComponent() { Material = assetsService.Load<Material>(WaveContent.Materials.DefaultMaterial) })
                //.AddComponent(new SphereMesh())
                //.AddComponent(new MeshRenderer())
                ;
                this.hoverLight = hoverLight.FindComponent<Transform3D>();
                this.Managers.EntityManager.Add(hoverLight);

                xrPlatform = Application.Current.Container.Resolve<XRPlatform>();
                if (xrPlatform != null)
                {
                    IVoiceCommandService voiceCommandService = Application.Current.Container.Resolve<IVoiceCommandService>();
                    if(voiceCommandService != null)
                    {
                        voiceCommandService.CommandRecognized += VoiceCommandService_CommandRecognized;
                    }
                }
            }
        }

        private void VoiceCommandService_CommandRecognized(object sender, string e)
        {
            if (GazeTarget != null)
            {
                FindComponent<ISpeechHandler>(GazeTarget)?.OnSpeechKeywordRecognized(e);
            }
        }

        /// <summary>
        /// Update.
        /// </summary>
        /// <param name="gameTime">The time.</param>
        protected override void Update(TimeSpan gameTime)
        {
            Ray? ray = null;
            if (xrPlatform != null)
            {
                ray = xrPlatform.EyeGaze;
                if (ray == null)
                {
                    ray = xrPlatform.HeadGaze;
                }
            }
            else
            {
                ray = new Ray(this.transform.Position, this.transform.LocalTransform.Forward);
            }

            if (ray != null)
            {
                Ray r = ray.Value;
                HitResult3D result = this.Managers.PhysicManager3D.RayCast(ref r, this.Distance, this.Mask);
                Entity hitEntity = (result.Succeeded) ? ((PhysicBody3D)result.PhysicBody.UserData).Owner : null;
                if (hitEntity != this.GazeTarget)
                {
                    this.GazeTarget = hitEntity;
                }

                // Update Hover light
                if (result.Succeeded)
                {
                    this.hoverLight.Position = result.Point;
                }
            } 
            else
            {
                this.GazeTarget = null;
            }
        }
    }
}
