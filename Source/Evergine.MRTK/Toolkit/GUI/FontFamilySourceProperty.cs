// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;
using Evergine.Common.Attributes;

namespace Evergine.MRTK.Toolkit.GUI
{
    /// <summary>
    /// A property to hold a font family source to be used in a component.
    /// </summary>
    public class FontFamilySourceProperty
    {
        /// <summary>
        /// Gets or sets the font family source to use.
        /// </summary>
        [RenderProperty(Tooltip = "The font family source to use.")]
        public string FontFamilySource
        {
            get => this.fontFamilySource;
            set
            {
                if (this.fontFamilySource != value)
                {
                    this.fontFamilySource = value;
                    this.Refresh();
                }
            }
        }

        private string fontFamilySource;

        /// <summary>
        /// Gets a value indicating whether the font family source is set and valid.
        /// </summary>
        public bool IsFontFamilySourceValid => !string.IsNullOrEmpty(this.fontFamilySource);

        /// <summary>
        /// An event that will raise when the font family source changes.
        /// </summary>
        public event EventHandler OnFontFamilySourceChanged;

        /// <summary>
        /// Raise the changed event.
        /// </summary>
        public void Refresh()
        {
            this.OnFontFamilySourceChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets <see cref="Noesis.FontFamily"/> instance from source.
        /// </summary>
        /// <returns>Font family.</returns>
        public Noesis.FontFamily GetFontFamily()
        {
            return this.IsFontFamilySourceValid ? new Noesis.FontFamily(this.fontFamilySource) : null;
        }
    }
}
