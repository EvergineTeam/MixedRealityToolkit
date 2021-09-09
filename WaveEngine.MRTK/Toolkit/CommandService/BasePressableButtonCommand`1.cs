// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Framework;
using WaveEngine.MRTK.SDK.Features.UX.Components.PressableButtons;

namespace WaveEngine.MRTK.Toolkit.CommandService
{
    /// <summary>
    /// Base class for adding support to a <see cref="PressableButton"/> to make command requests to a <see cref="BaseCommandService{T}"/>/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Enum"/> containing the commands that can be requested.</typeparam>
    public class BasePressableButtonCommand<T> : BaseCommandRequester<T>
        where T : Enum
    {
        /// <summary>
        /// The <see cref="PressableButton"/> component that this component will react to.
        /// </summary>
        [BindComponent(source: BindComponentSource.Children)]
        protected PressableButton pressableButton;

        /// <summary>
        /// Gets or sets the command that will be issued when the button is pressed.
        /// </summary>
        [RenderProperty(Tooltip = "The command that will be issued when the button is pressed.")]
        public T Command { get; set; }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            this.pressableButton.ButtonReleased += this.PressableButton_ButtonReleased;
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            this.pressableButton.ButtonReleased -= this.PressableButton_ButtonReleased;
        }

        private void PressableButton_ButtonReleased(object sender, EventArgs e)
        {
            this.commandService.ExecuteCommand(this, this.Command);
        }
    }
}
