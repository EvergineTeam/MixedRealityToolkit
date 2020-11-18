// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Audio;
using WaveEngine.Components.Sound;
using WaveEngine.Framework;

namespace WaveEngine.MRTK.SDK.Features.UX.Components.BoundingBox
{
    /// <summary>
    /// Sound feedback component of a bounding box.
    /// </summary>
    public class BoundingBoxSoundFeedbackComponent : Component
    {
        /// <summary>
        /// The bounding box.
        /// </summary>
        [BindComponent]
        protected BoundingBox boundingBox;

        /// <summary>
        /// Gets or sets the sound to be played when the rotation is started.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the rotation is started")]
        public AudioBuffer RotateStartedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when the rotation is stopped.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the rotation is stopped")]
        public AudioBuffer RotateStoppedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when the scaling is started.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the scaling is started")]
        public AudioBuffer ScaleStartedSound { get; set; }

        /// <summary>
        /// Gets or sets the sound to be played when the scaling is stopped.
        /// </summary>
        [RenderProperty(Tooltip = "The sound to be played when the scaling is stopped")]
        public AudioBuffer ScaleStoppedSound { get; set; }

        private SoundEmitter3D soundEmitter;

        /// <inheritdoc/>
        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.boundingBox.RotateStarted += this.BoundingBox_RotateStarted;
                this.boundingBox.RotateStopped += this.BoundingBox_RotateStopped;
                this.boundingBox.ScaleStarted += this.BoundingBox_ScaleStarted;
                this.boundingBox.ScaleStopped += this.BoundingBox_ScaleStopped;

                if (!Application.Current.IsEditor)
                {
                    this.soundEmitter = this.Owner.GetOrAddComponent<SoundEmitter3D>();
                }
            }

            return attached;
        }

        /// <inheritdoc/>
        protected override void OnDetach()
        {
            base.OnDetach();

            this.boundingBox.RotateStarted -= this.BoundingBox_RotateStarted;
            this.boundingBox.RotateStopped -= this.BoundingBox_RotateStopped;
            this.boundingBox.ScaleStarted -= this.BoundingBox_ScaleStarted;
            this.boundingBox.ScaleStopped -= this.BoundingBox_ScaleStopped;
        }

        private void BoundingBox_RotateStarted(object sender, EventArgs e)
        {
            Tools.PlaySound(this.soundEmitter, this.RotateStartedSound);
        }

        private void BoundingBox_RotateStopped(object sender, EventArgs e)
        {
            Tools.PlaySound(this.soundEmitter, this.RotateStoppedSound);
        }

        private void BoundingBox_ScaleStarted(object sender, EventArgs e)
        {
            Tools.PlaySound(this.soundEmitter, this.ScaleStartedSound);
        }

        private void BoundingBox_ScaleStopped(object sender, EventArgs e)
        {
            Tools.PlaySound(this.soundEmitter, this.ScaleStoppedSound);
        }
    }
}
