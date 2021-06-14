// Copyright © Wave Engine S.L. All rights reserved. Use is subject to license terms.

using Noesis;
using System;
using System.Collections.Generic;
using WaveEngine.Common.Attributes;
using WaveEngine.Common.Graphics;
using WaveEngine.Components.Graphics3D;
using WaveEngine.Framework;
using WaveEngine.Framework.Graphics;
using WaveEngine.Framework.Graphics.Effects;
using WaveEngine.Framework.Services;
using WaveEngine.Mathematics;
using WaveEngine.MRTK.Effects;
using WaveEngine.NoesisGUI;
using static WaveEngine.Components.Graphics3D.PlaneMesh;
using Color = WaveEngine.Common.Graphics.Color;
using Vector3 = WaveEngine.Mathematics.Vector3;

namespace WaveEngine.MRTK.Toolkit.GUI
{
    /// <summary>
    /// A component that loads 3D text.
    /// </summary>
    public class Text3D : Behavior
    {
        /// <summary>
        /// Graphics context.
        /// </summary>
        [BindService]
        protected GraphicsContext graphicsContext;

        /// <summary>
        /// Assets service.
        /// </summary>
        [BindService]
        protected AssetsService assetsService;

        /// <summary>
        /// Noesis service.
        /// </summary>
        [BindService]
        protected NoesisService noesisService;

        /// <summary>
        /// Gets or sets text.
        /// </summary>
        [RenderProperty(Tooltip = "The text that will be shown")]
        public string Text
        {
            get => this.text;
            set
            {
                if (this.SetProperty(ref this.text, value, this.textBlock))
                {
                    this.textBlock.Text = value;
                    this.textBlock.TextTrimming = this.TextTrimming; // Noesis workaround
                    this.Invalidate();
                }
            }
        }

        private string text = string.Empty;

        /// <summary>
        /// Gets or sets font size.
        /// </summary>
        [RenderPropertyAsInput(minLimit: 1, Tooltip = "The font size used to render the text")]
        public int FontSize
        {
            get => this.fontSize;
            set
            {
                if (this.SetProperty(ref this.fontSize, value, this.textBlock))
                {
                    this.textBlock.FontSize = value;
                    this.Invalidate();
                }
            }
        }

        private int fontSize = 36;

        /// <summary>
        /// Gets or sets font alpha.
        /// </summary>
        [RenderPropertyAsFInput(Tooltip = "The alpha value for the text component", MinLimit = 0, MaxLimit = 1)]
        public float Alpha
        {
            get => this.alpha;
            set
            {
                if (this.SetProperty(ref this.alpha, value, this.holographicMaterial))
                {
                    this.holographicMaterial.Parameters_Alpha = value;
                }
            }
        }

        private float alpha = 1.0f;

        /// <summary>
        /// Gets or sets a value indicating whether the width of the text block will be predefined or auto-calculated based on text.
        /// </summary>
        [RenderProperty(Tooltip = "Indicates whether the width of the text block will be predefined or auto-calculated based on text.")]
        public bool CustomWidth
        {
            get => this.customWidth;
            set
            {
                if (this.SetProperty(ref this.customWidth, value, this.noesisFramebufferPanel))
                {
                    this.Invalidate(ifAutoSizeOnly: false);
                }
            }
        }

        private bool customWidth = false;

        /// <summary>
        /// Gets or sets with of text block.
        /// </summary>
        [RenderProperty(Tooltip = "The text block width in meters.")]
        public float Width
        {
            get => this.width;
            set
            {
                if (this.SetProperty(ref this.width, value, this.noesisFramebufferPanel))
                {
                    this.Invalidate(ifAutoSizeOnly: false);
                }
            }
        }

        private float width = 0.3f;

        /// <summary>
        /// Gets or sets text horizontal alignment.
        /// </summary>
        [RenderProperty(Tooltip = "The text horizontal alignment")]
        public HorizontalAlignment HorizontalAlignment
        {
            get => this.horizontalAlignment;
            set
            {
                if (this.SetProperty(ref this.horizontalAlignment, value, this.textBlock))
                {
                    this.textBlock.HorizontalAlignment = value;
                }
            }
        }

        private HorizontalAlignment horizontalAlignment = HorizontalAlignment.Left;

        /// <summary>
        /// Gets or sets a value indicating whether the height of the text block will be predefined or auto-calculated based on text.
        /// </summary>
        [RenderProperty(Tooltip = "Indicates whether the height of the text block will be predefined or auto-calculated based on text.")]
        public bool CustomHeight
        {
            get => this.customHeight;
            set
            {
                if (this.SetProperty(ref this.customHeight, value, this.noesisFramebufferPanel))
                {
                    this.Invalidate(ifAutoSizeOnly: false);
                }
            }
        }

        private bool customHeight = false;

        /// <summary>
        /// Gets or sets height of text block.
        /// </summary>
        [RenderProperty(Tooltip = "The text block height in meters.")]
        public float Height
        {
            get => this.height;
            set
            {
                if (this.SetProperty(ref this.height, value, this.noesisFramebufferPanel))
                {
                    this.Invalidate(ifAutoSizeOnly: false);
                }
            }
        }

        private float height = 0.1f;

        /// <summary>
        /// Gets or sets text vertical alignment.
        /// </summary>
        [RenderProperty(Tooltip = "The text vertical alignment.")]
        public VerticalAlignment VerticalAlignment
        {
            get => this.verticalAlignment;
            set
            {
                if (this.SetProperty(ref this.verticalAlignment, value, this.textBlock))
                {
                    this.textBlock.VerticalAlignment = value;
                }
            }
        }

        private VerticalAlignment verticalAlignment = VerticalAlignment.Top;

        /// <summary>
        /// Gets or sets text block origin.
        /// </summary>
        [RenderProperty(Tooltip = "The origin (also known as pivot) from where the text block scales, rotates and translates. Its values are included in [0, 1] where (0, 0) indicates the top left corner. Such values are percentages where 1 means the 100% of the rectangle's width/height.")]
        public Vector2 Origin
        {
            get => this.origin;
            set
            {
                if (this.SetProperty(ref this.origin, value, this.planeTransform))
                {
                    this.UpdateOrigin(this.planeTransform);
                }
            }
        }

        private Vector2 origin = Vector2.Center;

        /// <summary>
        /// Gets or sets pixel density.
        /// </summary>
        [RenderProperty(Tooltip = "The pixel density expressed as pixels/meter.")]
        public int PixelDensity
        {
            get => this.pixelDensity;
            set
            {
                if (this.SetProperty(ref this.pixelDensity, value, this.noesisFramebufferPanel))
                {
                    this.Invalidate(ifAutoSizeOnly: false);
                }

                this.resolutionScaleFactor = (float)this.pixelDensity / ReferencePixelDensity;
            }
        }

        private const int ReferencePixelDensity = 3780;

        private int pixelDensity = ReferencePixelDensity;

        /// <summary>
        /// Gets or sets block background color.
        /// </summary>
        [RenderProperty(Tooltip = "The text block foreground color.")]
        public Color Background
        {
            get => this.background;
            set
            {
                if (this.SetProperty(ref this.background, value, this.backgroundBrush))
                {
                    var color = this.backgroundBrush.Color;
                    color.R = this.background.R;
                    color.G = this.background.G;
                    color.B = this.background.B;
                    color.A = this.background.A;
                    this.backgroundBrush.Color = color;
                }
            }
        }

        private Color background = Color.Transparent;

        /// <summary>
        /// Gets or sets text foreground color.
        /// </summary>
        [RenderProperty(Tooltip = "The text foreground color.")]
        public Color Foreground
        {
            get => this.foreground;
            set
            {
                if (this.SetProperty(ref this.foreground, value, this.foregroundBrush))
                {
                    var color = this.foregroundBrush.Color;
                    color.R = this.foreground.R;
                    color.G = this.foreground.G;
                    color.B = this.foreground.B;
                    color.A = this.foreground.A;
                    this.foregroundBrush.Color = color;
                }
            }
        }

        private Color foreground = Color.White;

        /// <summary>
        /// Gets or sets text block plane normal direction.
        /// </summary>
        [RenderProperty(Tooltip = "The text block plane normal direction.")]
        public NormalAxis Normal
        {
            get => this.normal;
            set
            {
                if (this.SetProperty(ref this.normal, value, this.planeMesh))
                {
                    this.UpdatePlaneNormal();
                }
            }
        }

        private NormalAxis normal = NormalAxis.ZNegative;

        /// <summary>
        /// Gets or sets text alignment to use.
        /// </summary>
        [RenderProperty(Tooltip = "The text alignment to use.")]
        public TextAlignment TextAlignment
        {
            get => this.textAlignment;
            set
            {
                if (this.SetProperty(ref this.textAlignment, value, this.textBlock))
                {
                    this.textBlock.TextAlignment = value;
                    this.Invalidate();
                }
            }
        }

        private TextAlignment textAlignment = TextAlignment.Left;

        /// <summary>
        /// Gets or sets text wrapping to use.
        /// </summary>
        [RenderProperty(Tooltip = "The text wrapping to use.")]
        public TextWrapping TextWrapping
        {
            get => this.textWrapping;
            set
            {
                if (this.SetProperty(ref this.textWrapping, value, this.textBlock))
                {
                    this.textBlock.TextWrapping = value;
                    this.Invalidate();
                }
            }
        }

        private TextWrapping textWrapping = TextWrapping.NoWrap;

        /// <summary>
        /// Gets or sets text font family source to use.
        /// </summary>
        [RenderProperty(Tooltip = "The text font family source to use.")]
        public string FontFamilySource
        {
            get => this.fontFamilySource;
            set
            {
                if (this.SetProperty(ref this.fontFamilySource, value, this.textBlock))
                {
                    this.textBlock.FontFamily = this.GetFontFamily(this.fontFamilySource);
                    this.Invalidate();
                }
            }
        }

        private string fontFamilySource = string.Empty;

        /// <summary>
        /// Gets or sets text font weight to use.
        /// </summary>
        [RenderProperty(Tooltip = "The text font weight to use.")]
        public FontWeight FontWeight
        {
            get => this.fontWeight;
            set
            {
                if (this.SetProperty(ref this.fontWeight, value, this.textBlock))
                {
                    this.textBlock.FontWeight = value;
                    this.Invalidate();
                }
            }
        }

        private FontWeight fontWeight = FontWeight.Normal;

        /// <summary>
        /// Gets or sets text font stretch to use.
        /// </summary>
        [RenderProperty(Tooltip = "The text font stretch to use.")]
        public FontStretch FontStretch
        {
            get => this.fontStretch;
            set
            {
                if (this.SetProperty(ref this.fontStretch, value, this.textBlock))
                {
                    this.textBlock.FontStretch = value;
                    this.Invalidate();
                }
            }
        }

        private FontStretch fontStretch = FontStretch.Normal;

        /// <summary>
        /// Gets or sets text font style to use.
        /// </summary>
        [RenderProperty(Tooltip = "The text font style to use.")]
        public FontStyle FontStyle
        {
            get => this.fontStyle;
            set
            {
                if (this.SetProperty(ref this.fontStyle, value, this.textBlock))
                {
                    this.textBlock.FontStyle = value;
                    this.Invalidate();
                }
            }
        }

        private FontStyle fontStyle = FontStyle.Normal;

        /// <summary>
        /// Gets or sets text trimming to use.
        /// </summary>
        [RenderProperty(Tooltip = "The text trimming to use when the text width is longer than the available width")]
        public TextTrimming TextTrimming
        {
            get => string.IsNullOrEmpty(this.Text) ? TextTrimming.None : this.textTrimming;
            set
            {
                if (this.SetProperty(ref this.textTrimming, value, this.textBlock))
                {
                    this.textBlock.TextTrimming = this.TextTrimming;
                    this.Invalidate();
                }
            }
        }

        private TextTrimming textTrimming = TextTrimming.None;

        private string containerEntityName => $"Text3D_{this.Id}";

        /// <summary>
        /// Gets text block inlines.
        /// </summary>
        public InlineCollection Inlines => this.textBlock?.Inlines;

        private Transform3D planeTransform;
        private MaterialComponent materialComponent;
        private MeshRenderer meshRenderer;
        private PlaneMesh planeMesh;
        private NoesisFramebufferPanel noesisFramebufferPanel;

        private HoloGraphic holographicMaterial;
        private FrameBuffer noesisFramebuffer;

        private FrameworkElement frameworkElement;
        private TextBlock textBlock;
        private SolidColorBrush backgroundBrush;
        private SolidColorBrush foregroundBrush;

        private bool isContainerEntityAdded = false;
        private bool isFrameBufferDirty = true;
        private float resolutionScaleFactor = 1;

        /// <inheritdoc />
        protected override void OnLoaded()
        {
            base.OnLoaded();

            if (Application.Current.IsEditor)
            {
                this.Family = FamilyType.PriorityBehavior;
            }
        }

        /// <inheritdoc />
        protected override bool OnAttached()
        {
            if (!base.OnAttached())
            {
                return false;
            }

            // Build FrameworkElement
            this.textBlock = this.BuildTextBlock();

            return true;
        }

        /// <inheritdoc />
        protected override void OnDetach()
        {
            this.RemoveContainerEntity();

            this.textBlock = null;

            base.OnDetach();
        }

        /// <inheritdoc />
        protected override void Update(TimeSpan gameTime)
        {
            this.UpdateNoesisPanelFrameBuffer();
        }

        /// <summary>
        /// Invalidates the text control and forces an internal frameBuffer update.
        /// </summary>
        /// <param name="ifAutoSizeOnly">Indicates whether the invalidation operation should be done only if auto-size is active.</param>
        public void Invalidate(bool ifAutoSizeOnly = true)
        {
            if (!ifAutoSizeOnly || !this.customWidth || !this.customHeight)
            {
                this.isFrameBufferDirty = true;
            }
        }

        private bool CanUpdateFrameBuffer()
        {
            if (this.customWidth && this.width <= 0)
            {
                return false;
            }

            if (this.customHeight && this.height <= 0)
            {
                return false;
            }

            if (!this.customWidth || !this.CustomHeight)
            {
                var isFontFamilyLoaded = string.IsNullOrEmpty(this.fontFamilySource) ||
                                         this.noesisService.StyleValid;
                var hasContent = !string.IsNullOrEmpty(this.text) || this.Inlines?.Count > 0;
                return isFontFamilyLoaded && hasContent;
            }

            return true;
        }

        private void AddContainerEntity()
        {
            // Add components to owner entity
            this.planeTransform = new Transform3D();

            this.materialComponent = new MaterialComponent();

            this.meshRenderer = new MeshRenderer();
            this.meshRenderer.CastShadows = false;

            this.planeMesh = new PlaneMesh();

            this.noesisFramebufferPanel = new NoesisFramebufferPanel()
            {
                BackgroundColor = this.background,
                PPAAEnabled = true,
                EnableKeyboard = false,
                EnableMouse = false,
                EnableTouch = false,
                Width = 1,
                Height = 1,
            };

            // Set property values
            this.UpdatePlaneNormal();

            // Build FrameworkElement
            this.frameworkElement = this.BuildFrameworkElement(this.textBlock);

            this.noesisFramebufferPanel.FrameworkElement = this.frameworkElement;

            // Material initialization
            if (this.materialComponent.Material == null)
            {
                var effect = this.assetsService.Load<Effect>(HoloGraphic.EffectId);
                this.holographicMaterial = new HoloGraphic(effect)
                {
                    Id = this.Id,
                    Albedo = Color.White,
                    LayerDescription = this.assetsService.Load<RenderLayerDescription>(DefaultResourcesIDs.AlphaRenderLayerID),
                    Sampler = this.assetsService.Load<SamplerState>(DefaultResourcesIDs.LinearClampSamplerID),
                };

                this.materialComponent.Material = this.holographicMaterial.Material;
            }
            else
            {
                this.holographicMaterial = new HoloGraphic(this.materialComponent.Material);
            }

            this.holographicMaterial.Parameters_Alpha = this.Alpha;

            // Add container entity and components
            var containerEntity = new Entity(this.containerEntityName)
            {
                Flags = HideFlags.DontSave | HideFlags.DontShow,
            }
            .AddComponent(this.planeTransform)
            .AddComponent(this.materialComponent)
            .AddComponent(this.meshRenderer)
            .AddComponent(this.planeMesh)
            .AddComponent(this.noesisFramebufferPanel)
            ;

            this.Owner.AddChild(containerEntity);

            this.isContainerEntityAdded = true;
        }

        private void RemoveContainerEntity()
        {
            if (this.noesisFramebuffer != null)
            {
                this.noesisFramebuffer.Dispose();
                this.noesisFramebufferPanel.FrameBuffer = null;
                this.noesisFramebufferPanel.FrameworkElement = null;
                this.holographicMaterial.Texture = null;
            }

            this.planeTransform = null;
            this.materialComponent = null;
            this.meshRenderer = null;
            this.planeMesh = null;
            this.noesisFramebufferPanel = null;
            this.holographicMaterial = null;

            this.Owner.RemoveChild(this.containerEntityName);

            this.isContainerEntityAdded = false;
        }

        /// <summary>
        /// Builds a <see cref="TextBlock"/>.
        /// </summary>
        /// <returns>A text block.</returns>
        protected virtual TextBlock BuildTextBlock()
        {
            var noesisColor = new Noesis.Color();
            noesisColor.R = this.foreground.R;
            noesisColor.G = this.foreground.G;
            noesisColor.B = this.foreground.B;
            noesisColor.A = this.foreground.A;
            this.foregroundBrush = new SolidColorBrush(noesisColor);

            return new TextBlock()
            {
                Foreground = this.foregroundBrush,
                HorizontalAlignment = this.HorizontalAlignment,
                VerticalAlignment = this.VerticalAlignment,
                Text = this.Text,
                TextWrapping = this.TextWrapping,
                FontWeight = this.FontWeight,
                FontSize = this.FontSize,
                FontStretch = this.FontStretch,
                FontStyle = this.FontStyle,
                FontFamily = this.GetFontFamily(this.FontFamilySource),
                TextTrimming = this.TextTrimming,
            };
        }

        /// <summary>
        /// Builds a <see cref="FrameworkElement"/>.
        /// </summary>
        /// <param name="textBlock">A <see cref="TextBlock"/>.</param>
        /// <returns>A framework element.</returns>
        protected virtual FrameworkElement BuildFrameworkElement(TextBlock textBlock)
        {
            var noesisColor = new Noesis.Color();
            noesisColor.R = this.background.R;
            noesisColor.G = this.background.G;
            noesisColor.B = this.background.B;
            noesisColor.A = this.background.A;
            this.backgroundBrush = new SolidColorBrush(noesisColor);

            var frameworkElement = new Grid()
            {
                Background = this.backgroundBrush,
            };

            frameworkElement.Children.Add(textBlock);

            return frameworkElement;
        }

        private void UpdatePlaneNormal()
        {
            if (this.planeMesh == null)
            {
                return;
            }

            this.planeMesh.PlaneNormal = this.normal;
        }

        /// <summary>
        /// Gets <see cref="FontFamily"/> instance from source.
        /// </summary>
        /// <param name="fontFamilySource">Font source.</param>
        /// <returns>Font family.</returns>
        protected FontFamily GetFontFamily(string fontFamilySource)
        {
            return string.IsNullOrEmpty(fontFamilySource) ? null : new FontFamily(fontFamilySource);
        }

        private void UpdateOrigin(Transform3D planeTransform)
        {
            planeTransform.LocalPosition = new Vector3((this.origin.X - 0.5f) * this.width, (this.origin.Y - 0.5f) * this.height, 0);
        }

        private void UpdateNoesisPanelFrameBuffer()
        {
            var needUpdate = !this.isContainerEntityAdded || this.isFrameBufferDirty;
            if (!needUpdate || !this.CanUpdateFrameBuffer())
            {
                return;
            }

            if (!this.isContainerEntityAdded)
            {
                this.AddContainerEntity();
            }

            uint pixelWidth;
            uint pixelHeight;
            var planeTiling = Vector2.One;
            if (!this.customWidth || !this.customHeight)
            {
                var maxPixelSize = this.ClampPixelSize(float.MaxValue);
                var availableSize = new Size(maxPixelSize, maxPixelSize);

                if (this.customWidth)
                {
                    availableSize.Width = this.GetPixelSize(this.width, ReferencePixelDensity);
                }

                if (this.customHeight)
                {
                    availableSize.Height = this.GetPixelSize(this.height, ReferencePixelDensity);
                }

                this.textBlock.Measure(availableSize);
                var desiredSize = this.textBlock.DesiredSize * this.resolutionScaleFactor;
                desiredSize.Width = (float)Math.Ceiling(desiredSize.Width);
                desiredSize.Height = (float)Math.Ceiling(desiredSize.Height);

                if (!this.customWidth)
                {
                    pixelWidth = (uint)desiredSize.Width;
                    this.width = this.GetPhysicalSize(desiredSize.Width);
                }
                else
                {
                    pixelWidth = (uint)this.GetPixelSize(this.width, this.pixelDensity);
                    var unclampedPixelWidth = this.GetPixelSize(this.width, this.pixelDensity, clamped: false);
                    if (unclampedPixelWidth > pixelWidth)
                    {
                        planeTiling.X = unclampedPixelWidth / pixelWidth;
                    }
                }

                if (!this.customHeight)
                {
                    pixelHeight = (uint)desiredSize.Height;
                    this.height = this.GetPhysicalSize(desiredSize.Height);
                }
                else
                {
                    pixelHeight = (uint)this.GetPixelSize(this.height, this.pixelDensity);
                    var unclampedPixelHeight = this.GetPixelSize(this.height, this.pixelDensity, clamped: false);
                    if (unclampedPixelHeight > pixelHeight)
                    {
                        planeTiling.Y = unclampedPixelHeight / pixelHeight;
                    }
                }
            }
            else
            {
                pixelWidth = (uint)this.GetPixelSize(this.width, this.pixelDensity);
                pixelHeight = (uint)this.GetPixelSize(this.height, this.pixelDensity);
            }

            pixelWidth = (uint)this.ClampPixelSize(pixelWidth);
            pixelHeight = (uint)this.ClampPixelSize(pixelHeight);

            this.UpdateOrigin(this.planeTransform);

            this.frameworkElement.LayoutTransform = new MatrixTransform(Transform2.Scale(this.resolutionScaleFactor, this.resolutionScaleFactor));

            if (this.planeMesh.Width != this.width ||
                this.planeMesh.Height != this.height ||
                this.planeMesh.UTile != planeTiling.X ||
                this.planeMesh.YTile != planeTiling.Y)
            {
                this.planeMesh.Width = this.width;
                this.planeMesh.Height = this.height;
                this.planeMesh.UTile = planeTiling.X;
                this.planeMesh.YTile = planeTiling.Y;
            }

            if (this.noesisFramebufferPanel.Width != pixelWidth ||
                this.noesisFramebufferPanel.Height != pixelHeight)
            {
                this.noesisFramebufferPanel.Width = pixelWidth;
                this.noesisFramebufferPanel.Height = pixelHeight;

                if (this.noesisFramebufferPanel.FrameBuffer != null)
                {
                    this.noesisFramebuffer.Dispose();
                    this.noesisFramebuffer = null;
                    this.noesisFramebufferPanel.FrameBuffer = null;
                }

                this.noesisFramebuffer = this.graphicsContext.Factory.CreateFrameBuffer(pixelWidth, pixelHeight);
                this.noesisFramebufferPanel.FrameBuffer = this.noesisFramebuffer;

                // Set material texture to use Noesis framebuffer texture
                this.holographicMaterial.Texture = this.noesisFramebuffer.ColorTargets[0].Texture;
            }

            this.isFrameBufferDirty = false;
        }

        private float GetPhysicalSize(float pixelSize)
        {
            return (float)Math.Ceiling(pixelSize) / this.pixelDensity;
        }

        private float GetPixelSize(float physicalSize, float pixelDensity, bool clamped = true)
        {
            var pixelSize = physicalSize * pixelDensity;
            return clamped ? this.ClampPixelSize(pixelSize) : pixelSize;
        }

        private float ClampPixelSize(float pixelSize)
        {
            return MathHelper.Clamp(pixelSize, 1, 4096);
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
