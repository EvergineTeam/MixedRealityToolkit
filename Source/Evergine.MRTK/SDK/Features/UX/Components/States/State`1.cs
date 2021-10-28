// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

namespace Evergine.MRTK.SDK.Features.UX.Components.States
{
    /// <summary>
    /// UI element state model.
    /// </summary>
    /// <typeparam name="T">Value type.</typeparam>
    public class State<T>
    {
        /// <summary>
        /// Gets or sets state name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets value.
        /// </summary>
        public T Value { get; set; }
    }
}
