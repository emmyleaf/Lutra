## 0.2.0
* `Surface` now implements the majority of the functionality it had in Otter. This includes:
    * Extends `Image` so can now be used as a `Graphic`.
    * `ClearColor` and `AutoClear` properties.
    * `Draw` method which draws Graphics directly to the `Surface`.
    * `UseSceneCamera` property, which when set to false applies a neutral camera on render.
* `Graphic.Layer` can fall back to its Entity's `Layer` value if not set (Otter compat).
* `Graphic` can have a set of `Surface`s to draw to, which can also fall back to a value set on its Entity (Otter compat).
* `SpriteGraphic.Smooth` property now applies linear sampling when true.
* `SpriteGraphic.Blend` property can now be set to either `Alpha` or `Add` blend modes.
* `FontPageTexture` can now enlarge its texture when full.
* `LutraTexture` has a new constructor taking another `LutraTexture`, which clones the texture.
* `SpriteShader` renamed to `ShaderData`, plus more types allowed as uniform buffers.
* `Scene.GetEntitiesWithCollider` added, equivalent to Otter's `Scene.GetEntities` method, taking a collider tag.
* Fix `VirtualAxis` buttons getting stuck on while disabled.
* Fix `Window.SetFullscreen` not resizing the window correctly.
* Fix debug console messages displaying incorrectly when containing '%'.

## 0.1.0
* Initial public release
