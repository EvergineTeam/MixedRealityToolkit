// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Common.Attributes;
using Evergine.Components.Graphics3D;
using Evergine.Framework;
using Evergine.Framework.Graphics;
using Evergine.Mathematics;
using Evergine.MRTK.Emulation;

namespace Evergine.MRTK.SDK.Features.UX.Components.PressableButtons
{
    /// <summary>
    /// Visual feedback of a pressable button.
    /// </summary>
    public class PressableButtonVisualFeedbackComponent : Component, IPressableButtonFeedback
    {
        /// <summary>
        /// The transform.
        /// </summary>
        [BindComponent]
        protected Transform3D transform;

        /// <summary>
        /// The material component.
        /// </summary>
        [BindComponent(isRequired: false)]
        protected MaterialComponent materialComponent;

        /// <summary>
        /// Gets or sets the color that the optional MaterialComponent will be set to when the button is pressed.
        /// </summary>
        [RenderProperty(Tooltip = "The color that the optional MaterialComponent will be set to when the button is pressed")]
        public Material PressedMaterial { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the button visuals scale will be modified when pressed.
        /// </summary>
        [RenderProperty(Tooltip = "Set whether the button visuals scale will be modified when pressed")]
        public bool Compressable { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating the minimum ratio of the original scale the visual feedback entity can be compressed to.
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "The minimum ratio of the original scale the visual feedback entity can be compressed to.", MinLimit = 0.0f, MaxLimit = 1.0f)]
        public float MinCompressRatio { get; set; } = 0.25f;

        /// <summary>
        /// Gets or sets a value indicating the scale to apply to the movement of the entity.
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "The scale to apply to the movement of the entity.", MinLimit = 0.0f, MaxLimit = 1.0f)]
        public float MovementScale { get; set; } = 1f;

        /// <summary>
        /// Gets or sets a value indicating whether the button visuals color will be modified when pressed.
        /// </summary>
        [RenderProperty(Tooltip = "Set whether the button visuals color will be modified when pressed")]
        public bool ChangeColor { get; set; } = false;

        /// <summary>
        /// Gets or sets the distance offset that this entity will receive when the button is focused.
        /// </summary>
        [RenderProperty(Tooltip = "The distance offset that this entity will receive when the button is focused")]
        public float FocusedAccentDistance { get; set; } = 0.0f;

        private Vector3 movingVisualsInitialLocalPosition;
        private Vector3 movingVisualsInitialLocalScale;

        private Material releasedMaterial;

        private Vector3 focusOffset;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            if (this.materialComponent?.Material != null)
            {
                // Cache current material as released
                this.releasedMaterial = this.materialComponent.Material;
            }

            this.movingVisualsInitialLocalPosition = this.transform.LocalPosition;
            this.movingVisualsInitialLocalScale = this.transform.LocalScale;

            return true;
        }

        /// <inheritdoc/>
        void IPressableButtonFeedback.Feedback(Vector3 pushVector, float pressRatio, bool pressed)
        {
            if (this.Compressable)
            {
                // Compress the button visuals by the push amount.
                float press = Math.Max(this.MinCompressRatio, 1.0f - pressRatio);

                Vector3 scale = this.transform.LocalScale;
                scale.Z = this.movingVisualsInitialLocalScale.Z * press;
                this.transform.LocalScale = scale;
            }
            else
            {
                // Transform the push vector into the collider's parent's space
                Vector3 pushVectorLocal = Vector3.TransformNormal(pushVector, this.transform.WorldToLocalTransform);

                // Move visuals
                this.transform.LocalPosition = this.movingVisualsInitialLocalPosition + (pushVectorLocal * this.MovementScale) + this.focusOffset;
            }

            if (this.ChangeColor)
            {
                // Set MaterialComponent color
                this.FixUpReleasedMaterial();
                this.materialComponent.Material = pressed ? this.PressedMaterial : this.releasedMaterial;
            }
        }

        /// <inheritdoc/>
        void IPressableButtonFeedback.FocusChanged(Cursor cursor, bool focus)
        {
            this.focusOffset = Vector3.Forward * this.FocusedAccentDistance * (focus ? 1 : 0);
            this.transform.LocalPosition = this.movingVisualsInitialLocalPosition + this.focusOffset;
        }

        /// <summary>
        /// It's possible that other component changes default button material, that here is cached
        /// when component is attached.
        /// </summary>
        private void FixUpReleasedMaterial()
        {
            var currentMaterial = this.materialComponent?.Material;
            if (currentMaterial == null)
            {
                return;
            }

            if (currentMaterial.Id != this.releasedMaterial?.Id &&
                currentMaterial.Id != this.PressedMaterial?.Id)
            {
                this.releasedMaterial = currentMaterial;
            }
        }
    }
}
