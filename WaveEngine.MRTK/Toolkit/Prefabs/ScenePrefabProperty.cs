// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;

namespace WaveEngine.MRTK.Toolkit.Prefabs
{
    /// <summary>
    /// A property to hold the ID of a scene prefab to be instanced in a scene.
    /// </summary>
    public class ScenePrefabProperty
    {
        /// <summary>
        /// Gets or sets the prefab to use.
        /// </summary>
        [RenderProperty(Tooltip = "The prefab to use.")]
        public Guid PrefabId
        {
            get => this.prefabId;
            set
            {
                if (this.prefabId != value)
                {
                    this.prefabId = value;
                    this.Refresh();
                }
            }
        }

        private Guid prefabId;

        /// <summary>
        /// Gets a value indicating whether the prefab ID is set and valid.
        /// </summary>
        public bool IsPrefabIdValid => this.PrefabId != Guid.Empty;

        /// <summary>
        /// An event that will raise when the prefab ID changes.
        /// </summary>
        public event EventHandler OnScenePrefabChanged;

        /// <summary>
        /// Raise the changed event.
        /// </summary>
        public void Refresh()
        {
            this.OnScenePrefabChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
