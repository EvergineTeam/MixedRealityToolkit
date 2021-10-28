// Copyright © Evergine S.L. All rights reserved. Use is subject to license terms.

using System;

namespace Evergine.MRTK.SDK.Features.UX.Components.AxisManipulationHandler
{
    /// <summary>
    /// A manipulation handler that restricts movement to a combination of axes.
    /// </summary>
    public partial class AxisManipulationHandler
    {
        /// <summary>
        /// Event fired when the user starts a center manipulation.
        /// </summary>
        public event EventHandler CenterManipulationStarted;

        /// <summary>
        /// Event fired when the user ends a center manipulation.
        /// </summary>
        public event EventHandler CenterManipulationEnded;

        /// <summary>
        /// Event fired when the user starts an axis manipulation.
        /// </summary>
        public event EventHandler AxisManipulationStarted;

        /// <summary>
        /// Event fired when the user ends an axis manipulation.
        /// </summary>
        public event EventHandler AxisManipulationEnded;

        /// <summary>
        /// Event fired when the user starts a plane manipulation.
        /// </summary>
        public event EventHandler PlaneManipulationStarted;

        /// <summary>
        /// Event fired when the user ends a plane manipulation.
        /// </summary>
        public event EventHandler PlaneManipulationEnded;

        private void FireManipulationEvent(AxisManipulationHelperType amhType, bool started)
        {
            switch (amhType)
            {
                case AxisManipulationHelperType.Center:
                    if (started)
                    {
                        this.CenterManipulationStarted?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        this.CenterManipulationEnded?.Invoke(this, EventArgs.Empty);
                    }

                    break;
                case AxisManipulationHelperType.Axis:
                    if (started)
                    {
                        this.AxisManipulationStarted?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        this.AxisManipulationEnded?.Invoke(this, EventArgs.Empty);
                    }

                    break;
                case AxisManipulationHelperType.Plane:
                    if (started)
                    {
                        this.PlaneManipulationStarted?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        this.PlaneManipulationEnded?.Invoke(this, EventArgs.Empty);
                    }

                    break;
            }
        }
    }
}
