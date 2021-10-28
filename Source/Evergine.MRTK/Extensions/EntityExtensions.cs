// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using Evergine.Framework;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;

namespace Evergine.MRTK.Extensions
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
                throw new ArgumentNullException(nameof(entity));
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
                throw new ArgumentNullException(nameof(entity));
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
                throw new ArgumentNullException(nameof(entity));
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

        /// <summary>
        /// Runs the specified action for the components from the given entity implementing the <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">The component type of interface.</typeparam>
        /// <param name="entity">The entity used to find the components.</param>
        /// <param name="action">The action callback to be invoked on every <typeparamref name="T"/> component.</param>
        public static void RunOnComponents<T>(this Entity entity, Action<T> action)
            where T : IMixedRealityEventHandler
        {
            foreach (var interactable in entity.FindEventHandlers<T>())
            {
                action(interactable);
            }
        }
    }
}
