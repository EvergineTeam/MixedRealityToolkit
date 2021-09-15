// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using WaveEngine.Framework;

namespace WaveEngine.MRTK.Toolkit.CommandService
{
    /// <summary>
    /// Base class for adding support to make command requests to a <see cref="BaseCommandService{T}"/>/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Enum"/> containing the commands that can be requested.</typeparam>
    public class BaseCommandRequester<T> : Component
        where T : Enum
    {
        /// <summary>
        /// The command service.
        /// </summary>
        [BindService(isRequired: false)]
        protected BaseCommandService<T> commandService;
    }
}
