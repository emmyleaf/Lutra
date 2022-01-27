# Lutra Game Framework

Lutra is a framework for making 2D games in C# with a focus on cross-platform builds, pixel perfect rendering, and ease of use.
It adapts some code from (and functions as a spiritual successor to) [Kyle Pulver's Otter framework][1].

Basically, I wanted to port games away from Otter's dependency on SFML.NET, due to various cross-platform porting issues.
The renderer was built from scratch using [Veldrid][2], which provides a low-level graphics API with many backends.

## Current status

Lutra is still at an early stage of development, but can definitely be used to make games as is. [TMFBMA][3] was fully ported within a week, while also driving out features for Lutra.

## Porting games from Otter

Lutra doesn't provide all the features of Otter, but does build on it with plenty of new features.
Therefore porting from Otter is not trivial, but definitely possible.
See the [PORTING article][4] for more information.

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

Each of the following configurations has been tested on hardware:

| Platform    | Supported backends                     |
| ---         | ---                                    |
| Windows x64 | Vulkan, Direct3D 11, OpenGL, OpenGL ES |
| Linux x64   | Vulkan, OpenGL, OpenGL ES              |
| macOS x64   | OpenGL                                 |

Note: macOS/x64/Metal should in theory work fine thanks to Veldrid, however testing it may require newer hardware than I have available.

## License

Lutra is licensed under the [MIT License][5].

[1]: http://otter2d.com/
[2]: http://veldrid.dev/
[3]: https://drmelon.itch.io/tmfbma-demo
[4]: ./PORTING.md
[5]: ./LICENSE
