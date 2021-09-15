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
using WaveEngine.MRTK.Services.InputSystem;

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

        [BindSceneManager]
        private FocusProvider focusProvider = null;

        /// <summary>
        /// The <see cref="Transform3D"/> component dependency.
        /// </summary>
        [BindComponent]
        protected Transform3D transform;

        private float historyElapsedTime;
        private List<float> gameTimeHistory = new List<float>(VELOCITY_HISTORY_SIZE);

        private Vector3 linearVelocity;
        private Quaternion angularVelocity;

        private Entity focusedEntity;
        private List<Entity> pointerFocusableEntities = new List<Entity>();

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
                if (this.pointerInteractedEntity != null)
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
            }
            else if (this.Pinch && this.pointerInteractedEntities.Count > 0)
            {
                // PointerDown when the cursor transitions to pinched while inside a collider
                this.pointerInteractedEntity = this.pointerInteractedEntities[0];
                this.RunPointerHandlers(this.pointerInteractedEntity, (h, e) => h?.OnPointerDown(e));
            }

            this.PreviousPinch = this.Pinch;

            // Update entity in focus
            Entity newFocusedEntity = null;
            var closestDistance = float.PositiveInfinity;
            for (int i = 0; i < this.pointerFocusableEntities.Count; i++)
            {
                var entity = this.pointerFocusableEntities[i];
                var distance = Vector3.DistanceSquared(this.transform.Position, entity.FindComponent<Transform3D>().Position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    newFocusedEntity = entity;
                }
            }

            this.UpdateFocusedEntity(newFocusedEntity);
        }

        private void UpdateFocusedEntity(Entity focusedEntity)
        {
            if (this.focusedEntity != focusedEntity)
            {
                this.focusProvider.FocusExit(this.focusedEntity, this);

                this.focusedEntity = focusedEntity;

                this.focusProvider.FocusEnter(this.focusedEntity, this);
            }
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

            other.RunOnComponents<IMixedRealityPointerHandler>((x) => action(x, eventArgs));
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
        /// Add an entity to the list of focusable objects for this cursor.
        /// </summary>
        /// <param name="focusedEntity">The entity to add.</param>
        protected void AddFocusableInteraction(Entity focusedEntity)
        {
            if (focusedEntity.HasEventHandlers<IMixedRealityFocusHandler>())
            {
                if (!this.pointerFocusableEntities.Contains(focusedEntity))
                {
                    this.pointerFocusableEntities.Add(focusedEntity);
                }
            }
        }

        /// <summary>
        /// Remove an entity from the list of focusable objects for this cursor.
        /// </summary>
        /// <param name="focusedEntity">The entity to remove.</param>
        protected void RemoveFocusableInteraction(Entity focusedEntity)
        {
            this.pointerFocusableEntities.Remove(focusedEntity);
        }

        /// <summary>
        /// Remove all entities from the list of focusable objects for this cursor.
        /// </summary>
        protected void RemoveAllFocusableInteractions()
        {
            this.pointerFocusableEntities.Clear();
        }
    }
}
