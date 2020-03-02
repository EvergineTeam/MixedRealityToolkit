// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System.Collections;
using WaveEngine.Framework.Physics3D;
using WaveEngine.Mathematics;
using WaveEngine.MixedReality.Toolkit.Physics;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// Interface for handling pointers.
    /// </summary>
    public interface IMixedRealityPointer : IEqualityComparer
    {
        /// <summary>
        /// Gets or sets the pointer's current controller reference.
        /// </summary>
        IMixedRealityController Controller { get; set; }

        /// <summary>
        /// Gets the pointer's id.
        /// </summary>
        uint PointerId { get; }

        /// <summary>
        /// Gets or sets the pointer's name.
        /// </summary>
        string PointerName { get; set; }

        /// <summary>
        /// Gets the pointer's input source parent.
        /// </summary>
        IMixedRealityInputSource InputSourceParent { get; }

        /// <summary>
        /// Gets or sets the pointer's cursor.
        /// </summary>
        IMixedRealityCursor BaseCursor { get; set; }

        /// <summary>
        /// Gets or sets the currently active cursor modifier.
        /// </summary>
        ICursorModifier CursorModifier { get; set; }

        /// <summary>
        /// Gets a value indicating whether the pointer active and have the conditions for the interaction been satisfied to enable the interaction?.
        /// </summary>
        bool IsInteractionEnabled { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the pointer dispatches input..
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the focus for this pointer currently locked?.
        /// </summary>
        bool IsFocusLocked { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the pointer's target position (cursor) is locked to the target object when focus is locked.
        /// </summary>
        bool IsTargetPositionLockedOnFocusLock { get; set; }

        /// <summary>
        /// Gets the scene query rays.
        /// </summary>
        RayStep[] Rays { get; }

        /// <summary>
        /// Gets or sets the physics layers to use when performing scene queries.
        /// </summary>
        /// <remarks>If set, will override the <see cref="IMixedRealityInputSystem"/>'s default scene query layer mask array.</remarks>
        CollisionCategory3D PrioritizedLayerMasksOverride { get; set; }

        /// <summary>
        /// Gets or sets the currently focused target.
        /// </summary>
        IMixedRealityFocusHandler FocusTarget { get; set; }

        /// <summary>
        /// Gets or sets the scene query pointer result.
        /// </summary>
        IPointerResult Result { get; set; }

        /// <summary>
        /// Gets or sets the type of physics scene query to use.
        /// </summary>
        SceneQueryType SceneQueryType { get; set; }

        /// <summary>
        /// Gets or sets the radius to use when <see cref="SceneQueryType"/> is set to Sphere or SphereColliders.
        /// </summary>
        float SphereCastRadius { get; set; }

        /// <summary>
        /// Gets the Pointer position.
        /// </summary>
        Vector3 Position { get; }

        /// <summary>
        /// Gets the Pointer rotation.
        /// </summary>
        Quaternion Orientation { get; }

        /// <summary>
        /// Called before performing the scene query.
        /// </summary>
        void OnPreSceneQuery();

        /// <summary>
        /// Called after performing the scene query.
        /// </summary>
        void OnPostSceneQuery();

        /// <summary>
        /// Called during the scene query just before the current pointer target changes.
        /// </summary>
        void OnPreCurrentPointerTargetChange();

        /// <summary>
        /// Resets pointer to initial state. After invoked pointer should be functional and ready for re-use.
        /// </summary>
        /// <remarks>
        /// Useful for caching and recycling of pointers.
        /// </remarks>
        void Reset();
    }
}
