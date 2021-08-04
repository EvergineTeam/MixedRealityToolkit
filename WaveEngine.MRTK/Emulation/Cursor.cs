// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using System;
using System.Collections.Generic;
using WaveEngine.Common.Attributes;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Base.EventDatum.Input;
using WaveEngine.MRTK.Base.Interfaces.InputSystem.Handlers;
using WaveEngine.MRTK.Extensions;

namespace WaveEngine.MRTK.Emulation
{
    /// <summary>
    /// Visible cursor in scene.
    /// </summary>
    public abstract class Cursor : Behavior
    {
        private static readonly int VELOCITY_HISTORY_SIZE = 10;

        private static readonly List<Cursor> activeCursors = new List<Cursor>();

        /// <summary>
        /// Gets the currently active cursors.
        /// </summary>
        public static IEnumerable<Cursor> ActiveCursors => activeCursors;

        /// <summary>
        /// The <see cref="Transform3D"/> component dependency.
        /// </summary>
        [BindComponent]
        protected Transform3D transform;

        private float historyElapsedTime;
        private List<float> gameTimeHistory = new List<float>(VELOCITY_HISTORY_SIZE);

        private Vector3 linearVelocity;
        private Quaternion angularVelocity;

        /// <summary>
        /// This <see cref="Cursor"/>'s position history list.
        /// </summary>
        protected readonly List<Vector3> PositionHistory = new List<Vector3>(VELOCITY_HISTORY_SIZE);

        /// <summary>
        /// This <see cref="Cursor"/>'s orientation history list.
        /// </summary>
        protected readonly List<Quaternion> OrientationHistory = new List<Quaternion>(VELOCITY_HISTORY_SIZE);

        private List<Entity> pointerInteractedEntities = new List<Entity>();
        private Entity pointerInteractedEntity;

        /// <summary>
        /// The Material component.
        /// </summary>
        [BindComponent(isRequired: false, source: BindComponentSource.Children)]
        protected MaterialComponent materialComponent;

        /// <summary>
        /// Gets or sets the material when the cursor is pressed.
        /// </summary>
        [RenderProperty(Tooltip = "The material to be set when the cursor is pressed")]
        public Material PressedMaterial { get; set; }

        /// <summary>
        /// Gets or sets the material when the cursor is released.
        /// </summary>
        [RenderProperty(Tooltip = "The material to be set when the cursor is released")]
        public Material ReleasedMaterial { get; set; }

        private bool pinch;

        /// <summary>
        /// Gets or sets a value indicating whether the cursor is pinched.
        /// </summary>
        [WaveIgnore]
        [DontRenderProperty]
        public bool Pinch
        {
            get
            {
                return this.pinch;
            }

            set
            {
                if (this.pinch != value)
                {
                    this.pinch = value;
                    this.UpdateColor();
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether gets a value indicating the previous pinch.
        /// </summary>
        [WaveIgnore]
        [DontRenderProperty]
        public bool PreviousPinch { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether the cursor visible.
        /// </summary>
        [WaveIgnore]
        public bool IsVisible
        {
            get => this.materialComponent?.Owner.IsEnabled == true;
            set
            {
                if (this.materialComponent != null)
                {
                    this.materialComponent.Owner.IsEnabled = value;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnActivated()
        {
            base.OnActivated();

            this.UpdateColor();

            activeCursors.Add(this);
        }

        /// <inheritdoc/>
        protected override void OnDeactivated()
        {
            base.OnDeactivated();

            if (this.pointerInteractedEntity != null)
            {
                this.RunPointerHandlers(this.pointerInteractedEntity, (h, e) => h?.OnPointerUp(e));
                this.pointerInteractedEntity = null;
            }

            this.historyElapsedTime = 0;
            this.gameTimeHistory.Clear();
            this.PositionHistory.Clear();
            this.OrientationHistory.Clear();
            this.pointerInteractedEntities.Clear();

            activeCursors.Remove(this);
        }

        /// <inheritdoc/>
        protected override void Update(TimeSpan gameTime)
        {
            // Update gameTime history and compute history elapsed time
            var elapsed = (float)gameTime.TotalSeconds;
            this.gameTimeHistory.Add(elapsed);
            this.historyElapsedTime += elapsed;

            if (this.gameTimeHistory.Count > VELOCITY_HISTORY_SIZE)
            {
                this.historyElapsedTime -= this.gameTimeHistory[0];
                this.gameTimeHistory.RemoveAt(0);
            }

            // Update cursor velocity
            this.AddToHistoryList(this.PositionHistory, this.transform.Position);
            this.AddToHistoryList(this.OrientationHistory, this.transform.Orientation);

            var linearVelocity = (this.PositionHistory[this.PositionHistory.Count - 1] - this.PositionHistory[0]) / this.historyElapsedTime;
            var angularVelocity = this.OrientationHistory[this.OrientationHistory.Count - 1] * Quaternion.Inverse(this.OrientationHistory[0]) * (1 / this.historyElapsedTime);

            this.linearVelocity = linearVelocity;
            this.angularVelocity = angularVelocity;

            if (this.PreviousPinch)
            {
                if (this.Pinch)
                {
                    // PointerDragged while the cursor is pinched
                    this.RunPointerHandlers(this.pointerInteractedEntity, (h, e) => h?.OnPointerDragged(e));
                }
                else
                {
                    // PointerUp when the cursor is unpinched
                    this.RunPointerHandlers(this.pointerInteractedEntity, (h, e) => h?.OnPointerUp(e));
                    this.pointerInteractedEntity = null;
                }
            }
            else if (this.Pinch && this.pointerInteractedEntities.Count > 0)
            {
                // PointerDown when the cursor transitions to pinched while inside a collider
                this.pointerInteractedEntity = this.pointerInteractedEntities[0];
                this.RunPointerHandlers(this.pointerInteractedEntity, (h, e) => h?.OnPointerDown(e));
            }

            this.PreviousPinch = this.Pinch;
        }

        private void UpdateColor()
        {
            if (this.materialComponent == null)
            {
                return;
            }

            this.materialComponent.Material = this.Pinch ? this.PressedMaterial : this.ReleasedMaterial;
        }

        private void AddToHistoryList<T>(List<T> list, T newItem)
        {
            list.Add(newItem);

            if (list.Count > VELOCITY_HISTORY_SIZE)
            {
                list.RemoveAt(0);
            }
        }

        private void RunPointerHandlers(Entity other, Action<IMixedRealityPointerHandler, MixedRealityPointerEventData> action)
        {
            var eventArgs = new MixedRealityPointerEventData()
            {
                Cursor = this,
                CurrentTarget = other,
                Position = this.transform.Position,
                Orientation = this.transform.Orientation,
                LinearVelocity = this.linearVelocity,
                AngularVelocity = this.angularVelocity,
            };

            this.RunOnComponents<IMixedRealityPointerHandler>(other, (x) => action(x, eventArgs));
        }

        /// <summary>
        /// Adds a pointer interacted entity.
        /// </summary>
        /// <param name="interactedEntity">The pointer interacted entity.</param>
        protected void AddPointerInteraction(Entity interactedEntity)
        {
            this.pointerInteractedEntities.Insert(0, interactedEntity);
        }

        /// <summary>
        /// Remove a pointer interacted entity.
        /// </summary>
        /// <param name="interactedEntity">The pointer interacted entity.</param>
        protected void RemovePointerInteraction(Entity interactedEntity)
        {
            this.pointerInteractedEntities.Remove(interactedEntity);
        }

        /// <summary>
        /// Runs the specified action for the components from the given entity implementing the <typeparamref name="T"/> type.
        /// </summary>
        /// <typeparam name="T">The component type of interface.</typeparam>
        /// <param name="entity">The entity used to find the components.</param>
        /// <param name="action">The action callback to be invoked on every <typeparamref name="T"/> component.</param>
        protected void RunOnComponents<T>(Entity entity, Action<T> action)
            where T : IMixedRealityEventHandler
        {
            foreach (var interactable in entity.FindEventHandlers<T>())
            {
                action(interactable);
            }
        }
    }
}
