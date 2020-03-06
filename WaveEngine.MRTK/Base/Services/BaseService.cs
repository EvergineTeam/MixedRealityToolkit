﻿// Copyright © 2019 Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;

namespace WaveEngine.MixedReality.Toolkit
{
    /// <summary>
    /// The base service implements <see cref="IMixedRealityService"/> and provides default properties for all services.
    /// </summary>
    public abstract class BaseService : IMixedRealityService
    {
        /// <summary>
        /// The default priority.
        /// </summary>
        public const uint DefaultPriority = 10;

        #region IMixedRealityService Implementation

        /// <inheritdoc />
        public virtual string Name { get; protected set; }

        /// <inheritdoc />
        public virtual uint Priority { get; protected set; } = DefaultPriority;

        /// <inheritdoc />
        public virtual void Initialize()
        {
        }

        /// <inheritdoc />
        public virtual void Reset()
        {
        }

        /// <inheritdoc />
        public virtual void Activate()
        {
        }

        /// <inheritdoc />
        public virtual void Update()
        {
        }

        /// <inheritdoc />
        public virtual void Deactivate()
        {
        }

        /// <inheritdoc />
        public virtual void Detach()
        {
        }

        /// <inheritdoc />
        public virtual void Destroy()
        {
        }

        #endregion IMixedRealityService Implementation

        #region IDisposable Implementation

        /// <summary>
        /// Value indicating if the object has completed disposal.
        /// </summary>
        /// <remarks>
        /// Set by derived classes to indicate that disposal has been completed.
        /// </remarks>
        protected bool disposed = false;

        /// <summary>
        /// Finalizes an instance of the <see cref="BaseService"/> class.
        /// </summary>
        ~BaseService()
        {
            this.Dispose();
        }

        /// <summary>
        /// Cleanup resources used by this object.
        /// </summary>
        public void Dispose()
        {
            // Clean up our resources (managed and unmanaged resources)
            this.Dispose(true);

            // Suppress finalization as the finalizer also calls our cleanup code.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Cleanup resources used by the object.
        /// </summary>
        /// <param name="disposing">Are we fully disposing the object?
        /// True will release all managed resources, unmanaged resources are always released.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

        #endregion IDisposable Implementation
    }
}