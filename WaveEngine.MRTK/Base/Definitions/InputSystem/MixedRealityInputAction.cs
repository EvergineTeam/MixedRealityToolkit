// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections;
using WaveEngine.MixedReality.Toolkit.Utilities;

namespace WaveEngine.MixedReality.Toolkit.Input
{
    /// <summary>
    /// An Input Action for mapping an action to an Input Sources Button, Joystick, Sensor, etc.
    /// </summary>
    [Serializable]
    public struct MixedRealityInputAction : IEqualityComparer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MixedRealityInputAction"/> struct.
        /// Constructor.
        /// </summary>
        /// <param name="id">The action id.</param>
        /// <param name="description">The action description.</param>
        /// <param name="axisConstraint">The axis constant.</param>
        public MixedRealityInputAction(uint id, string description, AxisType axisConstraint = AxisType.None)
        {
            this.id = id;
            this.description = description;
            this.axisConstraint = axisConstraint;
        }

        /// <summary>
        /// Gets the default input action.
        /// </summary>
        public static MixedRealityInputAction None { get; } = new MixedRealityInputAction(0, "None");

        /// <summary>
        /// Gets the Unique Id of this Input Action.
        /// </summary>
        public uint Id => this.id;

        private uint id;

        /// <summary>
        /// Gets the short description of the Input Action.
        /// </summary>
        public string Description => this.description;

        private string description;

        /// <summary>
        /// Gets the Axis constraint for the Input Action.
        /// </summary>
        public AxisType AxisConstraint => this.axisConstraint;

        private AxisType axisConstraint;

        public static bool operator ==(MixedRealityInputAction left, MixedRealityInputAction right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MixedRealityInputAction left, MixedRealityInputAction right)
        {
            return !left.Equals(right);
        }

        #region IEqualityComparer Implementation

        /// <summary>
        /// Equal comparison.
        /// </summary>
        /// <param name="left">The left object to compare.</param>
        /// <param name="right">The right object to compare.</param>
        /// <returns>True if are equal.</returns>
        bool IEqualityComparer.Equals(object left, object right)
        {
            if (left is null || right is null)
            {
                return false;
            }

            if (!(left is MixedRealityInputAction) || !(right is MixedRealityInputAction))
            {
                return false;
            }

            return ((MixedRealityInputAction)left).Equals((MixedRealityInputAction)right);
        }

        /// <summary>
        /// Equal comparison.
        /// </summary>
        /// <param name="other">The other input action .</param>
        /// <returns>True if are equal.</returns>
        public bool Equals(MixedRealityInputAction other)
        {
            return this.Id == other.Id &&
                   this.AxisConstraint == other.AxisConstraint;
        }

        /// <summary>
        /// Equal comparison.
        /// </summary>
        /// <param name="obj">The other input action .</param>
        /// <returns>True if are equal.</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            return obj is MixedRealityInputAction && this.Equals((MixedRealityInputAction)obj);
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>The hash code.</returns>
        int IEqualityComparer.GetHashCode(object obj)
        {
            return obj is MixedRealityInputAction ? ((MixedRealityInputAction)obj).GetHashCode() : 0;
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return $"{this.Id}.{this.AxisConstraint}".GetHashCode();
        }

        #endregion IEqualityComparer Implementation
    }
}
