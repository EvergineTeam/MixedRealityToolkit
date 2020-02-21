// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using WaveEngine.Framework;
using WaveEngine.Mathematics;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Cursor Interface for handling input events and setting visibility.
    /// </summary>
    public interface IMixedRealityCursor : IMixedRealityFocusChangedHandler, IMixedRealitySourceStateHandler, IMixedRealityPointerHandler
    {
        /// <summary>
        /// Gets or sets the <see cref="IMixedRealityPointer"/> this <see cref="IMixedRealityCursor"/> is associated with.
        /// </summary>
        IMixedRealityPointer Pointer { get; set; }

        /// <summary>
        /// Gets the surface distance to place the cursor off of the surface at.
        /// </summary>
        float SurfaceCursorDistance { get; }

        /// <summary>
        /// Gets or sets the maximum distance the cursor can be with nothing hit.
        /// </summary>
        float DefaultCursorDistance { get; set; }

        /// <summary>
        /// Gets the position of the <see cref="IMixedRealityCursor"/>.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Gets the orientation of the <see cref="IMixedRealityCursor"/>.
        /// </summary>
        Quaternion Orientation { get; }

        /// <summary>
        /// Gets the local scale of the <see cref="IMixedRealityCursor"/>.
        /// </summary>
        Vector3 LocalScale { get; }

        /// <summary>
        /// Sets the visibility of the <see cref="IMixedRealityCursor"/>.
        /// </summary>
        /// <param name="visible">True if cursor should be visible, false if not.</param>
        void SetVisibility(bool visible);

        /// <summary>
        /// Utility method to destroy cursor dependencies (e.g. event subscriptions) in the system
        /// explicitly in the middle update loop. This is a "replacement" of Unity OnDestroy.
        /// Relying on Unity OnDestroy may cause event handler subscription to
        /// become invalid at the point of destroying.
        /// </summary>
        void Destroy();

        /// <summary>
        /// Gets a value indicating whether the cursor is currently visible?.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the visibility of the <see cref="IMixedRealityCursor"/> when the source is detected.
        /// </summary>
        bool SetVisibilityOnSourceDetected { get; set; }

        /// <summary>
        /// Gets the <see cref="IMixedRealityCursor"/>'s <see cref="Entity"/> reference.
        /// </summary>
        /// <returns>The <see cref="Entity"/> this <see cref="IMixedRealityCursor"/> component is attached to.</returns>
        Entity EntityReference { get; }
    }
}
