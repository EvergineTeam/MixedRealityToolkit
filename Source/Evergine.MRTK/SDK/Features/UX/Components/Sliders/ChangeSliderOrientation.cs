// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Common.Attributes;
using Evergine.Common.Attributes.Converters;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;

namespace Evergine.MRTK.SDK.Features.UX.Components.Sliders
{
    /// <summary>
    /// Rotates slider elements. Default rotation is to make it horizontal.
    /// </summary>
    public class ChangeSliderOrientation : Component
    {
        private const float DefaultRotation = -(float)(Math.PI / 2f);

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_Root", isRequired: true)]
        private Transform3D rootTransform = null;

        [BindComponent(source: BindComponentSource.ChildrenSkipOwner, tag: "PART_Text", isRequired: false)]
        private Transform3D textTransform = null;

        private float rotation = DefaultRotation;

        /// <summary>
        /// Gets or sets slider rotation.
        /// </summary>
        [RenderProperty(typeof(FloatRadianToDegreeConverter))]
        public float Rotation
        {
            get => this.rotation;

            set
            {
                if (this.rotation != value)
                {
                    this.rotation = value;
                    this.OnRotationUpdated();
                }
            }
        }

        /// <inheritdoc />
        protected override void OnActivated()
        {
            base.OnActivated();
            this.OnRotationUpdated();
        }

        private void OnRotationUpdated()
        {
            var rotationVector = new Vector3(0, 0, this.rotation);

            if (this.rootTransform != null)
            {
                this.rootTransform.LocalRotation = rotationVector;
            }

            if (this.textTransform != null)
            {
                this.textTransform.LocalRotation = -rotationVector;
            }
        }
    }
}
