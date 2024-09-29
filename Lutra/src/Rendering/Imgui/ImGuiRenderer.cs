// Used from Veldrid.ImGui under the MIT license.
// Copyright (c) 2017 Eric Mellino and Veldrid contributors.

using ImGuiNET;
using System.Numerics;
using Veldrid;
using Lutra.Input;
using Lutra.Utility;

namespace Lutra.Rendering
{
    /// <summary>
    /// Can render draw lists produced by ImGui.
    /// Also provides functions for updating ImGui input.
    /// </summary>
    public class ImGuiRenderer : IDisposable
    {
        private GraphicsDevice _gd;

        // Device objects
        private DeviceBuffer _vertexBuffer;
        private DeviceBuffer _indexBuffer;
        private DeviceBuffer _projMatrixBuffer;
        private Texture _fontTexture;
        private Shader _vertexShader;
        private Shader _fragmentShader;
        private ResourceLayout _layout;
        private ResourceLayout _textureLayout;
        private Pipeline _pipeline;
        private ResourceSet _mainResourceSet;
        private ResourceSet _fontTextureResourceSet;
        private IntPtr _fontAtlasID = (IntPtr)1;

        private int _windowWidth;
        private int _windowHeight;
        private Vector2 _scaleFactor = Vector2.One;

        private bool _frameBegun;

        /// <summary>
        /// Constructs a new ImGuiRenderer.
        /// </summary>
        /// <param name="gd">The GraphicsDevice used to create and update resources.</param>
        /// <param name="outputDescription">The output format.</param>
        /// <param name="width">The initial width of the rendering target. Can be resized.</param>
        /// <param name="height">The initial height of the rendering target. Can be resized.</param>
        public ImGuiRenderer(GraphicsDevice gd, OutputDescription outputDescription, int width, int height)
        {
            _gd = gd;
            _windowWidth = width;
            _windowHeight = height;

            IntPtr context = ImGui.CreateContext();
            ImGui.SetCurrentContext(context);

            ImGui.GetIO().Fonts.AddFontDefault();
            ImGui.GetIO().Fonts.Flags |= ImFontAtlasFlags.NoBakedLines;

            CreateDeviceResources(gd, outputDescription);

            SetPerFrameImGuiData(1f / 60f);

            ImGui.NewFrame();
            _frameBegun = true;
        }

        public void WindowResized(int width, int height)
        {
            _windowWidth = width;
            _windowHeight = height;
        }

        public void DestroyDeviceObjects()
        {
            Dispose();
        }

        public void CreateDeviceResources(GraphicsDevice gd, OutputDescription outputDescription)
        {
            _gd = gd;
            ResourceFactory factory = gd.ResourceFactory;
            _vertexBuffer = factory.CreateBuffer(new BufferDescription(10000, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
            _vertexBuffer.Name = "ImGui.NET Vertex Buffer";
            _indexBuffer = factory.CreateBuffer(new BufferDescription(2000, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
            _indexBuffer.Name = "ImGui.NET Index Buffer";

            _projMatrixBuffer = factory.CreateBuffer(new BufferDescription(64, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            _projMatrixBuffer.Name = "ImGui.NET Projection Buffer";

            byte[] vertexShaderBytes = GetShaderCode(gd.ResourceFactory, ShaderStages.Vertex);
            byte[] fragmentShaderBytes = GetShaderCode(gd.ResourceFactory, ShaderStages.Fragment);
            _vertexShader = factory.CreateShader(new ShaderDescription(ShaderStages.Vertex, vertexShaderBytes, _gd.BackendType == GraphicsBackend.Vulkan ? "main" : "VS"));
            _vertexShader.Name = "ImGui.NET Vertex Shader";
            _fragmentShader = factory.CreateShader(new ShaderDescription(ShaderStages.Fragment, fragmentShaderBytes, _gd.BackendType == GraphicsBackend.Vulkan ? "main" : "FS"));
            _fragmentShader.Name = "ImGui.NET Fragment Shader";

            VertexLayoutDescription[] vertexLayouts = new VertexLayoutDescription[]
            {
                new VertexLayoutDescription(
                    new VertexElementDescription("in_position", VertexElementSemantic.Position, VertexElementFormat.Float2),
                    new VertexElementDescription("in_texCoord", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
                    new VertexElementDescription("in_color", VertexElementSemantic.Color, VertexElementFormat.Byte4Norm))
            };

            _layout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("ProjectionMatrixBuffer", ResourceKind.UniformBuffer, ShaderStages.Vertex),
                new ResourceLayoutElementDescription("MainSampler", ResourceKind.Sampler, ShaderStages.Fragment)));
            _layout.Name = "ImGui.NET Resource Layout";
            _textureLayout = factory.CreateResourceLayout(new ResourceLayoutDescription(
                new ResourceLayoutElementDescription("MainTexture", ResourceKind.TextureReadOnly, ShaderStages.Fragment)));
            _textureLayout.Name = "ImGui.NET Texture Layout";

            GraphicsPipelineDescription pd = new GraphicsPipelineDescription(
                BlendStateDescription.SINGLE_ALPHA_BLEND,
                new DepthStencilStateDescription(false, false, ComparisonKind.Always),
                new RasterizerStateDescription(FaceCullMode.None, PolygonFillMode.Solid, FrontFace.Clockwise, true, true),
                PrimitiveTopology.TriangleList,
                new ShaderSetDescription(
                    vertexLayouts,
                    new[] { _vertexShader, _fragmentShader },
                    new[]
                    {
                        new SpecializationConstant(0, gd.IsClipSpaceYInverted),
                        new SpecializationConstant(1, true), // always legacy
                    }),
                new ResourceLayout[] { _layout, _textureLayout },
                outputDescription,
                ResourceBindingModel.Default);
            _pipeline = factory.CreateGraphicsPipeline(ref pd);
            _pipeline.Name = "ImGui.NET Pipeline";

            _mainResourceSet = factory.CreateResourceSet(new ResourceSetDescription(_layout,
                _projMatrixBuffer,
                gd.PointSampler));
            _mainResourceSet.Name = "ImGui.NET Main Resource Set";

            RecreateFontDeviceTexture(gd);
        }

        private static byte[] GetShaderCode(ResourceFactory factory, ShaderStages stage)
        {
            var backend = factory.BackendType;
            var name = stage == ShaderStages.Vertex ? VertexShaderName(backend) : FragmentShaderName(backend);
            return AssetManager.BuiltinShaderBytesCache[$"Lutra.Shaders.ImGui.{name}"];
        }

        private static string VertexShaderName(GraphicsBackend backend) => backend switch
        {
            GraphicsBackend.Direct3D11 => "imgui-vertex-legacy.hlsl.bytes",
            GraphicsBackend.OpenGL => "imgui-vertex-legacy.glsl",
            GraphicsBackend.OpenGLES => "imgui-vertex-legacy.glsles",
            GraphicsBackend.Vulkan => "imgui-vertex.spv",
            GraphicsBackend.Metal => "imgui-vertex.metallib",
            _ => throw new NotImplementedException(),
        };

        private static string FragmentShaderName(GraphicsBackend backend) => backend switch
        {
            GraphicsBackend.Direct3D11 => "imgui-frag.hlsl.bytes",
            GraphicsBackend.OpenGL => "imgui-frag.glsl",
            GraphicsBackend.OpenGLES => "imgui-frag.glsles",
            GraphicsBackend.Vulkan => "imgui-frag.spv",
            GraphicsBackend.Metal => "imgui-frag.metallib",
            _ => throw new NotImplementedException(),
        };

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public unsafe void RecreateFontDeviceTexture() => RecreateFontDeviceTexture(_gd);

        /// <summary>
        /// Recreates the device texture used to render text.
        /// </summary>
        public unsafe void RecreateFontDeviceTexture(GraphicsDevice gd)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            // Build
            io.Fonts.GetTexDataAsRGBA32(out byte* pixels, out int width, out int height, out int bytesPerPixel);

            // Store our identifier
            io.Fonts.SetTexID(_fontAtlasID);

            _fontTexture?.Dispose();
            _fontTexture = gd.ResourceFactory.CreateTexture(TextureDescription.Texture2D(
                (uint)width,
                (uint)height,
                1,
                1,
                PixelFormat.R8G8B8A8UNorm,
                TextureUsage.Sampled));
            _fontTexture.Name = "ImGui.NET Font Texture";
            gd.UpdateTexture(
                _fontTexture,
                (IntPtr)pixels,
                (uint)(bytesPerPixel * width * height),
                0,
                0,
                0,
                (uint)width,
                (uint)height,
                1,
                0,
                0);

            _fontTextureResourceSet?.Dispose();
            _fontTextureResourceSet = gd.ResourceFactory.CreateResourceSet(new ResourceSetDescription(_textureLayout, _fontTexture));
            _fontTextureResourceSet.Name = "ImGui.NET Font Texture Resource Set";

            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Renders the ImGui draw list data.
        /// </summary>
        public unsafe void Render(GraphicsDevice gd, CommandList cl)
        {
            if (_frameBegun)
            {
                _frameBegun = false;
                ImGui.Render();
                RenderImDrawData(ImGui.GetDrawData(), gd, cl);
            }
        }

        /// <summary>
        /// Updates ImGui input and IO configuration state.
        /// </summary>
        public void Update(float deltaSeconds)
        {
            BeginUpdate(deltaSeconds);
            UpdateImGuiInput();
            EndUpdate();
        }

        /// <summary>
        /// Called before we handle the input in <see cref="Update(float, InputSnapshot)"/>.
        /// This render ImGui and update the state.
        /// </summary>
        protected void BeginUpdate(float deltaSeconds)
        {
            if (_frameBegun)
            {
                ImGui.Render();
            }

            SetPerFrameImGuiData(deltaSeconds);
        }

        /// <summary>
        /// Called at the end of <see cref="Update(float, InputSnapshot)"/>.
        /// This tells ImGui that we are on the next frame.
        /// </summary>
        protected void EndUpdate()
        {
            _frameBegun = true;
            ImGui.NewFrame();
        }

        /// <summary>
        /// Sets per-frame data based on the associated window.
        /// This is called by Update(float).
        /// </summary>
        private unsafe void SetPerFrameImGuiData(float deltaSeconds)
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.DisplaySize = new Vector2(
                _windowWidth / _scaleFactor.X,
                _windowHeight / _scaleFactor.Y);
            io.DisplayFramebufferScale = _scaleFactor;
            io.DeltaTime = deltaSeconds; // DeltaTime is in seconds.
        }

        private static unsafe void UpdateImGuiInput()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.AddMousePosEvent(InputManager.MouseRawX, InputManager.MouseRawY);
            io.AddMouseButtonEvent(0, InputManager.MouseButtonDown(MouseButton.Left));
            io.AddMouseButtonEvent(1, InputManager.MouseButtonDown(MouseButton.Right));
            io.AddMouseButtonEvent(2, InputManager.MouseButtonDown(MouseButton.Middle));
            io.AddMouseButtonEvent(3, InputManager.MouseButtonDown(MouseButton.X1));
            io.AddMouseButtonEvent(4, InputManager.MouseButtonDown(MouseButton.X2));
            io.AddMouseWheelEvent(0f, InputManager.MouseWheelDelta);

            io.AddInputCharactersUTF8(InputManager.KeyStringPerFrame);

            foreach (var key in InputManager.KeysPressedThisFrame)
            {
                if (TryMapKey(key, out ImGuiKey imguikey))
                {
                    io.AddKeyEvent(imguikey, true);
                }
            }

            foreach (var key in InputManager.KeysReleasedThisFrame)
            {
                if (TryMapKey(key, out ImGuiKey imguikey))
                {
                    io.AddKeyEvent(imguikey, false);
                }
            }
        }

        private static bool TryMapKey(Key key, out ImGuiKey result)
        {
            result = ImGuiKey.None;

            if (key >= Key.F1 && key <= Key.F12)
            {
                result = MapKeyRange(key, Key.F1, ImGuiKey.F1);
            }
            else if (key >= Key.NumPad1 && key <= Key.NumPad9)
            {
                result = MapKeyRange(key, Key.NumPad1, ImGuiKey.Keypad1);
            }
            else if (key >= Key.A && key <= Key.Z)
            {
                result = MapKeyRange(key, Key.A, ImGuiKey.A);
            }
            else if (key >= Key.D1 && key <= Key.D9)
            {
                result = MapKeyRange(key, Key.D1, ImGuiKey._1);
            }

            result = MapIndividualKeys(key, result);

            return result != ImGuiKey.None;
        }

        private static ImGuiKey MapKeyRange(Key keyToConvert, Key startKey1, ImGuiKey startKey2)
        {
            int changeFromStart1 = (int)keyToConvert - (int)startKey1;
            return startKey2 + changeFromStart1;
        }

        private static ImGuiKey MapIndividualKeys(Key key, ImGuiKey result)
        {
            return key switch
            {
                Key.D0 => ImGuiKey._0,
                Key.LeftShift or
                Key.RightShift => ImGuiKey.ModShift,
                Key.LeftControl or
                Key.RightControl => ImGuiKey.ModCtrl,
                Key.LeftAlt or
                Key.RightAlt => ImGuiKey.ModAlt,
                Key.LeftWindows or
                Key.RightWindows => ImGuiKey.ModSuper,
                Key.Up => ImGuiKey.UpArrow,
                Key.Down => ImGuiKey.DownArrow,
                Key.Left => ImGuiKey.LeftArrow,
                Key.Right => ImGuiKey.RightArrow,
                Key.Enter => ImGuiKey.Enter,
                Key.Escape => ImGuiKey.Escape,
                Key.Space => ImGuiKey.Space,
                Key.Tab => ImGuiKey.Tab,
                Key.Backspace => ImGuiKey.Backspace,
                Key.Insert => ImGuiKey.Insert,
                Key.Delete => ImGuiKey.Delete,
                Key.PageUp => ImGuiKey.PageUp,
                Key.PageDown => ImGuiKey.PageDown,
                Key.Home => ImGuiKey.Home,
                Key.End => ImGuiKey.End,
                Key.CapsLock => ImGuiKey.CapsLock,
                Key.ScrollLock => ImGuiKey.ScrollLock,
                Key.PrintScreen => ImGuiKey.PrintScreen,
                Key.Pause => ImGuiKey.Pause,
                Key.NumLock => ImGuiKey.NumLock,
                Key.NumPad0 => ImGuiKey.Keypad0,
                Key.NumPadDivide => ImGuiKey.KeypadDivide,
                Key.NumPadMultiply => ImGuiKey.KeypadMultiply,
                Key.NumPadMinus => ImGuiKey.KeypadSubtract,
                Key.NumPadPlus => ImGuiKey.KeypadAdd,
                Key.NumPadPeriod => ImGuiKey.KeypadDecimal,
                Key.NumPadEnter => ImGuiKey.KeypadEnter,
                Key.Grave => ImGuiKey.GraveAccent,
                Key.Minus => ImGuiKey.Minus,
                Key.Equals => ImGuiKey.Equal,
                Key.LeftBracket => ImGuiKey.LeftBracket,
                Key.RightBracket => ImGuiKey.RightBracket,
                Key.Semicolon => ImGuiKey.Semicolon,
                Key.Apostrophe => ImGuiKey.Apostrophe,
                Key.Comma => ImGuiKey.Comma,
                Key.Period => ImGuiKey.Period,
                Key.Slash => ImGuiKey.Slash,
                Key.Backslash or
                Key.NonUsBackslash => ImGuiKey.Backslash,
                _ => result,
            };
        }

        private unsafe void RenderImDrawData(ImDrawDataPtr draw_data, GraphicsDevice gd, CommandList cl)
        {
            uint vertexOffsetInVertices = 0;
            uint indexOffsetInElements = 0;

            if (draw_data.CmdListsCount == 0)
            {
                return;
            }

            uint totalVBSize = (uint)(draw_data.TotalVtxCount * sizeof(ImDrawVert));
            if (totalVBSize > _vertexBuffer.SizeInBytes)
            {
                _vertexBuffer.Dispose();
                _vertexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalVBSize * 1.5f), BufferUsage.VertexBuffer | BufferUsage.Dynamic));
                _vertexBuffer.Name = $"ImGui.NET Vertex Buffer";
            }

            uint totalIBSize = (uint)(draw_data.TotalIdxCount * sizeof(ushort));
            if (totalIBSize > _indexBuffer.SizeInBytes)
            {
                _indexBuffer.Dispose();
                _indexBuffer = gd.ResourceFactory.CreateBuffer(new BufferDescription((uint)(totalIBSize * 1.5f), BufferUsage.IndexBuffer | BufferUsage.Dynamic));
                _indexBuffer.Name = $"ImGui.NET Index Buffer";
            }

            for (int i = 0; i < draw_data.CmdListsCount; i++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdLists[i];

                cl.UpdateBuffer(
                    _vertexBuffer,
                    vertexOffsetInVertices * (uint)sizeof(ImDrawVert),
                    cmd_list.VtxBuffer.Data,
                    (uint)(cmd_list.VtxBuffer.Size * sizeof(ImDrawVert)));

                cl.UpdateBuffer(
                    _indexBuffer,
                    indexOffsetInElements * sizeof(ushort),
                    cmd_list.IdxBuffer.Data,
                    (uint)(cmd_list.IdxBuffer.Size * sizeof(ushort)));

                vertexOffsetInVertices += (uint)cmd_list.VtxBuffer.Size;
                indexOffsetInElements += (uint)cmd_list.IdxBuffer.Size;
            }

            // Setup orthographic projection matrix into our constant buffer
            {
                var io = ImGui.GetIO();

                Matrix4x4 mvp = Matrix4x4.CreateOrthographicOffCenter(
                    0f,
                    io.DisplaySize.X,
                    io.DisplaySize.Y,
                    0.0f,
                    -1.0f,
                    1.0f);

                _gd.UpdateBuffer(_projMatrixBuffer, 0, ref mvp);
            }

            cl.SetVertexBuffer(0, _vertexBuffer);
            cl.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
            cl.SetPipeline(_pipeline);
            cl.SetGraphicsResourceSet(0, _mainResourceSet);

            draw_data.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            // Render command lists
            int vtx_offset = 0;
            int idx_offset = 0;
            for (int n = 0; n < draw_data.CmdListsCount; n++)
            {
                ImDrawListPtr cmd_list = draw_data.CmdLists[n];
                for (int cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
                {
                    ImDrawCmdPtr pcmd = cmd_list.CmdBuffer[cmd_i];
                    if (pcmd.UserCallback != IntPtr.Zero)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        if (pcmd.TextureId != IntPtr.Zero)
                        {
                            if (pcmd.TextureId == _fontAtlasID)
                            {
                                cl.SetGraphicsResourceSet(1, _fontTextureResourceSet);
                            }
                            else
                            {
                                throw new NotImplementedException();
                            }
                        }

                        cl.SetScissorRect(
                            0,
                            (uint)pcmd.ClipRect.X,
                            (uint)pcmd.ClipRect.Y,
                            (uint)(pcmd.ClipRect.Z - pcmd.ClipRect.X),
                            (uint)(pcmd.ClipRect.W - pcmd.ClipRect.Y));

                        cl.DrawIndexed(pcmd.ElemCount, 1, pcmd.IdxOffset + (uint)idx_offset, (int)(pcmd.VtxOffset + vtx_offset), 0);
                    }
                }

                idx_offset += cmd_list.IdxBuffer.Size;
                vtx_offset += cmd_list.VtxBuffer.Size;
            }
        }

        /// <summary>
        /// Frees all graphics resources used by the renderer.
        /// </summary>
        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
            _projMatrixBuffer.Dispose();
            _fontTexture.Dispose();
            _vertexShader.Dispose();
            _fragmentShader.Dispose();
            _layout.Dispose();
            _textureLayout.Dispose();
            _pipeline.Dispose();
            _mainResourceSet.Dispose();
            _fontTextureResourceSet.Dispose();
        }
    }
}
