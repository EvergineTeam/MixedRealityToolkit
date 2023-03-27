// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using Evergine.Common.Attributes;
using Evergine.Framework.Prefabs;
using Evergine.MRTK.Managers.Data;
using System.Collections.Generic;

namespace Evergine.MRTK.Managers
{
    /// <content>
    /// Contains all the properties for the <see cref="MRTKManager"/> that can be configured from the Editor.
    /// </content>
    public partial class MRTKManager
    {
        /// <summary>
        /// Gets or sets the prefab that will be used for the left physical controller.
        /// </summary>
        [RenderProperty(
            CustomPropertyName = "Left Controller Prefab",
            Tooltip = "The prefab that will be used for the left physical controller")]
        public Prefab LeftPhysicalControllerPrefab { get; set; }

        /// <summary>
        /// Gets or sets the prefab that will be used for the right physical controller.
        /// </summary>
        [RenderProperty(
            CustomPropertyName = "Right Controller Prefab",
            Tooltip = "The prefab that will be used for the right physical controller")]
        public Prefab RightPhysicalControllerPrefab { get; set; }

        /// <summary>
        /// Gets or sets the prefab that will be used for the left articulated hand.
        /// </summary>
        [RenderProperty(
            CustomPropertyName = "Left Hand Prefab",
            Tooltip = "The prefab that will be used for the left articulated hand")]
        public Prefab LeftArticulatedHandPrefab { get; set; }

        /// <summary>
        /// Gets or sets the prefab that will be used for the right articulated hand.
        /// </summary>
        [RenderProperty(
            CustomPropertyName = "Right Hand Prefab",
            Tooltip = "The prefab that will be used for the right articulated hand")]
        public Prefab RightArticulatedHandPrefab { get; set; }

        /// <summary>
        /// Gets or sets the pointers associated with each type of controller.
        /// </summary>
        [RenderProperty(
            CustomPropertyName = "Pointer Options",
            Tooltip = "The pointers associated with each type of controller")]
        public List<PointerOption> PointerOptions { get; set; }
    }
}
