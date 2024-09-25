namespace Lutra.Examples
{
    public class AssetArchiveTestScene : Scene
    {
        public override void Begin()
        {
            base.Begin();

            string tempPath = System.IO.Path.GetTempPath();

            Assets.Archive.ConstructFromDirectoryAndSave("Assets/", $"{tempPath}archiveTest.lta", 0, false);
            Assets.Archive.ConstructFromDirectoryAndSave("Assets/", $"{tempPath}archiveTestCompressed.lta", 0, true);
            Assets.Archive loadedArchive = Assets.Archive.LoadFromFile($"{tempPath}archiveTest.lta");
            Assets.Archive loadedCompressedArchive = Assets.Archive.LoadFromFile($"{tempPath}archiveTestCompressed.lta");

            Utility.Util.LogInfo($"Archive Contains Assets/LType/BG.png? {loadedArchive.ContainsFile("Assets/LType/BG.png")}");
            Utility.Util.LogInfo($"Compressed Archive Contains Assets/lutra.png? {loadedCompressedArchive.ContainsFile("Assets/lutra.png")}");

            using (var memoryStream = loadedArchive.GetStreamForFile("Assets/LType/BG.png"))
            {
                Lutra.Rendering.LutraTexture tex = new Rendering.LutraTexture(memoryStream);
                Lutra.Graphics.Image img = new Graphics.Image(tex);
                AddGraphic(img);
            }

            using (var memoryStream = loadedCompressedArchive.GetStreamForFile("Assets/lutra.png"))
            {
                Lutra.Rendering.LutraTexture tex = new Rendering.LutraTexture(memoryStream);
                Lutra.Graphics.Image img = new Graphics.Image(tex);
                AddGraphic(img);
            }

            using (var memoryStream = loadedCompressedArchive.GetStreamForFile("Assets/TransparentTester.png"))
            {
                Lutra.Rendering.LutraTexture tex = new Rendering.LutraTexture(memoryStream);
                Lutra.Graphics.Image img = new Graphics.Image(tex);
                AddGraphic(img);
            }
        }
    }
}
