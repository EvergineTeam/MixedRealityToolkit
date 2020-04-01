using System;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Materials;
using WaveEngine.Framework.Physics3D;

namespace WaveEngine_MRTK_Demo.Emulation
{
    public class Cursor : Behavior
    {
        [BindComponent]
        public Transform3D transform = null;

        [BindComponent(isExactType: false)]
        public Collider3D Collider3D;

        [BindComponent]
        public StaticBody3D StaticBody3D;

        [BindComponent(isRequired: false)]
        protected MaterialComponent materialComponent;

        [RenderProperty(Tooltip = "The color to be set to the material when the cursor is pressed")]
        public Color PressedColor { get; set; }

        [RenderProperty(Tooltip = "The color to be set to the material when the cursor is released")]
        public Color ReleasedColor { get; set; }

        private bool pinch;
        [WaveIgnore]
        [DontRenderProperty]
        public bool Pinch
        {
            get
            {
                return pinch;
            }

            set
            {
                if (value != pinch)
                {
                    pinch = value;
                    this.UpdateColor();
                }
            }
        }

        [WaveIgnore]
        [DontRenderProperty]
        public bool PreviousPinch { get; private set; }

        private StandardMaterial material;

        protected override bool OnAttached()
        {
            return base.OnAttached();
        }

        protected override void Start()
        {
            if (this.materialComponent != null)
            {
                if (!Application.Current.IsEditor)
                {
                    this.materialComponent.Material = this.materialComponent.Material.Clone();

                    this.Managers.FindManager<CursorManager>().AddCursor(this);
                }

                this.material = new StandardMaterial(this.materialComponent.Material);
            }
        }

        protected override void Update(TimeSpan gameTime)
        {
            this.PreviousPinch = this.Pinch;
        }

        private void UpdateColor()
        {
            if (this.material != null)
            {
                this.material.BaseColor = this.Pinch ? this.PressedColor : this.ReleasedColor;
            }
        }
    }
}
