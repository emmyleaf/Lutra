// Used from Veldrid.StartupUtilities under the MIT license.
// Copyright (c) 2017 Eric Mellino and Veldrid contributors.

using System.Diagnostics;
using Veldrid;
using Veldrid.OpenGL;
using SDL;
using Lutra.Rendering;

namespace Lutra.Utility.Veldrid
{
    public static class VeldridStartup
    {
        public static GraphicsDevice CreateGraphicsDevice(
            Window window,
            GraphicsDeviceOptions options,
            GraphicsBackend preferredBackend)
        {
            return preferredBackend switch
            {
                GraphicsBackend.Direct3D11 => CreateDefaultD3D11GraphicsDevice(options, window),
                GraphicsBackend.Vulkan => CreateVulkanGraphicsDevice(options, window),
                GraphicsBackend.OpenGL => CreateDefaultOpenGLGraphicsDevice(options, window, preferredBackend),
                GraphicsBackend.Metal => CreateMetalGraphicsDevice(options, window),
                GraphicsBackend.OpenGLES => CreateDefaultOpenGLGraphicsDevice(options, window, preferredBackend),
                _ => throw new VeldridException("Invalid GraphicsBackend: " + preferredBackend),
            };
        }

        public static unsafe SwapchainSource GetSwapchainSource(Window window)
        {
            var propertiesID = SDL3.SDL_GetWindowProperties(window.SdlWindowHandle);
            var videoDriver = SDL3.SDL_GetCurrentVideoDriver();

            switch (videoDriver)
            {
                case "windows":
                    var hwnd = SDL3.SDL_GetPointerProperty(propertiesID, SDL3.SDL_PROP_WINDOW_WIN32_HWND_POINTER, 0);
                    var hinstance = SDL3.SDL_GetPointerProperty(propertiesID, SDL3.SDL_PROP_WINDOW_WIN32_INSTANCE_POINTER, 0);
                    // Console.WriteLine($"Windows - hwnd: {hwnd}, hinstance: {hinstance}");
                    return SwapchainSource.CreateWin32(hwnd, hinstance);
                case "x11":
                    var xDisplay = SDL3.SDL_GetPointerProperty(propertiesID, SDL3.SDL_PROP_WINDOW_X11_DISPLAY_POINTER, 0);
                    var windowNo = SDL3.SDL_GetNumberProperty(propertiesID, SDL3.SDL_PROP_WINDOW_X11_WINDOW_NUMBER, 0);
                    // Console.WriteLine($"X11 - display: {xDisplay}, window: {windowNo}");
                    return SwapchainSource.CreateXlib(xDisplay, (nint)windowNo); // TODO: test if this is right!
                case "wayland":
                    var wDisplay = SDL3.SDL_GetPointerProperty(propertiesID, SDL3.SDL_PROP_WINDOW_WAYLAND_DISPLAY_POINTER, 0);
                    var surface = SDL3.SDL_GetPointerProperty(propertiesID, SDL3.SDL_PROP_WINDOW_WAYLAND_SURFACE_POINTER, 0);
                    // Console.WriteLine($"Wayland - display: {wDisplay}, surface: {surface}");
                    return SwapchainSource.CreateWayland(wDisplay, surface);
                case "cocoa":
                    var nsWindow = SDL3.SDL_GetPointerProperty(propertiesID, SDL3.SDL_PROP_WINDOW_COCOA_WINDOW_POINTER, 0);
                    // Console.WriteLine($"macOS - NSWindow: {nsWindow}");
                    return SwapchainSource.CreateNSWindow(nsWindow);
                default:
                    throw new PlatformNotSupportedException($"Cannot create a SwapchainSource for {videoDriver}.");
            }
        }

#if !EXCLUDE_METAL_BACKEND
        private static unsafe GraphicsDevice CreateMetalGraphicsDevice(GraphicsDeviceOptions options, Window window)
            => CreateMetalGraphicsDevice(options, window, options.SwapchainSrgbFormat);
        private static unsafe GraphicsDevice CreateMetalGraphicsDevice(
            GraphicsDeviceOptions options,
            Window window,
            bool colorSrgb)
        {
            SwapchainSource source = GetSwapchainSource(window);
            SwapchainDescription swapchainDesc = new SwapchainDescription(
                source,
                (uint)window.Width, (uint)window.Height,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                colorSrgb);

            return GraphicsDevice.CreateMetal(options, swapchainDesc);
        }
#endif

#if !EXCLUDE_VULKAN_BACKEND
        public static unsafe GraphicsDevice CreateVulkanGraphicsDevice(GraphicsDeviceOptions options, Window window)
            => CreateVulkanGraphicsDevice(options, window, false);
        public static unsafe GraphicsDevice CreateVulkanGraphicsDevice(
            GraphicsDeviceOptions options,
            Window window,
            bool colorSrgb)
        {
            SwapchainSource source = GetSwapchainSource(window);
            SwapchainDescription scDesc = new SwapchainDescription(
                source,
                (uint)window.Width,
                (uint)window.Height,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                colorSrgb);
            GraphicsDevice gd = GraphicsDevice.CreateVulkan(options, scDesc);

            return gd;
        }
#endif

#if !EXCLUDE_OPENGL_BACKEND
        public static unsafe GraphicsDevice CreateDefaultOpenGLGraphicsDevice(
            GraphicsDeviceOptions options,
            Window window,
            GraphicsBackend backend)
        {
            SDL3.SDL_ClearError();
            var sdlHandle = window.SdlWindowHandle;

            SetSDLGLContextAttributes(options, backend);

            var contextHandle = SDL3.SDL_GL_CreateContext(sdlHandle);
            var error = SDL3.SDL_GetError();
            if (!string.IsNullOrEmpty(error))
            {
                throw new VeldridException($"Unable to create OpenGL Context: \"{error}\". This may indicate that the system does not support the requested OpenGL profile, version, or Swapchain format.");
            }

            int actualDepthSize, actualStencilSize;
            SDL3.SDL_GL_GetAttribute(SDL_GLattr.SDL_GL_DEPTH_SIZE, &actualDepthSize);
            SDL3.SDL_GL_GetAttribute(SDL_GLattr.SDL_GL_STENCIL_SIZE, &actualStencilSize);

            SDL3.SDL_GL_SetSwapInterval(options.SyncToVerticalBlank ? 1 : 0);

            OpenGLPlatformInfo platformInfo = new(
                (IntPtr)contextHandle,
                proc => SDL3.SDL_GL_GetProcAddress(proc),
                context => SDL3.SDL_GL_MakeCurrent(sdlHandle, (SDL_GLContextState*)context),
                () => (IntPtr)SDL3.SDL_GL_GetCurrentContext(),
                () => SDL3.SDL_GL_MakeCurrent((SDL_Window*)IntPtr.Zero, (SDL_GLContextState*)IntPtr.Zero),
                context => SDL3.SDL_GL_DestroyContext((SDL_GLContextState*)context),
                () => SDL3.SDL_GL_SwapWindow(sdlHandle),
                sync => SDL3.SDL_GL_SetSwapInterval(sync ? 1 : 0));

            return GraphicsDevice.CreateOpenGL(options, platformInfo, (uint)window.Width, (uint)window.Height);
        }

        public static unsafe void SetSDLGLContextAttributes(GraphicsDeviceOptions options, GraphicsBackend backend)
        {
            if (backend != GraphicsBackend.OpenGL && backend != GraphicsBackend.OpenGLES)
            {
                throw new VeldridException(
                    $"{nameof(backend)} must be {nameof(GraphicsBackend.OpenGL)} or {nameof(GraphicsBackend.OpenGLES)}.");
            }

            SDL_GLcontextFlag contextFlags = options.Debug
                ? SDL_GLcontextFlag.SDL_GL_CONTEXT_DEBUG_FLAG | SDL_GLcontextFlag.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG
                : SDL_GLcontextFlag.SDL_GL_CONTEXT_FORWARD_COMPATIBLE_FLAG;

            SDL3.SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_FLAGS, (int)contextFlags);

            var gles = backend == GraphicsBackend.OpenGLES;
            (int major, int minor) = GetMaxGLVersion(gles);
            var profile = gles ? SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_ES : SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE;

            SDL3.SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)profile);
            SDL3.SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, major);
            SDL3.SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, minor);

            int depthBits = 0;
            int stencilBits = 0;
            if (options.SwapchainDepthFormat.HasValue)
            {
                switch (options.SwapchainDepthFormat)
                {
                    case PixelFormat.R16UNorm:
                        depthBits = 16;
                        break;
                    case PixelFormat.D24UNormS8UInt:
                        depthBits = 24;
                        stencilBits = 8;
                        break;
                    case PixelFormat.R32Float:
                        depthBits = 32;
                        break;
                    case PixelFormat.D32FloatS8UInt:
                        depthBits = 32;
                        stencilBits = 8;
                        break;
                    default:
                        throw new VeldridException("Invalid depth format: " + options.SwapchainDepthFormat.Value);
                }
            }

            SDL3.SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_DEPTH_SIZE, depthBits);
            SDL3.SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_STENCIL_SIZE, stencilBits);

            var srgb = options.SwapchainSrgbFormat ? 1 : 0;
            SDL3.SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_FRAMEBUFFER_SRGB_CAPABLE, srgb);
        }
#endif

#if !EXCLUDE_D3D11_BACKEND
        public static GraphicsDevice CreateDefaultD3D11GraphicsDevice(
            GraphicsDeviceOptions options,
            Window window)
        {
            SwapchainSource source = GetSwapchainSource(window);
            SwapchainDescription swapchainDesc = new SwapchainDescription(
                source,
                (uint)window.Width, (uint)window.Height,
                options.SwapchainDepthFormat,
                options.SyncToVerticalBlank,
                options.SwapchainSrgbFormat);

            return GraphicsDevice.CreateD3D11(options, swapchainDesc);
        }
#endif

#if !EXCLUDE_OPENGL_BACKEND
        private static readonly object s_glVersionLock = new object();
        private static (int Major, int Minor)? s_maxSupportedGLVersion;
        private static (int Major, int Minor)? s_maxSupportedGLESVersion;

        private static (int Major, int Minor) GetMaxGLVersion(bool gles)
        {
            lock (s_glVersionLock)
            {
                (int Major, int Minor)? maxVer = gles ? s_maxSupportedGLESVersion : s_maxSupportedGLVersion;
                if (maxVer == null)
                {
                    maxVer = TestMaxVersion(gles);
                    if (gles) { s_maxSupportedGLESVersion = maxVer; }
                    else { s_maxSupportedGLVersion = maxVer; }
                }

                return maxVer.Value;
            }
        }

        private static (int Major, int Minor) TestMaxVersion(bool gles)
        {
            (int, int)[] testVersions = gles
                ? new[] { (3, 2), (3, 0) }
                : new[] { (4, 6), (4, 3), (4, 0), (3, 3), (3, 0) };

            foreach ((int major, int minor) in testVersions)
            {
                if (TestIndividualGLVersion(gles, major, minor)) { return (major, minor); }
            }

            return (0, 0);
        }

        private static unsafe bool TestIndividualGLVersion(bool gles, int major, int minor)
        {
            var profileMask = gles ? SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_ES : SDL_GLprofile.SDL_GL_CONTEXT_PROFILE_CORE;

            SDL3.SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_PROFILE_MASK, (int)profileMask);
            SDL3.SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_MAJOR_VERSION, major);
            SDL3.SDL_GL_SetAttribute(SDL_GLattr.SDL_GL_CONTEXT_MINOR_VERSION, minor);

            var flags = SDL_WindowFlags.SDL_WINDOW_HIDDEN | SDL_WindowFlags.SDL_WINDOW_OPENGL;
            SDL_Window* window = SDL3.SDL_CreateWindow(string.Empty, 1, 1, flags);
            var error = SDL3.SDL_GetError();

            if (!string.IsNullOrEmpty(error))
            {
                SDL3.SDL_ClearError();
                Debug.WriteLine($"Unable to create version {major}.{minor} {profileMask} context.");
                return false;
            }

            var context = SDL3.SDL_GL_CreateContext(window);
            error = SDL3.SDL_GetError();

            if (!string.IsNullOrEmpty(error))
            {
                SDL3.SDL_ClearError();
                Debug.WriteLine($"Unable to create version {major}.{minor} {profileMask} context.");
                SDL3.SDL_DestroyWindow(window);
                return false;
            }

            SDL3.SDL_GL_DestroyContext(context);
            SDL3.SDL_DestroyWindow(window);
            return true;
        }
#endif
    }
}
