# Porting from Otter

This doc contains tips for porting a game from Otter to Lutra.
Unfortunately it's not complete yet, but should provide a starting point to work from.

## Namespaces

Lutra has a lot of the same classes and utility classes as Otter, but under more specific (or different) namespaces.
Try importing relevant namespaces if imports are no longer being found, eg. `Lutra.Utility`.
The vector types used by Lutra come from `System.Numerics` unlike Otter which has its own.
Here is a list of namespaces you could replace `using Otter;` with and allow your IDE to remove the unnecessary ones:
```
using Lutra;
using Lutra.Collision;
using Lutra.Components;
using Lutra.Entities;
using Lutra.Input;
using Lutra.Rendering;
using Lutra.Graphics;
using Lutra.Utility;
using Lutra.Utility.Collections;
using System.Numerics;
```

## Changed function/property names

Here is a list of common changed names that were found when porting a project from Otter to Lutra.

* `Util.Lerp` is now `MathHelper.Lerp`, alongside many other useful maths functions for gamedev
* `Vector2.Normalize` is now static and doesn't modify the Vector2, so `vec.Normalize()` would become `vec = Vector2.Normalize(vec)`
* `scene.GetEntitiesAll()` is now `scene.Entities`
* Color constructor from hex string is now static method `Color.FromString(str)`

TODO: Soon, I will take another pass over the changes made to TMFBMA and update this section with more of the changes.

## Deprecations

Some features have been kept in for Otter compatibility, but tagged as obsolete/deprecated.
I don't intend for these to be used in new games using Lutra, but were helpful for porting TMFBMA from Otter.

* `Layer` can still be set on Entities but this is deprecated. `Layer` should now be set on individual Graphics, but will fall back to the Entity's `Layer` value if not set.
* `Surface` can still be set on Entities but this is deprecated. `Surfaces` should now be added on individual Graphics, but will fall back to the Entity's `Surface` value if that list is empty.
* Setting Camera properties directly on `Scene` is now deprecated in favour of `CameraManager`.
* `Util.Log` with `String.Format` params is deprecated in favour of log level specific methods without params.

## Input system

The input system has been significantly redesigned in Lutra. It should still be just as flexible as Otter's, including creating virtual controllers that can aggregate various inputs.
`Controller` now refers to a physical controller, and all the virtual components are named as such: `VirtualController`, `VirtualButton`, etc.

## Audio

Lutra has no built in audio systems, but has optional audio libraries.
* [`Lutra.Audio.OpenAL`](./Lutra.Audio.OpenAL) is very basic, but provides Sound and Music classes that implement a decent subset of Otter's functionality.
* [`Lutra.FMODAudio`](https://github.com/emmyleaf/Lutra.FMODAudio) is an FMOD integration library.

## Window

Window properties are no longer on `Game`, but on the `Window` property of `Game`. For example, `MouseVisible`, `LockIntegerScale`, `Title` etc.
