// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Materials;
using WaveEngine.Framework.Physics3D;
using WaveEngine.MRTK.Toolkit.Extensions;

namespace WaveEngine.MRTK.Emulation
{
    /// <summary>
    /// Visible cursor in scene.
    /// </summary>
    public class Cursor : Behavior
    {
        /// <summary>
        /// The transform.
        /// </summary>
        [BindComponent]
        public Transform3D transform = null;

        /// <summary>
        /// The collider.
        /// </summary>
        [BindComponent(isExactType: false)]
        public Collider3D Collider3D;

        /// <summary>
        /// The StaticBody3D.
        /// </summary>
        [BindComponent]
        public StaticBody3D StaticBody3D;

        /// <summary>
        /// The MeshRenderer.
        /// </summary>
        [BindComponent]
        public MeshRenderer meshRenderer;

        /// <summary>
        /// The Material component.
        /// </summary>
        [BindComponent(isRequired: false)]
        protected MaterialComponent materialComponent;

        /// <summary>
        /// Gets or sets the color when the cursos is pressed.
        /// </summary>
        [RenderProperty(Tooltip = "The color to be set to the material when the cursor is pressed")]
        public Color PressedColor { get; set; }

        /// <summary>
        /// Gets or sets the color when the cursos is released.
        /// </summary>
        [RenderProperty(Tooltip = "The color to be set to the material when the cursor is released")]
        public Color ReleasedColor { get; set; }

        private bool pinch;

        /// <summary>
        /// Gets or sets a value indicating whether the cursor is pinched or not.
        /// </summary>
        [WaveIgnore]
        [DontRenderProperty]
        public bool Pinch
        {
            get
            {
                return this.pinch;
            }

            set
            {
                if (value != this.pinch)
                {
                    this.pinch = value;
                    this.UpdateColor();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets a value indicating the previous pinch.
        /// </summary>
        [WaveIgnore]
        [DontRenderProperty]
        public bool PreviousPinch { get; private set; }

        private StandardMaterial material;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            return base.OnAttached();
        }

        /// <inheritdoc/>
        protected override void Start()
        {
            if (this.materialComponent != null)
            {
                if (!Application.Current.IsEditor)
                {
                    this.materialComponent.Material = this.materialComponent.Material.Clone();

                    this.Managers.FindManager<CursorManager>().AddCursor(this);
                }

                this.material = new StandardMaterial(this.materialComponent.Material);
            }
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            this.PreviousPinch = this.Pinch;
        }

        private void UpdateColor()
        {
            if (this.material != null)
            {
                this.material.BaseColor = this.Pinch ? this.PressedColor : this.ReleasedColor;
            }
        }
    }
}
