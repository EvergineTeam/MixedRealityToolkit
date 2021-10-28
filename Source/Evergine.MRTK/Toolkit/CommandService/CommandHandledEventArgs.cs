// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;

namespace Evergine.MRTK.Toolkit.CommandService
{
    /// <summary>
    /// Event arguments received when a command is handled.
    /// </summary>
    public class CommandHandledEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the <see cref="object"/> that handled the command.
        /// </summary>
        public object Handler { get; private set; }

        /// <summary>
        /// Gets the requested command.
        /// </summary>
        public Enum Command { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandledEventArgs"/> class.
        /// </summary>
        /// <param name="handler">The object that handled the request.</param>
        /// <param name="command">The command.</param>
        public CommandHandledEventArgs(object handler, Enum command)
        {
            this.Handler = handler;
            this.Command = command;
        }
    }
}
