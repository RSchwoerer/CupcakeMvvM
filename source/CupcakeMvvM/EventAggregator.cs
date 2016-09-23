﻿using CupcakeMvvM.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CupcakeMvvM
{
    /// <summary>
    /// Enables loosely-coupled publication of and subscription to events.
    /// </summary>
    public class EventAggregator : IEventAggregator
    {
        /// <summary>
        /// Processing of handler results on publication thread.
        /// </summary>
        public static Action<object, object> HandlerResultProcessing = (target, result) => { };

        private readonly List<Handler> handlers = new List<Handler>();

        static EventAggregator()
        {
            Instance = new EventAggregator();
        }

        private EventAggregator()
        { }

        public static EventAggregator Instance { get; } = null;

        /// <summary>
        /// Searches the subscribed handlers to check if we have a handler for
        /// the message type supplied.
        /// </summary>
        /// <param name="messageType">The message type to check with</param>
        /// <returns>True if any handler is found, false if not.</returns>
        public bool HandlerExistsFor(Type messageType)
        {
            return handlers.Any(handler => handler.Handles(messageType) & !handler.IsDead);
        }

        /// <summary>
        /// Publishes a message.
        /// </summary>
        /// <param name = "message">The message instance.</param>
        /// <param name = "marshal">Allows the publisher to provide a custom thread marshaller for the message publication.</param>
        public virtual void Publish(object message, Action<System.Action> marshal)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            if (marshal == null)
            {
                throw new ArgumentNullException(nameof(marshal));
            }

            Handler[] toNotify;
            lock (handlers)
            {
                toNotify = handlers.ToArray();
            }

            marshal(() =>
            {
                var messageType = message.GetType();

                var dead = toNotify
                    .Where(handler => !handler.Handle(messageType, message))
                    .ToList();

                if (dead.Any())
                {
                    lock (handlers)
                    {
                        dead.Apply(x => handlers.Remove(x));
                    }
                }
            });
        }

        /// <summary>
        /// Subscribes an instance to all events declared through implementations of <see cref = "IHandle{T}" />
        /// </summary>
        /// <param name = "subscriber">The instance to subscribe for event publication.</param>
        public virtual void Subscribe(object subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }
            lock (handlers)
            {
                if (handlers.Any(x => x.Matches(subscriber)))
                {
                    return;
                }

                handlers.Add(new Handler(subscriber));
            }
        }

        /// <summary>
        /// Unsubscribes the instance from all events.
        /// </summary>
        /// <param name = "subscriber">The instance to unsubscribe.</param>
        public virtual void Unsubscribe(object subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException(nameof(subscriber));
            }
            lock (handlers)
            {
                var found = handlers.FirstOrDefault(x => x.Matches(subscriber));

                if (found != null)
                {
                    handlers.Remove(found);
                }
            }
        }

        private class Handler
        {
            private readonly WeakReference reference;
            private readonly Dictionary<Type, MethodInfo> supportedHandlers = new Dictionary<Type, MethodInfo>();

            public Handler(object handler)
            {
                reference = new WeakReference(handler);

                var interfaces = handler.GetType().GetInterfaces()
                    .Where(x => typeof(IHandle).IsAssignableFrom(x) && x.IsGenericType());

                foreach (var @interface in interfaces)
                {
                    var type = @interface.GetGenericArguments()[0];
                    var method = @interface.GetMethod("Handle", new Type[] { type });

                    if (method != null)
                    {
                        supportedHandlers[type] = method;
                    }
                }
            }

            public bool IsDead => reference.Target == null;

            public bool Handle(Type messageType, object message)
            {
                var target = reference.Target;
                if (target == null)
                {
                    return false;
                }

                foreach (var pair in supportedHandlers)
                {
                    if (pair.Key.IsAssignableFrom(messageType))
                    {
                        var result = pair.Value.Invoke(target, new[] { message });
                        if (result != null)
                        {
                            HandlerResultProcessing(target, result);
                        }
                    }
                }

                return true;
            }

            public bool Handles(Type messageType) => supportedHandlers.Any(pair => pair.Key.IsAssignableFrom(messageType));

            public bool Matches(object instance) => reference.Target == instance;
        }
    }
}