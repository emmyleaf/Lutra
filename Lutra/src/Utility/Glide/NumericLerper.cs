namespace Lutra.Utility.Glide;

internal class NumericLerper : GlideLerper
{
    float from, to, range;

    public override void Initialize(object fromValue, object toValue, GlideLerper.Behavior behavior)
    {
        from = Convert.ToSingle(fromValue);
        to = Convert.ToSingle(toValue);
        range = to - from;

        if (behavior.HasFlag(GlideLerper.Behavior.Rotation))
        {
            float angle = from;
            if (behavior.HasFlag(GlideLerper.Behavior.RotationRadians))
                angle *= Util.DEG_TO_RAD;

            if (angle < 0)
                angle = 360 + angle;

            float r = angle + range;
            float d = r - angle;
            float a = (float)Math.Abs(d);

            if (a >= 180) range = (360 - a) * (d > 0 ? -1 : 1);
            else range = d;
        }
    }

    public override object Interpolate(float t, object current, GlideLerper.Behavior behavior)
    {
        var value = from + range * t;
        if (behavior.HasFlag(GlideLerper.Behavior.Rotation))
        {
            if (behavior.HasFlag(GlideLerper.Behavior.RotationRadians))
                value *= Util.DEG_TO_RAD;

            value %= 360.0f;

            if (value < 0)
                value += 360.0f;

            if (behavior.HasFlag(GlideLerper.Behavior.RotationRadians))
                value *= Util.RAD_TO_DEG;
        }

        if (behavior.HasFlag(GlideLerper.Behavior.Round)) value = (float)Math.Round(value);

        var type = current.GetType();
        return Convert.ChangeType(value, type);
    }
}
