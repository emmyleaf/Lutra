using ImGuiNET;
using Lutra.Collision;
using Lutra.Utility;

namespace Lutra.Examples
{
    public class ColliderTestScene : Scene
    {
        Entity CollidingEntA;
        BoxCollider BoxColliderA;

        Entity CollidingEntB;
        CircleCollider CircleColliderB;

        bool DrawQuadTree;
        bool DrawQuadTreeLeaves;

        public override void Begin()
        {
            CollidingEntA = new Entity(100, 100);
            BoxColliderA = new BoxCollider(50, 50, 1);
            BoxColliderA.CenterOrigin();
            CollidingEntA.AddCollider(BoxColliderA);

            CollidingEntB = new Entity(100, 100);
            CircleColliderB = new CircleCollider(25, 1);
            CircleColliderB.CenterOrigin();
            CollidingEntB.AddCollider(CircleColliderB);

            Add(CollidingEntA);
            Add(CollidingEntB);

            IMGUIDraw += ColliderDebugUIAndDebugDraw;
        }

        private void ColliderDebugUIAndDebugDraw()
        {
            ImGui.Begin("Colliders");

            ImGui.Checkbox("Draw Quadtree", ref DrawQuadTree);


            if (DrawQuadTree)
            {
                CollisionSystem.Instance.ColliderQuadTree.DebugDrawQuadsIMGUI(Color.White);
                ImGui.Checkbox("Draw Leaves", ref DrawQuadTreeLeaves);

                if (DrawQuadTreeLeaves)
                {
                    CollisionSystem.Instance.ColliderQuadTree.DebugDrawLeavesIMGUI(Color.Cyan);
                }
            }

            ImGui.Text(BoxColliderA.Collide(CollidingEntA.X, CollidingEntA.Y, CircleColliderB) != null ? "Colliding!" : "Not Colliding.");

            // oops i broke this
            // ImGui.DragFloat("Box XPos", ref CollidingEntA.X);
            // ImGui.DragFloat("Box YPos", ref CollidingEntA.Y);

            if (ImGui.Button("Spawn 100 Colliders"))
            {
                for (int i = 0; i < 100; i++)
                {
                    var ent = new Entity(Rand.Float(0, Game.Width), Rand.Float(0, Game.Height));
                    ent.AddCollider(new BoxCollider(16, 16, 2));
                    Add(ent);
                }
            }

            ImGui.End();

            BoxColliderA.DebugRenderIMGUI(Color.White);
            CircleColliderB.DebugRenderIMGUI(Color.Cyan);
        }
    }
}
