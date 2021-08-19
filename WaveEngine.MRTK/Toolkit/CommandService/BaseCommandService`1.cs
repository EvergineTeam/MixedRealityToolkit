// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
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
        /// <summary>
        /// Event raised when a new command is received.
        /// </summary>
        public event EventHandler<CommandEventArgs> NewCommandReceived;

        /// <summary>
        /// Send a new command request.
        /// </summary>
        /// <param name="command">The command to request.</param>
        /// <param name="parameter">The parameter for the request.</param>
        public void SendCommand(T command, object parameter = null)
        {
            this.NewCommandReceived?.Invoke(this, new CommandEventArgs(command, parameter));
        }
    }
}
