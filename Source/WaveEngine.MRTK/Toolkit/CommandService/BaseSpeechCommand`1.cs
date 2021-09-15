// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.MRTK.SDK.Features.Input.Handlers;

namespace WaveEngine.MRTK.Toolkit.CommandService
{
    /// <summary>
    /// <see cref="SpeechHandler"/> that adds support for making command requests to a <see cref="BaseCommandService{T}"/>/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Enum"/> containing the commands that can be requested.</typeparam>
    public class BaseSpeechCommand<T> : SpeechHandler
        where T : Enum
    {
        /// <summary>
        /// The command service.
        /// </summary>
        [BindService]
        protected BaseCommandService<T> commandService;

        /// <summary>
        /// Gets or sets the command that will be issued when the command is recognized.
        /// </summary>
        [RenderProperty(Tooltip = "The command that will be issued when the command is recognized.")]
        public T Command { get; set; }

        /// <inheritdoc/>
        protected override void InternalOnSpeechKeywordRecognized(string keyword)
        {
            this.commandService?.ExecuteCommand(this, this.Command);
        }
    }
}
