using System.Collections.Generic;

namespace Lutra;

public class SceneSystem
{
    private Game _game;

    public Stack<Scene> Stack = new();
    public Scene SwitchToScene;
    public Queue<Scene> ScenesToAdd = new();
    public int RemoveSceneCount;

    internal SceneSystem(Game game)
    {
        _game = game;
    }

    public void UpdateStack()
    {
        if (SwitchToScene == null)
        {
            while (RemoveSceneCount > 0)
            {
                if (Stack.TryPop(out var poppedScene))
                {
                    poppedScene.InternalEnd();

                    if (Stack.TryPeek(out var currentScene))
                    {
                        currentScene.InternalResume();
                    }
                }

                RemoveSceneCount--;
            }

            while (ScenesToAdd.TryDequeue(out var addingScene))
            {
                if (Stack.TryPeek(out var currentScene))
                {
                    currentScene.InternalPause();
                }

                Stack.Push(addingScene);
                addingScene.InternalBegin(_game);

                if (ScenesToAdd.Count > 0)
                {
                    addingScene.InternalUpdate();
                }
            }
        }
        else
        {
            while (Stack.TryPop(out var poppedScene))
            {
                poppedScene.InternalEnd();
            }

            Stack.Push(SwitchToScene);
            SwitchToScene.InternalBegin(_game);

            SwitchToScene = null;
            RemoveSceneCount = 0;
            ScenesToAdd.Clear();
        }

        Stack.TryPeek(out var newScene);
        _game.Scene = newScene;
    }

    public void EndAllScenes()
    {
        while (Stack.TryPop(out var poppedScene))
        {
            poppedScene.InternalEnd();
        }
    }
}
