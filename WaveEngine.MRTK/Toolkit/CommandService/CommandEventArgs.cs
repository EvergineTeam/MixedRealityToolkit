// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;

namespace WaveEngine.MRTK.Toolkit.CommandService
{
    /// <summary>
    /// Event arguments received when a command is requested.
    /// </summary>
    public class CommandEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the requested command.
        /// </summary>
        public Enum Command { get; private set; }

        /// <summary>
        /// Gets the parameter for the request.
        /// </summary>
        public object Parameter { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandEventArgs"/> class.
        /// </summary>
        /// <param name="command">The requested command.</param>
        /// <param name="parameter">The parameter for the request.</param>
        public CommandEventArgs(Enum command, object parameter)
        {
            this.Command = command;
            this.Parameter = parameter;
        }
    }
}
