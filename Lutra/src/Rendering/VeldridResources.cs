using System.Diagnostics;
using System.Numerics;
using Lutra.Input;
using Lutra.Utility;
using Veldrid;
using Veldrid.ImageSharp;
using Veldrid.Sdl2;
using Veldrid.SPIRV;
using Veldrid.StartupUtilities;
using Veldrid.Utilities;

namespace Lutra.Rendering;

public static class VeldridResources
{
    private static GraphicsDevice _graphicsDevice;
    private static DisposeCollectorResourceFactory _factory;
    private static CommandList _utilityCommandList;
    private static bool _windowResized;

    internal static Sdl2Window Sdl2Window;
    internal static Action OnResize = delegate { };

    public static GraphicsDevice GraphicsDevice => _graphicsDevice;
    public static ResourceFactory Factory => _factory;

    public static bool IsOpenGL;

    public static void Initialize(Game game, GraphicsBackend? preferredBackend)
    {
        if (!preferredBackend.HasValue || !GraphicsDevice.IsBackendSupported(preferredBackend.Value))
        {
            preferredBackend = GetDefaultBackend();
        }

        System.Console.WriteLine($"Graphics Backend: {Enum.GetName(preferredBackend.Value)}");

        var windowCreateInfo = new WindowCreateInfo(100, 100, game.Window.Width, game.Window.Height, WindowState.Normal, game.Window.Title);

        var gdOptions = new GraphicsDeviceOptions()
        {
#if DEBUG
            Debug = true,
#endif
            ResourceBindingModel = ResourceBindingModel.Improved,
            PreferDepthRangeZeroToOne = true,
            SyncToVerticalBlank = true,
        };

        VeldridStartup.CreateWindowAndGraphicsDevice(windowCreateInfo, gdOptions, preferredBackend.Value, out Sdl2Window, out _graphicsDevice);
        IsOpenGL = _graphicsDevice.BackendType == Veldrid.GraphicsBackend.OpenGL || _graphicsDevice.BackendType == Veldrid.GraphicsBackend.OpenGLES;

        Sdl2Window.Resized += () => _windowResized = true;
        OnResize += game.Window.OnResized;

        _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
        _utilityCommandList = _factory.CreateCommandList();

        DEBUG_Initialize();
    }

    public static void HandleResize()
    {
        if (Sdl2Window.Exists && _windowResized)
        {
            _windowResized = false;
            _graphicsDevice.MainSwapchain.Resize((uint)Sdl2Window.Width, (uint)Sdl2Window.Height);
            OnResize?.Invoke();
        }
    }

    public static Texture CloneTexture(Texture texture)
    {
        var newDesc = TextureDescription.Texture2D(texture.Width, texture.Height, texture.MipLevels, texture.ArrayLayers, texture.Format, texture.Usage);
        var newTex = _factory.CreateTexture(ref newDesc);

        _utilityCommandList.Begin();
        _utilityCommandList.CopyTexture(texture, newTex);
        _utilityCommandList.End();
        GraphicsDevice.SubmitCommands(_utilityCommandList);
        GraphicsDevice.WaitForIdle();

        return newTex;
    }

    public static Texture EnlargeTexture(Texture texture, uint width, uint height)
    {
        if (texture.Width >= width || texture.Height >= height)
        {
            Util.LogError($"Cannot enlarge a {texture.Width}x{texture.Height} texture to {width}x{height}");
            return texture;
        }

        var newDesc = TextureDescription.Texture2D(width, height, texture.MipLevels, texture.ArrayLayers, texture.Format, texture.Usage);
        var newTex = _factory.CreateTexture(ref newDesc);

        _utilityCommandList.Begin();
        _utilityCommandList.CopyTexture(texture, 0, 0, 0, 0, 0, newTex, 0, 0, 0, 0, 0, texture.Width, texture.Height, 1, 1);
        _utilityCommandList.End();
        GraphicsDevice.SubmitCommands(_utilityCommandList);
        GraphicsDevice.WaitForIdle();

        return newTex;
    }

    public static MappedResourceView<byte> GetMappedTexture(Texture texture)
    {
        var stagingDesc = TextureDescription.Texture2D(
            texture.Width, texture.Height, 1u, 1u, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Staging);
        var stagingTexture = _factory.CreateTexture(ref stagingDesc);

        _utilityCommandList.Begin();
        _utilityCommandList.CopyTexture(texture, stagingTexture);
        _utilityCommandList.End();
        GraphicsDevice.SubmitCommands(_utilityCommandList);
        GraphicsDevice.WaitForIdle();

        return GraphicsDevice.Map<byte>(stagingTexture, MapMode.Read);
    }

    public static void UpdateTexture<T>(Texture texture, T[] data, RectInt bounds) where T : unmanaged
    {
        if (bounds.Width == 0 || bounds.Height == 0)
        {
            Util.LogWarning("Tried to update a zero size region of a texture.");
            return;
        }

        _graphicsDevice.UpdateTexture(texture, data, (uint)bounds.X, (uint)bounds.Y, 0u, (uint)bounds.Width, (uint)bounds.Height, 1u, 0u, 0u);
    }

    public static void ShutDown()
    {
        _graphicsDevice.WaitForIdle();
        _factory.DisposeCollector.DisposeAll();
        _graphicsDevice.Dispose();
    }

    public static GraphicsBackend GetDefaultBackend()
    {
        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan))
        {
            return GraphicsBackend.Vulkan;
        }

        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11))
        {
            return GraphicsBackend.Direct3D11;
        }

        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGL))
        {
            return GraphicsBackend.OpenGL;
        }

        return GraphicsBackend.OpenGLES;
    }

    #region Factory Methods

    public static DeviceBuffer CreateVertexBuffer(uint size)
    {
        return _factory.CreateBuffer(new BufferDescription(size, BufferUsage.VertexBuffer));
    }

    public static DeviceBuffer CreateIndexBuffer(uint size)
    {
        return _factory.CreateBuffer(new BufferDescription(size, BufferUsage.IndexBuffer));
    }

    public static DeviceBuffer CreateMatrixUniformBuffer()
    {
        return _factory.CreateBuffer(new BufferDescription(64u, BufferUsage.UniformBuffer));
    }

    public static DeviceBuffer CreateStructuredBuffer(uint size, uint byteStride)
    {
        return _factory.CreateBuffer(new BufferDescription(
            size, BufferUsage.StructuredBufferReadOnly | BufferUsage.Dynamic, byteStride
        ));
    }

    public static Shader[] CreateShaders(byte[] vertexBytes, byte[] fragmentBytes)
    {
        var shaders = _factory.CreateFromSpirv(
            new ShaderDescription(ShaderStages.Vertex, vertexBytes, "main"),
            new ShaderDescription(ShaderStages.Fragment, fragmentBytes, "main")
        );

        _factory.DisposeCollector.Add(shaders);

        return shaders;
    }

    public static Texture CreateSurfaceTexture(uint width, uint height)
    {
        var pixelFormat = _graphicsDevice.SwapchainFramebuffer.OutputDescription.ColorAttachments[0].Format;
        var desc = TextureDescription.Texture2D(
            width, height, 1u, 1u, pixelFormat, TextureUsage.Sampled | TextureUsage.RenderTarget);
        return _factory.CreateTexture(ref desc);
    }

    public static Texture CreateSquareTexture(uint textureSize)
    {
        var desc = TextureDescription.Texture2D(
            textureSize, textureSize, 1u, 1u, PixelFormat.R8_G8_B8_A8_UNorm, TextureUsage.Sampled);
        return _factory.CreateTexture(ref desc);
    }

    public static Texture CreateTexture(ImageSharpTexture imageSharpTexture)
    {
        return imageSharpTexture.CreateDeviceTexture(_graphicsDevice, _factory);
    }

    public static Framebuffer CreateFramebuffer(Texture colorTarget)
    {
        return _factory.CreateFramebuffer(new FramebufferDescription(null, colorTarget));
    }

    public static Matrix4x4 CreateOrthographicProjection(float width, float height)
    {
        if (_graphicsDevice.IsClipSpaceYInverted)
        {
            return Matrix4x4.CreateOrthographicOffCenter(0, width, 0, height, 0, 1);
        }
        else
        {
            return Matrix4x4.CreateOrthographicOffCenter(0, width, height, 0, 0, 1);
        }
    }

    #endregion

    #region DEBUG

    private static ImGuiRenderer DEBUG_ImGuiRenderer;
    private static CommandList DEBUG_CommandList;

    [Conditional("DEBUG")]
    private static void DEBUG_Initialize()
    {
        DEBUG_CommandList = _factory.CreateCommandList();
        DEBUG_ImGuiRenderer = new ImGuiRenderer(_graphicsDevice, _graphicsDevice.SwapchainFramebuffer.OutputDescription,
            (int)_graphicsDevice.SwapchainFramebuffer.Width, (int)_graphicsDevice.SwapchainFramebuffer.Height);

        OnResize += () => DEBUG_ImGuiRenderer.WindowResized(Sdl2Window.Width, Sdl2Window.Height);
    }

    [Conditional("DEBUG")]
    internal static void DEBUG_ImGuiRender()
    {
        DEBUG_CommandList.Begin();
        DEBUG_CommandList.SetFramebuffer(_graphicsDevice.SwapchainFramebuffer);
        DEBUG_ImGuiRenderer.Render(_graphicsDevice, DEBUG_CommandList);
        DEBUG_CommandList.End();
        _graphicsDevice.SubmitCommands(DEBUG_CommandList);
    }

    [Conditional("DEBUG")]
    internal static void DEBUG_ImGuiUpdate(double deltaTime)
    {
        DEBUG_ImGuiRenderer!.Update((float)deltaTime, InputManager.DEBUG_LatestInput);
    }

    #endregion
}
