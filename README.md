# Lutra Game Framework

<img align="right" src="Lutra.Examples/Assets/lutra.png">

Lutra is a framework for making 2D games in C# with a focus on cross-platform builds, pixel perfect rendering, and ease of use.
It adapts code from (and functions as a spiritual successor to) [Kyle Pulver's Otter framework](https://web.archive.org/web/20240324143723/http://otter2d.com/).

This project was started with the aim to port games away from Otter's dependency on SFML.NET, due to various cross-platform runtime issues.
The renderer was built from scratch using [Veldrid](http://veldrid.dev/), which provides a low-level graphics API with many backends.

## Current status

Lutra is still in initial development, but has been used to make games already! 
During these early stages, dev work is being done in a private repository and synced here for releases.

You can either clone this repo or download the source of the latest release from the [tags page](https://github.com/emmyleaf/Lutra/tags).

See the [changelog](./CHANGELOG.md) for more details about the latest release.

## Future plans

* A new cross-platform renderer potentially based on [kelp-2d](https://github.com/emmyleaf/kelp-2d)
* An optional ECS game structure, slotting in beside the traditional Otter-like one
* Better Otter compatibility and more Otter features, for ease of porting

## Features

* Quick set up
* Scene-Entity-Component structure
* Quadtree based Collision
* Rich text with fancy formatting options
* ImGui based debug UI with console
* Useful utility functions
* Custom shader support (guide coming soon!)
* Instanced rendering for particles (to be extended to other basic graphics!)

## Platform support

| Platform[^1] | Vulkan             | Direct3D 11        | Metal          | OpenGL  | OpenGL ES |
| ---          | ---                | ---                | ---            | ---     | ---       |
| Windows x64  | :white_check_mark: | :white_check_mark: |                | :ok:    | :ok:      |
| Linux x64    | :white_check_mark: |                    |                | :ok:    | :ok:      |
| macOS x64    |                    |                    | :construction: | :x:[^2] |           |

:white_check_mark:: Recommended, :ok:: Tested, :construction:: Support in progress, :x:: Non-functional.

[^1]: Testing was done on Windows 10, Arch Linux, and macOS 11 Big Sur.
[^2]: OpenGL on macOS is deprecated and does not support all the features required for Lutra.

## Porting games from Otter

Lutra and Otter do not have identical featuresets, but porting games from Otter to Lutra is a goal of the project.

* [TMFBMA](https://drmelon.itch.io/tmfbma-demo) was fully ported within a week, while completing Lutra version 0.1.0.
* [DA-DA-DASHBONK](https://drmelon.itch.io/da-da-dashbonk) was fully ported within 2 days, while completing Lutra version 0.2.0.

See the [PORTING article](./PORTING.md) for more information.

## License

Lutra is licensed under the [MIT License](./LICENSE).
