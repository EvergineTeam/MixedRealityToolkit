// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using System.Text;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;

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
        /// Gets or sets the max distance to capture.
        /// </summary>
        public float Distance { get; set; } = 10.0f;

        /// <summary>
        /// Gets or sets the collision category bits.
        /// </summary>
        public CollisionCategory3D Mask { get; set; }

        /// <summary>
        /// Gets the target aimed by the gaze or null.
        /// </summary>
        public Entity GazeTarget { get; private set; }

        /// <summary>
        /// Update.
        /// </summary>
        /// <param name="gameTime">The time.</param>
        protected override void Update(TimeSpan gameTime)
        {
            //// TODO: in unity this is called by the focus provider

            var xrPlatform = Application.Current.Container.Resolve<XRPlatform>();
            Ray? ray = null;
            if (xrPlatform != null)
            {
                ray = xrPlatform.EyeGaze;
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

                if (results.Count > 0)
                {
                    Entity hitEntity = ((StaticBody3D)results[0].PhysicBody.UserData).Owner;
                    if (hitEntity != this.GazeTarget)
                    {
                        this.GazeTarget = hitEntity;
                    }
                }
            }
        }
    }
}
