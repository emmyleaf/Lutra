# Lutra Game Framework :otter:

<img align="right" src="Lutra.Examples/Assets/lutra.png">

Lutra is a framework for making 2D games in C# with a focus on cross-platform builds, pixel perfect rendering, and ease of use.
It adapts code from (and functions as a spiritual successor to) [Kyle Pulver's Otter framework](https://web.archive.org/web/20240324143723/http://otter2d.com/).

This project was started with the aim to port games away from Otter's dependency on SFML.NET, due to various cross-platform runtime issues.
The renderer was built from scratch using [Veldrid](http://veldrid.dev/), which provides a low-level graphics API with many backends.

## Current status

Lutra is still in development, but [has been used to make games already](#games-made-with-lutra)!
During these early stages, dev work is being done in a private repository and synced here for releases.

You can either clone this repo or download the source of the latest release from the [tags page](https://github.com/emmyleaf/Lutra/tags).

See the [changelog](./CHANGELOG.md) for more details about the latest release.

## Future plans

- A new renderer using SDL3's GPU API
- Support Windows/macOS/Linux across both x64 and arm64
- Better Otter compatibility and more Otter features, for ease of porting
- An optional ECS game structure, slotting in beside the traditional object-oriented one

## Features

- Code-first & quick to set up
- Object-oriented Scene/Entity/Component structure
- Quadtree based Collision
- Rich text with fancy formatting options
- ImGui based debug UI with console
- Useful utility functions
- Custom shader support (guide coming soon!)

## Platform support

| Platform[^1] | Vulkan             | Direct3D 11        | Metal              | OpenGL             |
| ------------ | ------------------ | ------------------ | ------------------ | ------------------ |
| Windows x64  | :ok:[^2]           | :white_check_mark: |                    | :white_check_mark: |
| Linux x64    | :white_check_mark: |                    |                    | :white_check_mark: |
| macOS x64    |                    |                    | :white_check_mark: | :x:[^3]            |

:white_check_mark:: Recommended, :ok:: Tested, :construction:: Support in progress, :x:: Non-functional.

[^1]: Testing was done on Windows 10, Arch Linux, and macOS 15 Sequoia.
[^2]: Vulkan on Windows has been found to have issues with certain AMD hardware.
[^3]: OpenGL on macOS is deprecated and does not support all the features required for Lutra.

## Games made with Lutra!

- [Bean To Me!](https://store.steampowered.com/app/3001670/Bean_To_Me/) by [DrMelon](https://melon.zone) at [Very Evil Demons](https://veryevildemons.com)

## Porting games from Otter

Lutra and Otter do not have identical featuresets, but porting games from Otter to Lutra is a goal of the project.

See the [OTTER article](./OTTER.md) for more information.

## Licenses

Lutra is licensed under the [MIT License](./LICENSE).

Lutra makes use of the following open source dependencies:

- [ImGui.NET](https://github.com/ImGuiNET/ImGui.NET) (MIT License) & [Dear ImGui](https://github.com/ocornut/imgui) (MIT License)
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp) (Apache 2.0 License)
- [SpaceWizards.SharpFont](https://github.com/space-wizards/SharpFont) (MIT License) & [FreeType](https://freetype.org) (FreeType License)
- [ppy.SDL3-CS](https://github.com/ppy/SDL3-CS) (MIT License) & [SDL3](https://github.com/libsdl-org/SDL) (Zlib License)
- [ppy.Veldrid](https://github.com/ppy/veldrid) (MIT License)
- [ZstdSharp.Port](https://github.com/oleg-st/ZstdSharp) (MIT License)
