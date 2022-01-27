namespace Lutra.Utility.Glide
{
    public abstract class GlideLerper
    {
        [Flags]
        public enum Behavior
        {
            None = 0,
            Reflect = 1,
            Rotation = 2,
            RotationRadians = 4,
            RotationDegrees = 8,
            Round = 16
        }

        public abstract void Initialize(Object fromValue, Object toValue, Behavior behavior);
        public abstract object Interpolate(float t, object currentValue, Behavior behavior);
    }
}
