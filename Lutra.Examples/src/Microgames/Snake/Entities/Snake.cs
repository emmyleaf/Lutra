using System.Collections.Generic;
using Lutra.Input;

namespace Lutra.Examples.Microgames.Snake.Entities
{
    public class Snake : Entity
    {
        public LinkedList<SnakeSegment> Segments = new();
        public int TileX, TileY;
        private float timeSinceLastStep;
        public bool Alive;

        private int DesiredDeltaX, DesiredDeltaY;

        public Snake(int tileX, int tileY)
        {
            TileX = tileX;
            TileY = tileY;
            DesiredDeltaX = -1;

            // Initial snake body setup
            var head = new SnakeSegment(SnakeSegment.SegmentType.HEAD);
            var bodyfirst = new SnakeSegment(SnakeSegment.SegmentType.BODY_FIRST);
            var bodylast = new SnakeSegment(SnakeSegment.SegmentType.BODY_LAST);
            Segments.AddFirst(head);
            Segments.AddAfter(Segments.First, bodyfirst);
            Segments.AddLast(bodylast);

            head.X = TileX * SnakeScene.GridSize;
            head.Y = TileY * SnakeScene.GridSize;
            head.Graphic.Layer = -10;
            bodyfirst.X = (TileX + 1) * SnakeScene.GridSize;
            bodyfirst.Y = TileY * SnakeScene.GridSize;
            bodylast.X = (TileX + 2) * SnakeScene.GridSize;
            bodylast.Y = TileY * SnakeScene.GridSize;

            Alive = true;
        }

        public override void Update()
        {
            base.Update();

            if (InputManager.KeyDown(Key.Left) && Segments.First.ValueRef.Direction != SnakeSegment.SegmentDirection.Right)
            {
                DesiredDeltaX = -1;
                DesiredDeltaY = 0;
            }

            if (InputManager.KeyDown(Key.Right) && Segments.First.ValueRef.Direction != SnakeSegment.SegmentDirection.Left)
            {
                DesiredDeltaX = 1;
                DesiredDeltaY = 0;
            }

            if (InputManager.KeyDown(Key.Up) && Segments.First.ValueRef.Direction != SnakeSegment.SegmentDirection.Down)
            {
                DesiredDeltaX = 0;
                DesiredDeltaY = -1;
            }

            if (InputManager.KeyDown(Key.Down) && Segments.First.ValueRef.Direction != SnakeSegment.SegmentDirection.Up)
            {
                DesiredDeltaX = 0;
                DesiredDeltaY = 1;
            }

            if ((timeSinceLastStep) > 0.25f && Alive)
            {
                StepForward(DesiredDeltaX, DesiredDeltaY);
                timeSinceLastStep -= 0.25f;
            }

            if (TouchingAWall() || TouchingOwnBody())
            {
                Alive = false;
            }

            timeSinceLastStep += Game.Instance.DeltaTime;
        }

        public override void Added()
        {
            base.Added();

            foreach (var segment in Segments)
            {
                Scene.Add(segment);
            }
        }

        private void StepForward(int deltaX, int deltaY)
        {
            var newDirection = SnakeSegment.SegmentDirection.Left;

            if (deltaX > 0)
            {
                newDirection = SnakeSegment.SegmentDirection.Right;
            }
            else if (deltaX < 0)
            {
                newDirection = SnakeSegment.SegmentDirection.Left;
            }
            else if (deltaY > 0)
            {
                newDirection = SnakeSegment.SegmentDirection.Down;
            }
            else if (deltaY < 0)
            {
                newDirection = SnakeSegment.SegmentDirection.Up;
            }

            // Starting from the last segment of the snake, move on up and move them into place
            var segment = Segments.Last;

            while (segment.Previous != null)
            {
                segment.ValueRef.SetDirection(segment.Previous.ValueRef.Direction);
                segment.ValueRef.X = segment.Previous.ValueRef.X;
                segment.ValueRef.Y = segment.Previous.ValueRef.Y;
                segment = segment.Previous;
            }

            // Update head
            TileX += deltaX;
            TileY += deltaY;

            Segments.First.ValueRef.SetDirection(newDirection);
            Segments.First.ValueRef.X = TileX * SnakeScene.GridSize;
            Segments.First.ValueRef.Y = TileY * SnakeScene.GridSize;
        }

        public void AddBodySegment()
        {
            var segment = new SnakeSegment(SnakeSegment.SegmentType.BODY);
            segment.SetDirection(Segments.Last.Previous.ValueRef.Direction);
            segment.X = Segments.Last.Previous.ValueRef.X;
            segment.Y = Segments.Last.Previous.ValueRef.Y;
            Segments.AddBefore(Segments.Last, segment);
            Scene.Add(segment);
        }

        public override void Removed()
        {
            base.Removed();

            foreach (var segment in Segments)
            {
                segment.RemoveSelf();
            }
        }

        public bool TouchingAFish() => Segments.First.ValueRef.Collider.Overlap(Segments.First.ValueRef.X, Segments.First.ValueRef.Y, new int[] { 2 });

        public bool TouchingAWall() => Segments.First.ValueRef.Collider.Overlap(Segments.First.ValueRef.X, Segments.First.ValueRef.Y, new int[] { 1 });

        public bool TouchingOwnBody() => Segments.First.ValueRef.Collider.Overlap(Segments.First.ValueRef.X, Segments.First.ValueRef.Y, new int[] { 3 });

    }
}
