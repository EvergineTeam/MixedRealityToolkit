using Noesis;
using System;
using System.Collections.Generic;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Effects;
using WaveEngine.Framework.Graphics.Materials;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;
using WaveEngine.NoesisGUI;

namespace WaveEngine_MRTK_Demo.Components
{
    public class Text3D : Component
    {
        [BindService]
        protected GraphicsContext graphicsContext;

        [BindService]
        protected AssetsService assetsService;

        [BindComponent]
        protected Transform3D transform;

        [RenderProperty(Tooltip = "The text that will be shown")]
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (this.SetProperty(ref this.text, value, this.textBlock))
                {
                    this.textBlock.Text = value;
                };
            }
        }
        private string text = string.Empty;

        [RenderProperty(Tooltip = "The font size used to render the text")]
        public int FontSize
        {
            get
            {
                return this.fontSize;
            }
            set
            {
                if (this.SetProperty(ref this.fontSize, value, this.textBlock))
                {
                    this.textBlock.FontSize = value;
                }
            }
        }
        private int fontSize = 12;

        [RenderPropertyAsFInput(Tooltip = "The alpha value for the text component", MinLimit = 0, MaxLimit = 1)]
        public float Alpha
        {
            get
            {
                return this.alpha;
            }
            set
            {
                if (this.SetProperty(ref this.alpha, value, this.standardMaterial))
                {
                    this.standardMaterial.Alpha = value;
                }
            }
        }
        private float alpha = 1.0f;

        [RenderProperty(Tooltip = "The panel resolution per unit of Scale. The panel resolution Width and Height are affected by the values of Scale X and Z components respectively")]
        public int Resolution
        {
            get
            {
                return this.resolution;
            }
            set
            {
                if (this.SetProperty(ref this.resolution, value, this.noesisFramebufferPanel))
                {
                    this.UpdateNoesisPanelFramebuffer();
                }
            }
        }
        private int resolution = 1000;

        [RenderPropertyAsFInput(Tooltip = "The panel tessellation quality", MinLimit = 0, MaxLimit = 1)]
        public float Quality { get; set; } = 0.5f;

        [RenderProperty(Tooltip = "The text horizontal alignment")]
        public HorizontalAlignment HorizontalAlignment
        {
            get
            {
                return this.horizontalAlignment;
            }
            set
            {
                if (this.SetProperty(ref this.horizontalAlignment, value, this.textBlock))
                {
                    this.textBlock.HorizontalAlignment = value;
                }
            }
        }
        private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;

        [RenderProperty(Tooltip = "The text vertical alignment")]
        public VerticalAlignment VerticalAlignment
        {
            get
            {
                return this.verticalAlignment;
            }
            set
            {
                if (this.SetProperty(ref this.verticalAlignment, value, this.textBlock))
                {
                    this.textBlock.VerticalAlignment = value;
                }
            }
        }
        private VerticalAlignment verticalAlignment = VerticalAlignment.Center;

        [RenderProperty(Tooltip = "The text wrapping to use")]
        public TextWrapping TextWrapping
        {
            get
            {
                return this.textWrapping;
            }
            set
            {
                if (this.SetProperty(ref this.textWrapping, value, this.textBlock))
                {
                    this.textBlock.TextWrapping = value;
                }
            }
        }
        private TextWrapping textWrapping = TextWrapping.NoWrap;

        private MaterialComponent materialComponent;
        private MeshRenderer meshRenderer;
        private PlaneMesh planeMesh;
        private NoesisFramebufferPanel noesisFramebufferPanel;

        private StandardMaterial standardMaterial;
        private FrameBuffer noesisFramebuffer;

        private FrameworkElement frameworkElement;
        private TextBlock textBlock;

        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
                this.transform.ScaleChanged += this.Transform_ScaleChanged;

                // Add components to owner entity
                this.materialComponent = new MaterialComponent();

                this.meshRenderer = new MeshRenderer();

                this.planeMesh = new PlaneMesh();

                this.noesisFramebufferPanel = new NoesisFramebufferPanel()
                {
                    BackgroundColor = WaveEngine.Common.Graphics.Color.Transparent,
                    PPAAEnabled = true,
                    EnableKeyboard = false,
                    EnableMouse = false,
                    EnableTouch = false,
                };

                // Set property values
                this.planeMesh.Width = 1f;
                this.planeMesh.Height = 1f;
                this.noesisFramebufferPanel.TessellationQuality = this.Quality;

                // Build FrameworkElement
                this.textBlock = this.BuildTextBlock();
                this.textBlock.Text = this.Text;
                this.textBlock.FontSize = this.FontSize;
                this.frameworkElement = this.BuildFrameworkElement();

                this.noesisFramebufferPanel.FrameworkElement = this.frameworkElement;

                // Noesis framebuffer configuration
                if (this.noesisFramebufferPanel.FrameBuffer == null)
                {
                    this.noesisFramebuffer = this.graphicsContext.Factory.CreateFrameBuffer(this.noesisFramebufferPanel.Width, this.noesisFramebufferPanel.Height);
                    this.noesisFramebufferPanel.FrameBuffer = this.noesisFramebuffer;
                }

                // Material initialization
                if (this.materialComponent.Material == null)
                {
                    var effect = this.assetsService.Load<Effect>(DefaultResourcesIDs.StandardEffectID);
                    this.standardMaterial = new StandardMaterial(effect)
                    {
                        Id = this.Id,
                        LightingEnabled = false,
                        IBLEnabled = false,
                        LayerDescription = this.assetsService.Load<RenderLayerDescription>(DefaultResourcesIDs.AlphaRenderLayerID)
                    };

                    this.materialComponent.Material = this.standardMaterial.Material;
                }
                else
                {
                    this.standardMaterial = new StandardMaterial(this.materialComponent.Material);
                }

                this.standardMaterial.Alpha = this.Alpha;

                // Update Noesis panel framebuffer
                this.UpdateNoesisPanelFramebuffer();

                // Add container entity and components (bug workaround)
                var containerEntity = new Entity("components")
                {
                    Flags = HideFlags.DontSave | HideFlags.DontShow,
                }
                .AddComponent(new Transform3D())
                .AddComponent(this.materialComponent)
                .AddComponent(this.meshRenderer)
                .AddComponent(this.planeMesh)
                .AddComponent(this.noesisFramebufferPanel)
                ;

                this.Owner.AddChild(containerEntity);
            }

            return attached;
        }

        protected override void OnDetach()
        {
            base.OnDetach();

            this.transform.ScaleChanged -= this.Transform_ScaleChanged;

            if (this.noesisFramebuffer != null)
            {
                this.noesisFramebuffer.Dispose();
                this.noesisFramebufferPanel.FrameBuffer = null;
                this.materialComponent.Material?.SetTexture(null, 0);
            }
        }

        private void Transform_ScaleChanged(object sender, System.EventArgs e)
        {
            this.UpdateNoesisPanelFramebuffer();
        }

        protected virtual TextBlock BuildTextBlock()
        {
            return new TextBlock()
            {
                Foreground = Brushes.White,
                Background = Brushes.Transparent,
                HorizontalAlignment = this.HorizontalAlignment,
                VerticalAlignment = this.VerticalAlignment,
                TextWrapping = this.TextWrapping,
            };
        }

        protected virtual FrameworkElement BuildFrameworkElement()
        {
            var frameworkElement = new Grid()
            {
                Background = Brushes.Transparent,
            };

            frameworkElement.Children.Add(this.textBlock);

            return frameworkElement;
        }

        private void UpdateNoesisPanelFramebuffer()
        {
            this.noesisFramebufferPanel.Width = Math.Max(1, (uint)(this.Resolution * this.transform.Scale.X));
            this.noesisFramebufferPanel.Height = Math.Max(1, (uint)(this.Resolution * this.transform.Scale.Z));

            if (this.noesisFramebufferPanel.FrameBuffer != null)
            {
                this.noesisFramebuffer.Dispose();
                this.noesisFramebuffer = null;
                this.noesisFramebufferPanel.FrameBuffer = null;
            }

            if (this.noesisFramebufferPanel.FrameBuffer == null)
            {
                this.noesisFramebuffer = this.graphicsContext.Factory.CreateFrameBuffer(this.noesisFramebufferPanel.Width, this.noesisFramebufferPanel.Height);
                this.noesisFramebufferPanel.FrameBuffer = this.noesisFramebuffer;
            }

            // Set material texture to use Noesis framebuffer texture
            this.standardMaterial.BaseColorTexture = this.noesisFramebufferPanel.FrameBuffer.ColorTargets[0].Texture;
        }

        private bool SetProperty<T>(ref T property, T value, object checkNotNull)
        {
            if (!EqualityComparer<T>.Default.Equals(property, value))
            {
                property = value;

                return checkNotNull != null;
            }

            return false;
        }
    }
}
