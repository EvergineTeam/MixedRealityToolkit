// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Physics3D;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;

namespace WaveEngine.MRTK.Emulation
{
    /// <summary>
    /// Visible cursor in scene.
    /// </summary>
    public class Cursor : Behavior
    {
        [BindComponent]
        internal Transform3D Transform { get; private set; }

        [BindComponent]
        internal StaticBody3D StaticBody3D { get; private set; }

        /// <summary>
        /// The Material component.
        /// </summary>
        [BindComponent(isRequired: false, source: BindComponentSource.Children)]
        protected MaterialComponent materialComponent;

        /// <summary>
        /// The cursor manager.
        /// </summary>
        [BindSceneManager]
        protected CursorManager cursorManager;

        /// <summary>
        /// Gets or sets the material when the cursor is pressed.
        /// </summary>
        [RenderProperty(Tooltip = "The material to be set when the cursor is pressed")]
        public Material PressedMaterial { get; set; }

        /// <summary>
        /// Gets or sets the material when the cursor is released.
        /// </summary>
        [RenderProperty(Tooltip = "The material to be set when the cursor is released")]
        public Material ReleasedMaterial { get; set; }

        private bool pinch;

        /// <summary>
        /// Gets or sets a value indicating whether the cursor is pinched.
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
                if (this.pinch != value)
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

        private bool isTouch;

        /// <summary>
        /// Gets or sets a value indicating whether the cursor should
        /// produce <see cref="IMixedRealityPointerHandler"/> events.
        /// <para>
        /// This property cannot be changed while <see cref="Cursor"/> is activated.</para>
        /// </summary>
        public bool IsTouch
        {
            get => this.isTouch;
            set
            {
                if (this.IsActivated)
                {
                    throw new InvalidOperationException($"{nameof(this.IsTouch)} property cannot be changed while {nameof(Cursor)} is activated.");
                }

                this.isTouch = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the cursor is colliding with a <see cref="IMixedRealityPointerHandler"/>.
        /// </summary>
        [WaveIgnore]
        [DontRenderProperty]
        public bool IsTouching { get; internal set; }

        /// <summary>
        /// Gets or sets a value indicating whether the cursor visible.
        /// </summary>
        [WaveIgnore]
        public bool IsVisible
        {
            get => this.materialComponent?.Owner.IsEnabled == true;
            set
            {
                if (this.materialComponent != null)
                {
                    this.materialComponent.Owner.IsEnabled = value;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();
            this.cursorManager.AddCursor(this);
            this.UpdateColor();
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();
            this.cursorManager.RemoveCursor(this);
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            this.PreviousPinch = this.Pinch;
        }

        private void UpdateColor()
        {
            if (this.materialComponent == null)
            {
                return;
            }

            this.materialComponent.Material = this.Pinch ? this.PressedMaterial : this.ReleasedMaterial;
        }
    }
}
