using System.Diagnostics;
using System.Numerics;
using Lutra.Utility;
using Lutra.Utility.Veldrid;
using Veldrid;
using Veldrid.SPIRV;

namespace Lutra.Rendering;

public static class VeldridResources
{
    private static GraphicsDevice _graphicsDevice;
    private static DisposeCollectorResourceFactory _factory;
    private static CommandList _utilityCommandList;

    internal static Window Window;
    internal static bool WindowResized;

    public static GraphicsDevice GraphicsDevice => _graphicsDevice;
    public static ResourceFactory Factory => _factory;

    public static bool IsOpenGL;

    static VeldridResources()
    {
        SixLabors.ImageSharp.Configuration.Default.PreferContiguousImageBuffers = true;
    }

    #region Initialization

    public static void Initialize(Game game, GraphicsBackend? preferredBackend)
    {
        Window = game.Window;

        if (!preferredBackend.HasValue || !GraphicsDevice.IsBackendSupported(preferredBackend.Value))
        {
            preferredBackend = GetDefaultBackend();
        }
        IsOpenGL = preferredBackend == GraphicsBackend.OpenGL || preferredBackend == GraphicsBackend.OpenGLES;

        Console.WriteLine($"Graphics Backend: {Enum.GetName(preferredBackend.Value)}");

        var gdOptions = new GraphicsDeviceOptions()
        {
#if DEBUG
            Debug = true,
#endif
            ResourceBindingModel = ResourceBindingModel.Improved,
            PreferDepthRangeZeroToOne = true,
            SyncToVerticalBlank = true,
        };

        _graphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, gdOptions, preferredBackend.Value);

        _factory = new DisposeCollectorResourceFactory(_graphicsDevice.ResourceFactory);
        _utilityCommandList = _factory.CreateCommandList();

        DEBUG_Initialize();
    }

    private static GraphicsBackend GetDefaultBackend()
    {
        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Direct3D11))
        {
            return GraphicsBackend.Direct3D11;
        }

        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Metal))
        {
            return GraphicsBackend.Metal;
        }

        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.Vulkan))
        {
            return GraphicsBackend.Vulkan;
        }

        if (GraphicsDevice.IsBackendSupported(GraphicsBackend.OpenGL))
        {
            return GraphicsBackend.OpenGL;
        }

        return GraphicsBackend.OpenGLES;
    }

    #endregion

    public static void HandleResize()
    {
        if (Window.Exists && WindowResized)
        {
            WindowResized = false;
            _graphicsDevice.MainSwapchain.Resize((uint)Window.Width, (uint)Window.Height);
            DEBUG_ImGuiRenderer.WindowResized(Window.Width, Window.Height);
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
            texture.Width, texture.Height, 1u, 1u, PixelFormat.R8G8B8A8UNorm, TextureUsage.Staging);
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
        DEBUG_ImGuiDispose();
    }

    #region Factory Methods

    public static DeviceBuffer CreateVertexBuffer(uint size)
    {
        return _factory.CreateBuffer(new BufferDescription(size, BufferUsage.VertexBuffer | BufferUsage.Dynamic));
    }

    public static DeviceBuffer CreateIndexBuffer(uint size)
    {
        return _factory.CreateBuffer(new BufferDescription(size, BufferUsage.IndexBuffer | BufferUsage.Dynamic));
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
            textureSize, textureSize, 1u, 1u, PixelFormat.R8G8B8A8UNorm, TextureUsage.Sampled);
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
        DEBUG_ImGuiRenderer.Update((float)deltaTime);
    }

    [Conditional("DEBUG")]
    internal static void DEBUG_ImGuiDispose()
    {
        DEBUG_ImGuiRenderer.DestroyDeviceObjects();
    }

    #endregion
}
