// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Components.Animation;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Interface for cursor modifiers that can modify a <see cref="Entity"/>'s properties.
    /// </summary>
    public interface ICursorModifier : IMixedRealityFocusChangedHandler
    {
        /// <summary>
        /// Gets or sets the transform for which this <see cref="IMixedRealityCursor"/> modifies applies its various properties.
        /// </summary>
        Transform3D HostTransform { get; set; }

        /// <summary>
        /// Gets or sets how much a <see cref="IMixedRealityCursor"/>'s position should be offset from the surface of the <see cref="Entity"/> when overlapping.
        /// </summary>
        Vector3 CursorPositionOffset { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="IMixedRealityCursor"/> snap to the <see cref="Entity"/>'s position.
        /// </summary>
        bool SnapCursorPosition { get; set; }

        /// <summary>
        /// Gets or sets the scale of the <see cref="IMixedRealityCursor"/> when looking at this <see cref="Entity"/>.
        /// </summary>
        Vector3 CursorScaleOffset { get; set; }

        /// <summary>
        /// Gets or sets the Direction of the <see cref="IMixedRealityCursor"/> offset.
        /// </summary>
        Vector3 CursorNormalOffset { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether if is true, the normal from the pointing vector will be used to orient the <see cref="IMixedRealityCursor"/> instead of the targeted <see cref="Entity"/>'s normal at point of contact.
        /// </summary>
        bool UseGazeBasedNormal { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the <see cref="IMixedRealityCursor"/> be hidden when this <see cref="Entity"/> is focused.
        /// </summary>
        bool HideCursorOnFocus { get; set; }

        /// <summary>
        /// Gets the <see cref="IMixedRealityCursor"/> animation parameters to set when this <see cref="Entity"/> is focused. Leave empty for none.
        /// </summary>
        Animation3D[] CursorParameters { get; }

        /// <summary>
        /// Indicates whether the <see cref="IMixedRealityCursor"/> should be visible or not.
        /// </summary>
        /// <returns>True if <see cref="IMixedRealityCursor"/> should be visible, false if not.</returns>
        bool GetCursorVisibility();

        /// <summary>
        /// Returns the <see cref="IMixedRealityCursor"/> position after considering this modifier.
        /// </summary>
        /// <param name="cursor"><see cref="IMixedRealityCursor"/> that is being modified.</param>
        /// <returns>New position for the <see cref="IMixedRealityCursor"/>.</returns>
        Vector3 GetModifiedPosition(IMixedRealityCursor cursor);

        /// <summary>
        /// Returns the <see cref="IMixedRealityCursor"/> rotation after considering this modifier.
        /// </summary>
        /// <param name="cursor"><see cref="IMixedRealityCursor"/> that is being modified.</param>
        /// <returns>New rotation for the <see cref="IMixedRealityCursor"/>.</returns>
        Quaternion GetModifiedRotation(IMixedRealityCursor cursor);

        /// <summary>
        /// Returns the <see cref="IMixedRealityCursor"/>'s local scale after considering this modifier.
        /// </summary>
        /// <param name="cursor"><see cref="IMixedRealityCursor"/> that is being modified.</param>
        /// <returns>New local scale for the <see cref="IMixedRealityCursor"/>.</returns>
        Vector3 GetModifiedScale(IMixedRealityCursor cursor);

        /// <summary>
        /// Returns the modified <see cref="Transform3D"/> for the <see cref="IMixedRealityCursor"/> after considering this modifier.
        /// </summary>
        /// <param name="cursor">Cursor that is being modified.</param>
        /// <param name="position">Modified position.</param>
        /// <param name="rotation">Modified rotation.</param>
        /// <param name="scale">Modified scale.</param>
        void GetModifiedTransform(IMixedRealityCursor cursor, out Vector3 position, out Quaternion rotation, out Vector3 scale);
    }
}
