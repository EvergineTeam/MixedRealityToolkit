// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using WaveEngine.Framework;
using WaveEngine.Framework.Services;

namespace WaveEngine.MRTK.Toolkit.CommandService
{
    /// <summary>
    /// Base class for creating a command service.
    /// </summary>
    /// <typeparam name="T">The <see cref="Enum"/> containing the commands that can be requested.</typeparam>
    public abstract class BaseCommandService<T> : Service
        where T : Enum
    {
        private List<EventHandler<CommandReceivedEventArgs>> CommandReceivedEventHandlers = new List<EventHandler<CommandReceivedEventArgs>>();

        /// <summary>
        /// Event raised when a command is received.
        /// </summary>
        public event EventHandler<CommandReceivedEventArgs> CommandReceived
        {
            add => this.CommandReceivedEventHandlers.Add(value);
            remove => this.CommandReceivedEventHandlers.Remove(value);
        }

        /// <summary>
        /// Event raised when a command is handled.
        /// </summary>
        public event EventHandler<CommandHandledEventArgs> CommandHandled;

        /// <summary>
        /// Execute a command request.
        /// </summary>
        /// <param name="source">The component that requested the command.</param>
        /// <param name="command">The command to request.</param>
        /// <param name="parameter">The parameter for the request.</param>
        /// <returns>
        /// <see langword="true"/> if the command was handled; otherwise, <see langword="false"/>.
        /// </returns>
        public bool ExecuteCommand(Component source, T command, object[] parameter = null)
        {
            var args = new CommandReceivedEventArgs(source, command, parameter);

            foreach (var handler in this.CommandReceivedEventHandlers)
            {
                handler.Invoke(this, args);

                if (args.Handled)
                {
                    var handledArgs = new CommandHandledEventArgs(handler.Target, command);
                    this.CommandHandled?.Invoke(this, handledArgs);
                    break;
                }
            }

            return args.Handled;
        }
    }
}
