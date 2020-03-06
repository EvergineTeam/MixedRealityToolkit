// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using WaveEngine.Framework;

#pragma warning disable SA1600 // Elements should be documented

namespace WaveEngine.EventSystems
{
    /// <summary>
    /// Static class for event execution.
    /// </summary>
    public static class ExecuteEvents
    {
        public delegate void EventFunction<T1>(T1 handler, BaseEventData eventData);

        public static T ValidateEventData<T>(BaseEventData data)
            where T : class
        {
            if ((data as T) == null)
            {
                throw new ArgumentException(string.Format("Invalid type: {0} passed to event expecting {1}", data.GetType(), typeof(T)));
            }

            return data as T;
        }

        private static void GetEventChain(Entity root, IList<Entity> eventChain)
        {
            eventChain.Clear();
            if (root == null)
            {
                return;
            }

            var t = root;
            while (t != null)
            {
                eventChain.Add(t);
                t = t.Parent;
            }
        }

        private class ListPooledObjectPolicy<T> : PooledObjectPolicy<List<T>>
        {
            public override List<T> Create()
            {
                return new List<T>();
            }

            public override bool Return(List<T> obj)
            {
                obj.Clear();
                return true;
            }
        }

        private static readonly ObjectPool<List<IEventSystemHandler>> handlerListPool = new DefaultObjectPool<List<IEventSystemHandler>>(new ListPooledObjectPolicy<IEventSystemHandler>());

        public static bool Execute<T>(Entity target, BaseEventData eventData, EventFunction<T> functor)
            where T : IEventSystemHandler
        {
            var internalHandlers = handlerListPool.Get();
            GetEventList<T>(target, internalHandlers);

            for (var i = 0; i < internalHandlers.Count; i++)
            {
                T arg;
                try
                {
                    arg = (T)internalHandlers[i];
                }
                catch (Exception e)
                {
                    var temp = internalHandlers[i];
                    Trace.TraceError($"Type {typeof(T).Name} expected {temp.GetType().Name} received.\n{e}");
                    continue;
                }

                try
                {
                    functor(arg, eventData);
                }
                catch (Exception e)
                {
                    Trace.TraceError(e.Message);
                    Trace.TraceError(e.StackTrace);
                }
            }

            var handlerCount = internalHandlers.Count;
            handlerListPool.Return(internalHandlers);
            return handlerCount > 0;
        }

        /// <summary>
        /// Execute the specified event on the first game object underneath the current touch.
        /// </summary>
        private static readonly List<Entity> internalTransformList = new List<Entity>(30);

        public static Entity ExecuteHierarchy<T>(Entity root, BaseEventData eventData, EventFunction<T> callbackFunction)
            where T : IEventSystemHandler
        {
            GetEventChain(root, internalTransformList);

            for (var i = 0; i < internalTransformList.Count; i++)
            {
                var transform = internalTransformList[i];
                if (Execute(transform, eventData, callbackFunction))
                {
                    return transform;
                }
            }

            return null;
        }

        private static bool ShouldSendToComponent<T>(Component component)
            where T : IEventSystemHandler
        {
            var valid = component is T;
            if (!valid)
            {
                return false;
            }

            if (component is Behavior behavior)
            {
                return behavior.IsActivated;
            }

            return true;
        }

        /// <summary>
        /// Get the specified object's event event.
        /// </summary>
        private static void GetEventList<T>(Entity go, IList<IEventSystemHandler> results)
            where T : IEventSystemHandler
        {
            // Debug.LogWarning("GetEventList<" + typeof(T).Name + ">");
            if (results == null)
            {
                throw new ArgumentException("Results array is null", "results");
            }

            if (go == null || !go.IsActivated)
            {
                return;
            }

            var components = go.FindComponentsInChildren(typeof(T), false);

            foreach (Component component in components)
            {
                if (!ShouldSendToComponent<T>(component))
                {
                    continue;
                }

                results.Add(component as IEventSystemHandler);
            }
        }

        /// <summary>
        /// Whether the specified game object will be able to handle the specified event.
        /// </summary>
        /// <param name="go">The entity to test.</param>
        /// <typeparam name="T">The event to test.</typeparam>
        /// <returns>If the entity can handle this event.</returns>
        public static bool CanHandleEvent<T>(Entity go)
            where T : IEventSystemHandler
        {
            var internalHandlers = handlerListPool.Get();
            GetEventList<T>(go, internalHandlers);
            var handlerCount = internalHandlers.Count;
            handlerListPool.Return(internalHandlers);
            return handlerCount != 0;
        }

        /// <summary>
        /// Bubble the specified event on the game object, figuring out which object will actually receive the event.
        /// </summary>
        /// <typeparam name="T">The event to test.</typeparam>
        /// <param name="root">The root object to test.</param>
        /// <returns>The object that contains the event handler.</returns>
        public static Entity GetEventHandler<T>(Entity root)
            where T : IEventSystemHandler
        {
            if (root == null)
            {
                return null;
            }

            Entity current = root;
            while (current != null)
            {
                if (CanHandleEvent<T>(current))
                {
                    return current;
                }

                current = current.Parent;
            }

            return null;
        }
    }
}

#pragma warning restore SA1600 // Elements should be documented
