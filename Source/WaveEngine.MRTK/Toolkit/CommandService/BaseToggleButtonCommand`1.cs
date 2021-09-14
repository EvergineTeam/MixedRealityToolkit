// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.MRTK.SDK.Features.UX.Components.ToggleButtons;

namespace WaveEngine.MRTK.Toolkit.CommandService
{
    /// <summary>
    /// Base class for adding support to a <see cref="ToggleButton"/> to make command requests to a <see cref="BaseCommandService{T}"/>/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Enum"/> containing the commands that can be requested.</typeparam>
    public class BaseToggleButtonCommand<T> : BaseCommandRequester<T>
        where T : Enum
    {
        /// <summary>
        /// The <see cref="ToggleButton"/> component that this component will react to.
        /// </summary>
        [BindComponent(source: BindComponentSource.Children)]
        protected ToggleButton toggleButton;

        /// <summary>
        /// Gets or sets the command that will be issued when the <see cref="ToggleButton"/> is toggled on.
        /// </summary>
        [RenderProperty(Tooltip = "The command that will be issued when the toggle button is toggled on.")]
        public T OnCommand { get; set; }

        /// <summary>
        /// Gets or sets the command that will be issued when the <see cref="ToggleButton"/> is toggled off.
        /// </summary>
        [RenderProperty(Tooltip = "The command that will be issued when the toggle button is toggled off.")]
        public T OffCommand { get; set; }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            this.toggleButton.Toggled += this.ToggleButton_Toggled;
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.toggleButton.Toggled -= this.ToggleButton_Toggled;
        }

        private void ToggleButton_Toggled(object sender, EventArgs e)
        {
            this.commandService?.ExecuteCommand(this, this.toggleButton.IsOn ? this.OnCommand : this.OffCommand);
        }
    }
}
