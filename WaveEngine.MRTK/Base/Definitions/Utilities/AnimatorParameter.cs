// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Diagnostics;

namespace Microsoft.MixedReality.Toolkit.Utilities
{
    /// <summary>
    /// A copy of the <see href="https://docs.unity3d.com/ScriptReference/AnimatorControllerParameter.html">AnimatorControllerParameter</see> because that class is not Serializable and cannot be modified in the editor.
    /// </summary>
    [Serializable]
    public struct AnimatorParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatorParameter"/> struct.
        /// </summary>
        /// <param name="name">Name of the animation parameter to modify.</param>
        /// <param name="parameterType">Type of the animation parameter to modify.</param>
        /// <param name="defaultInt">If the animation parameter type is an int, value to set. Ignored otherwise.</param>
        /// <param name="defaultFloat">If the animation parameter type is a float, value to set. Ignored otherwise.</param>
        /// <param name="defaultBool">"If the animation parameter type is a bool, value to set. Ignored otherwise.</param>
        public AnimatorParameter(string name, AnimatorControllerParameterType parameterType, int defaultInt = 0, float defaultFloat = 0f, bool defaultBool = false)
        {
            this.parameterType = parameterType;
            this.defaultInt = defaultInt;
            this.defaultFloat = defaultFloat;
            this.defaultBool = defaultBool;
            this.name = name;
            this.nameStringHash = null;
        }

        private AnimatorControllerParameterType parameterType;

        /// <summary>
        /// Gets the Type of the animation parameter to modify.
        /// </summary>
        public AnimatorControllerParameterType ParameterType => this.parameterType;

        private int defaultInt;

        /// <summary>
        /// Gets the animation parameter type is an int, value to set. Ignored otherwise.
        /// </summary>
        public int DefaultInt => this.defaultInt;

        private float defaultFloat;

        /// <summary>
        /// Gets the animation parameter type is a float, value to set. Ignored otherwise.
        /// </summary>
        public float DefaultFloat => this.defaultFloat;

        private bool defaultBool;

        /// <summary>
        /// Gets a value indicating whether the animation parameter type is a bool, value to set. Ignored otherwise.
        /// </summary>
        public bool DefaultBool => this.defaultBool;

        private string name;

        /// <summary>
        /// Gets the name of the animation parameter to modify.
        /// </summary>
        public string Name => this.name;

        private int? nameStringHash;

        /// <summary>
        /// Gets the animator Name String to Hash.
        /// </summary>
        public int NameHash
        {
            get
            {
                if (!this.nameStringHash.HasValue && !string.IsNullOrEmpty(this.Name))
                {
                    this.nameStringHash = Animator.StringToHash(this.Name);
                }

                Debug.Assert(this.nameStringHash != null);
                return this.nameStringHash.Value;
            }
        }
    }
}
