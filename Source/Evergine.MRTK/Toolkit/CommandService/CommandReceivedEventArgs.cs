// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Framework;

namespace Evergine.MRTK.Toolkit.CommandService
{
    /// <summary>
    /// Event arguments received when a command is requested.
    /// </summary>
    public class CommandReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="Component"/> that made the request.
        /// </summary>
        public Component Source { get; private set; }

        /// <summary>
        /// Gets the requested command.
        /// </summary>
        public Enum Command { get; private set; }

        /// <summary>
        /// Gets the parameters for the request.
        /// </summary>
        public object[] Parameter { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this event has been marked as handled.
        /// </summary>
        public bool Handled { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="source">The component that requested the command.</param>
        /// <param name="command">The requested command.</param>
        /// <param name="parameter">The parameter for the request.</param>
        public CommandReceivedEventArgs(Component source, Enum command, object[] parameter)
        {
            this.Source = source;
            this.Command = command;
            this.Parameter = parameter;
        }

        /// <summary>
        /// Mark the event as handled.
        /// </summary>
        public void SetHandled()
        {
            this.Handled = true;
        }
    }
}
