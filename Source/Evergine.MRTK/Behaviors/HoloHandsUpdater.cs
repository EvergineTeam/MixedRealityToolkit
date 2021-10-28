// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.XR;
using Evergine.Mathematics;
using Evergine.MRTK.Effects;
using Evergine.MRTK.Emulation;
using Evergine.MRTK.SDK.Features;

namespace Evergine.MRTK.Behaviors
{
    /// <summary>
    /// Updates hands shader.
    /// </summary>
    public class HoloHandsUpdater : Behavior
    {
        /// <summary>
        /// Gets or sets Left or right hand.
        /// </summary>
        public XRHandedness Handedness { get; set; }

        private HoloHandsLocal holoHandsDecorator;
        private Material material;
        private TrackXRJoint trackXRJoint;

        private float time = 0;
        private string[] directivesAnimating = { "PULSE", "REFLECTION" };
        private string[] directivesNotAnimating = { "BASE", "REFLECTION" };
        private bool isAnimating = true;
        private MeshRenderer meshRenderer;
        private MeshRenderer cursorMeshRenderer;

        /// <inheritdoc/>
        protected override void Start()
        {
            if (!Application.Current.IsEditor)
            {
                var materialComponent = this.Owner.FindComponent<MaterialComponent>();
                materialComponent.Material = materialComponent.Material.Clone();
                this.material = materialComponent.Material;
                this.holoHandsDecorator = new HoloHandsLocal(this.material);
                this.material.ActiveDirectivesNames = this.directivesAnimating;
                this.meshRenderer = this.Owner.FindComponent<MeshRenderer>();

                foreach (var c in Cursor.ActiveCursors)
                {
                    var joint = c.Owner.FindComponent<TrackXRJoint>();
                    if (joint != null && joint.Handedness == this.Handedness)
                    {
                        this.trackXRJoint = joint;
                        this.cursorMeshRenderer = c.Owner.FindComponentInChildren<MeshRenderer>();
                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            if (this.trackXRJoint != null)
            {
                if (Tools.IsJointValid(this.trackXRJoint))
                {
                    this.time = MathHelper.Clamp(this.time + ((float)gameTime.TotalSeconds * 0.6f), 0, 1);
                }
                else
                {
                    this.time = MathHelper.Clamp(this.time - ((float)gameTime.TotalSeconds * 0.6f), 0, 1);
                }

                if (this.isAnimating)
                {
                    this.holoHandsDecorator.Matrices_T = 1 - this.time;
                }

                bool isAnimating = this.time != 0 && this.time != 1;
                if (isAnimating != this.isAnimating)
                {
                    this.material.ActiveDirectivesNames = isAnimating ? this.directivesAnimating : this.directivesNotAnimating;
                    this.meshRenderer.IsEnabled = isAnimating || this.time == 1;
                    this.cursorMeshRenderer.IsEnabled = this.time != 0;
                    this.isAnimating = isAnimating;
                }
            }
        }
    }
}
