// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using Evergine.Framework;
using Evergine.Framework.Managers;
using Evergine.MRTK.Base.EventDatum.Input;
using Evergine.MRTK.Base.Interfaces.InputSystem.Handlers;
using Evergine.MRTK.Emulation;
using Evergine.MRTK.Extensions;

namespace Evergine.MRTK.Services.InputSystem
{
    /// <summary>
    /// The scene manager that controls entity focus.
    /// </summary>
    public class FocusProvider : SceneManager
    {
        private Dictionary<Entity, int> focusSources = new Dictionary<Entity, int>();

        /// <summary>
        /// Call this method when the entity receives a new focus source.
        /// </summary>
        /// <param name="entity">The affected entity.</param>
        /// <param name="cursor">The cursor that gave focus, if any.</param>
        public void FocusEnter(Entity entity, Cursor cursor)
        {
            if (entity != null)
            {
                if (!this.focusSources.TryGetValue(entity, out _))
                {
                    this.focusSources[entity] = 1;

                    this.RunFocusHandlers(entity, cursor, (h, e) => h?.OnFocusEnter(e));
                }
                else
                {
                    this.focusSources[entity]++;
                }
            }
        }

        /// <summary>
        /// Call this method when the entity loses a focus source.
        /// </summary>
        /// <param name="entity">The affected entity.</param>
        /// <param name="cursor">The cursor that left focus, if any.</param>
        public void FocusExit(Entity entity, Cursor cursor)
        {
            if (entity != null)
            {
                if (this.focusSources.TryGetValue(entity, out var count))
                {
                    if (count == 1)
                    {
                        this.focusSources.Remove(entity);

                        this.RunFocusHandlers(entity, cursor, (h, e) => h?.OnFocusExit(e));
                    }
                    else
                    {
                        this.focusSources[entity]--;
                    }
                }
            }
        }

        private void RunFocusHandlers(Entity entity, Cursor cursor, Action<IMixedRealityFocusHandler, MixedRealityFocusEventData> action)
        {
            var eventArgs = new MixedRealityFocusEventData()
            {
                Cursor = cursor,
                CurrentTarget = entity,
            };

            entity.RunOnComponents<IMixedRealityFocusHandler>((x) => action(x, eventArgs));
        }
    }
}
