using Lutra.Collision;
using Lutra.Graphics;

namespace Lutra.Examples.Microgames.Snake.Entities
{
    public class SnakeSegment : Entity
    {
        public enum SegmentType
        {
            HEAD,
            BODY_FIRST,
            BODY,
            BODY_LAST
        }

        public enum SegmentDirection
        {
            Left,
            Right,
            Up,
            Down
        }

        public SegmentDirection Direction;

        public SnakeSegment(SegmentType segmentType)
        {
            string segmentImagePath = "";
            switch (segmentType)
            {
                case SegmentType.HEAD:
                    {
                        segmentImagePath = "Snake/SnakeHead.png";
                        break;
                    }
                case SegmentType.BODY:
                    {
                        segmentImagePath = "Snake/SnakeBodySegment.png";
                        break;
                    }
                case SegmentType.BODY_FIRST:
                    {
                        segmentImagePath = "Snake/SnakeBodySegmentFront.png";
                        break;
                    }
                case SegmentType.BODY_LAST:
                    {
                        segmentImagePath = "Snake/SnakeBodySegmentTail.png";
                        break;
                    }
            }

            AddGraphic(new Image(segmentImagePath));
            Graphic.CenterOrigin();

            AddCollider(new BoxCollider(SnakeScene.GridSize, SnakeScene.GridSize, 3));
            Collider.CenterOrigin();
        }

        public void SetDirection(SegmentDirection direction)
        {
            Direction = direction;

            switch (Direction) // Sprite faces left, so rotation has to be enwiggled.
            {
                case SegmentDirection.Left:
                    {
                        Graphic.Angle = 0;
                        break;
                    }
                case SegmentDirection.Right:
                    {
                        Graphic.Angle = 180;
                        break;
                    }
                case SegmentDirection.Up:
                    {
                        Graphic.Angle = 270;
                        break;
                    }
                case SegmentDirection.Down:
                    {
                        Graphic.Angle = 90;
                        break;
                    }
            }
        }
    }
}
