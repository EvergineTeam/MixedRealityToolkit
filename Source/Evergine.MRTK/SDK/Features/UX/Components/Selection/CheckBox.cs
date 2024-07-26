// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Components.Fonts;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Framework.Physics3D;
using Evergine.Mathematics;
using Evergine.MRTK.SDK.Features.UX.Components.ToggleButtons;
using System;

namespace Evergine.MRTK.SDK.Features.UX.Components.Selection
{
    /// <summary>
    /// Component to handle checkboxes.
    /// </summary>
    public class CheckBox : Component
    {
        private const float TextMarginWithCheckBox = 0.012f;
        private const float TextMarginWithBorder = 0.003f;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_Button_Root")]
        private BoxCollider3D boxCollider = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_Plate")]
        private Transform3D plateTransform = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_Visuals_FrontPlate")]
        private Transform3D frontPlateTransform = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_Icon")]
        private Transform3D iconTransform = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_Text")]
        private Transform3D textTransform = null;

        [BindComponent(source: BindComponentSource.Children, tag: "PART_Text")]
        private Text3DMesh textMesh = null;

        [BindComponent(source: BindComponentSource.Children)]
        private ToggleButton toggleButton = null;

        [BindComponent(source: BindComponentSource.Children)]
        private ToggleStateManager toggleStateManager = null;

        private Vector2 size = new Vector2(0.064f, 0.032f);
        private bool isChecked = false;

        /// <summary>
        /// Raised when <see cref="IsChecked" /> changes.
        /// </summary>
        public event EventHandler IsCheckedChanged;

        /// <summary>
        /// Gets or sets ComboBox size.
        /// </summary>
        public Vector2 Size
        {
            get => this.size;
            set
            {
                this.size = value;
                if (this.IsAttached)
                {
                    this.UpdateSize();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether checkbox is checked.
        /// </summary>
        public bool IsChecked
        {
            get => this.isChecked;

            set
            {
                if (this.isChecked != value)
                {
                    this.isChecked = value;
                    this.OnIsCheckedUpdate();
                }
            }
        }

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            bool attached = base.OnAttached();
            if (attached)
            {
                this.toggleButton.IsOn = this.isChecked;
                this.toggleStateManager.InitialState = this.isChecked ? ToggleState.On : ToggleState.Off;
                this.toggleButton.Toggled += this.ToggleButton_Toggled;
            }

            return attached;
        }

        /// <inheritdoc/>
        protected override void OnDetach()
        {
            base.OnDetach();

            this.toggleButton.Toggled -= this.ToggleButton_Toggled;
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            this.UpdateSize();
        }

        private void UpdateSize()
        {
            var colliderSize = this.boxCollider.Size;
            colliderSize.X = this.size.X;
            colliderSize.Y = this.size.Y;
            this.boxCollider.Size = colliderSize;

            var plateScale = this.plateTransform.LocalScale;
            plateScale.X = this.size.X;
            plateScale.Y = this.size.Y;
            this.plateTransform.LocalScale = plateScale;

            var frontPlateScale = this.frontPlateTransform.LocalScale;
            frontPlateScale.X = this.size.X;
            frontPlateScale.Y = this.size.Y;
            this.frontPlateTransform.LocalScale = frontPlateScale;

            var iconPosition = this.iconTransform.LocalPosition;
            var iconScale = this.iconTransform.LocalScale;
            iconPosition.X = (iconScale.X - this.Size.X) / 2;
            this.iconTransform.LocalPosition = iconPosition;

            var textPosition = this.textTransform.LocalPosition;
            textPosition.X = iconPosition.X + TextMarginWithCheckBox;
            this.textTransform.LocalPosition = textPosition;

            var textMeshSize = this.textMesh.Size;
            textMeshSize.X = (this.size.X / 2) - textPosition.X - TextMarginWithBorder;
            this.textMesh.Size = textMeshSize;
        }

        private void OnIsCheckedUpdate() => this.toggleButton.IsOn = this.isChecked;

        private void ToggleButton_Toggled(object sender, EventArgs e)
        {
            this.isChecked = this.toggleButton.IsOn;
            this.IsCheckedChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
