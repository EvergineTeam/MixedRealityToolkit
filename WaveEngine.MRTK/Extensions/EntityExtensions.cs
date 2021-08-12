// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System.Collections.Generic;
using WaveEngine.Framework;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;

namespace WaveEngine.MRTK.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="Entity"/> used by the toolkit.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Finds <see cref="IMixedRealityEventHandler"/> components in the entity and its parents.
        /// </summary>
        /// <typeparam name="T">The <see cref="IMixedRealityEventHandler"/> type.</typeparam>
        /// <param name="entity">The entity used to find the components.</param>
        /// <returns>
        /// A <see cref="IEnumerable{T}"/> containing the found components that implements <typeparamref name="T"/>.
        /// </returns>
        public static IEnumerable<T> FindEventHandlers<T>(this Entity entity)
            where T : IMixedRealityEventHandler
        {
            if (entity is null)
            {
                throw new System.ArgumentNullException(nameof(entity));
            }

            var current = entity;
            while (current != null)
            {
                foreach (var c in current.Components)
                {
                    if (c.IsActivated && c is T interactable)
                    {
                        yield return interactable;
                    }
                }

                current = current.Parent;
            }
        }

        /// <summary>
        /// Checks if the entity or its parents has <see cref="IMixedRealityEventHandler"/> components.
        /// </summary>
        /// <typeparam name="T">The <see cref="IMixedRealityEventHandler"/> type.</typeparam>
        /// <param name="entity">The entity used to find the components.</param>
        /// <returns>
        /// <see langword="true"/> if the entity or its parents contains a component implementing <typeparamref name="T"/>;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool HasEventHandlers<T>(this Entity entity)
            where T : IMixedRealityEventHandler
        {
            if (entity is null)
            {
                throw new System.ArgumentNullException(nameof(entity));
            }

            var current = entity;
            while (current != null)
            {
                foreach (var c in current.Components)
                {
                    if (c.IsActivated && c is T)
                    {
                        return true;
                    }
                }

                current = current.Parent;
            }

            return false;
        }

        /// <summary>
        /// Checks if the entity or its parents has <see cref="IMixedRealityEventHandler"/> components.
        /// </summary>
        /// <typeparam name="T1">A first type derived from <see cref="IMixedRealityEventHandler"/> type.</typeparam>
        /// <typeparam name="T2">A second type derived from <see cref="IMixedRealityEventHandler"/> type.</typeparam>
        /// <param name="entity">The entity used to find the components.</param>
        /// <returns>
        /// <see langword="true"/> if the entity or its parents contains a component implementing <typeparamref name="T1"/>
        /// or <typeparamref name="T2"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool HasEventHandlers<T1, T2>(this Entity entity)
            where T1 : IMixedRealityEventHandler
            where T2 : IMixedRealityEventHandler
        {
            if (entity is null)
            {
                throw new System.ArgumentNullException(nameof(entity));
            }

            var current = entity;
            while (current != null)
            {
                foreach (var c in current.Components)
                {
                    if (c.IsActivated && (c is T1 || c is T2))
                    {
                        return true;
                    }
                }

                current = current.Parent;
            }

            return false;
        }
    }
}
