using Noesis;
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

        [RenderProperty(Tooltip = "The text that will be shown")]
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                if (this.text != value)
                {
                    this.text = value;
                    if (this.textBlock != null)
                    {
                        this.textBlock.Text = value;
                    }
                }
            }
        }

        [RenderProperty(Tooltip = "The font size used to render the text")]
        public int FontSize { get; set; } = 12;

        [RenderPropertyAsFInput(Tooltip = "The alpha value for the text component", MinLimit = 0, MaxLimit = 1)]
        public float Alpha { get; set; } = 1.0f;

        [RenderProperty(Tooltip = "The panel size")]
        public Vector2 Size { get; set; } = new Vector2(1, 0.5f);

        [RenderProperty(Tooltip = "The panel resolution")]
        public Vector2 Resolution { get; set; } = new Vector2(200, 100);

        [RenderPropertyAsFInput(Tooltip = "The panel tessellation quality", MinLimit = 0, MaxLimit = 1)]
        public float Quality { get; set; } = 0.5f;

        [RenderProperty(Tooltip = "The text horizontal alignment")]
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Center;

        private MaterialComponent materialComponent;
        private MeshRenderer meshRenderer;
        private PlaneMesh planeMesh;
        private NoesisFramebufferPanel noesisFramebufferPanel;

        private StandardMaterial standardMaterial;
        private FrameBuffer noesisFramebuffer;

        private FrameworkElement frameworkElement;
        private TextBlock textBlock;

        private string text = string.Empty;

        protected override bool OnAttached()
        {
            var attached = base.OnAttached();

            if (attached)
            {
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
                this.planeMesh.Width = this.Size.X;
                this.planeMesh.Height = this.Size.Y;
                this.noesisFramebufferPanel.Width = (uint)this.Resolution.X;
                this.noesisFramebufferPanel.Height = (uint)this.Resolution.Y;
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

                // Set material texture to use Noesis framebuffer texture
                this.standardMaterial.BaseColorTexture = this.noesisFramebufferPanel.FrameBuffer.ColorTargets[0].Texture;

                // Add container entity and components (bug workaround)
                var containerEntity = new Entity("components")
                {
                    Flags = HideFlags.DontSave,
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

            if (this.noesisFramebuffer != null)
            {
                this.noesisFramebuffer.Dispose();
                this.noesisFramebufferPanel.FrameBuffer = null;
                this.materialComponent.Material?.SetTexture(null, 0);
            }
        }

        protected virtual TextBlock BuildTextBlock()
        {
            return new TextBlock()
            {
                Foreground = Brushes.White,
                Background = Brushes.Transparent,
                HorizontalAlignment = this.HorizontalAlignment,
                VerticalAlignment = VerticalAlignment.Center,
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
    }
}
