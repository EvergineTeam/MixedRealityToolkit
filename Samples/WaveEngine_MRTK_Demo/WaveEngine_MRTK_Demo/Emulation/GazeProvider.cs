// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons;
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
            if (!Application.Current.IsEditor)
            {
                Entity hoverLight = new Entity("GazePointer")
                .AddComponent(new Transform3D())
                .AddComponent(new HoverLight())
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
                List<HitResult3D> results = new List<HitResult3D>();
                Ray r = ray.Value;
                this.Managers.PhysicManager3D.RayCastAll(ref r, this.Distance, results, this.Mask);


                Entity hitEntity = (results.Count > 0) ? ((PhysicBody3D)results[0].PhysicBody.UserData).Owner : null;
                if (hitEntity != this.GazeTarget)
                {
                    this.GazeTarget = hitEntity;
                }

                // Update Hover light
                if (results.Count > 0)
                {
                    this.hoverLight.Position = results[0].Point;
                }
            } 
            else
            {
                this.GazeTarget = null;
            }
        }
    }
}
