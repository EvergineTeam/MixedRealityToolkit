// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Framework.Threading;
using System;
using System.Linq;

namespace Evergine.MRTK.Emulation
{
    /// <summary>
    /// Ray for selecting where to teleport.
    /// </summary>
    public class TeleportRay : Behavior
    {
        //// Teleport Effect Area
        private Entity teleportAreaEntity = null;
        private StaticBody3D teleportAreaStaticBody = null;
        private SphereCollider3D sphereCollider = null;
        private Transform3D teleportAreaTransform = null;

        /// <inheritdoc/>
        protected override async void Start()
        {
            base.Start();
            if (Application.Current.IsEditor)
            {
                return;
            }

            await EvergineForegroundTask.Run(() =>
            {
                this.teleportAreaEntity = new Entity();
                this.teleportAreaTransform = new Transform3D();
                this.teleportAreaStaticBody = new StaticBody3D()
                {
                    IsSensor = true,
                    MaskBits = CollisionCategory3D.Cat1,
                    CollisionCategories = CollisionCategory3D.Cat3,
                };
                this.sphereCollider = new SphereCollider3D()
                {
                    Radius = 0.4f,
                    Margin = 0.040f,
                    Offset = new Mathematics.Vector3(0.0f, -0.5f, 0.0f),
                };
                this.teleportAreaEntity
                    .AddComponent(this.teleportAreaTransform)
                    .AddComponent(this.sphereCollider)
                    .AddComponent(this.teleportAreaStaticBody);
            });
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            throw new NotImplementedException();
        }
    }
}
