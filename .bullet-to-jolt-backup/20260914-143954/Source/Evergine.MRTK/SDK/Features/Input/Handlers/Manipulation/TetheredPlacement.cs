// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Mathematics;

namespace Evergine.MRTK.SDK.Features.Input.Handlers.Manipulation
{
    /// <inheritdoc/>
    public class TetheredPlacement : Behavior
    {
        /// <summary>
        /// The transform.
        /// </summary>
        [BindComponent]
        protected Transform3D transform = null;

        /// <summary>
        /// The rigid body.
        /// </summary>
        [BindComponent(isRequired: false)]
        protected RigidBody3D rigidBody;

        /// <summary>
        /// Gets or sets the distance threshold.
        /// </summary>
        public float DistanceThreshold { get; set; } = 0.5f;

        private Vector3 respawnPoint;
        private Quaternion respawnOrientation;

        /// <inheritdoc/>
        protected override void Start()
        {
            this.LockSpawnPoint();
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            float distance = (this.transform.Position - this.respawnPoint).Length();

            if (distance > this.DistanceThreshold)
            {
                if (this.rigidBody != null)
                {
                    this.rigidBody.ResetTransform(this.respawnPoint, this.respawnOrientation, this.transform.Scale);
                    this.rigidBody.LinearVelocity = Vector3.Zero;
                    this.rigidBody.AngularVelocity = Vector3.Zero;
                }
                else
                {
                    this.transform.Position = this.respawnPoint;
                    this.transform.Orientation = this.respawnOrientation;
                }
            }
        }

        /// <summary>
        /// Resets the spawn point.
        /// </summary>
        public void LockSpawnPoint()
        {
            this.respawnPoint = this.transform.Position;
            this.respawnOrientation = this.transform.Orientation;
        }
    }
}
