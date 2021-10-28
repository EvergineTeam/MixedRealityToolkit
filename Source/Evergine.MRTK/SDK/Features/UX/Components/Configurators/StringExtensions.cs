// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

namespace Evergine.MRTK.SDK.Features.UX.Components.Configurators
{
    internal static class StringExtensions
    {
        public const string SafeString = " ";

        /// <summary>
        /// Text3D crashes for zero length strings (i.e. null or empty string).
        /// This forces strings to at least have one empty space character.
        /// </summary>
        /// <param name="str">String to sanitize.</param>
        /// <returns>Sanitized string.</returns>
        public static string AsSafeStringForText3D(this string str) =>
            string.IsNullOrEmpty(str)
                ? SafeString
                : str;
    }
}
