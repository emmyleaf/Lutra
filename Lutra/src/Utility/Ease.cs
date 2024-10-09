namespace Lutra.Utility;

public delegate float Easer(float t);

public static class Ease
{
    public static readonly Easer Linear = (t) => t;
    public static readonly Easer ToAndFro = (t) => t < 0.5f ? t * 2 : 1 - (t - 0.5f) * 2f;

    public static readonly Easer ElasticIn = (t) => MathF.Sin(13 * PI_OVER_2 * t) * MathF.Pow(2, 10 * (t - 1));
    public static readonly Easer ElasticOut = (t) => 1f + MathF.Sin(-13 * PI_OVER_2 * (t + 1)) * MathF.Pow(2, -10 * t);
    public static readonly Easer ElasticInOut = InOut(ElasticIn, ElasticOut);

    public static readonly Easer SineIn = (t) => 1f - MathF.Cos(PI_OVER_2 * t);
    public static readonly Easer SineOut = (t) => MathF.Sin(PI_OVER_2 * t);
    public static readonly Easer SineInOut = (t) => 0.5f - MathF.Cos(PI * t) / 2f;

    public static readonly Easer QuadIn = (t) => t * t;
    public static readonly Easer QuadOut = Invert(QuadIn);
    public static readonly Easer QuadInOut = InOut(QuadIn, QuadOut);

    public static readonly Easer CubeIn = (t) => t * t * t;
    public static readonly Easer CubeOut = Invert(CubeIn);
    public static readonly Easer CubeInOut = InOut(CubeIn, CubeOut);

    public static readonly Easer QuartIn = (t) => t * t * t * t;
    public static readonly Easer QuartOut = Invert(QuartIn);
    public static readonly Easer QuartInOut = InOut(QuartIn, QuartOut);

    public static readonly Easer QuintIn = (t) => t * t * t * t * t;
    public static readonly Easer QuintOut = Invert(QuintIn);
    public static readonly Easer QuintInOut = InOut(QuintIn, QuintOut);

    public static readonly Easer CircIn = (t) => 1f - MathF.Sqrt(1 - t * t);
    public static readonly Easer CircOut = (t) => MathF.Sqrt(1 - (t - 1) * (t - 1));
    public static readonly Easer CircInOut = InOut(CircIn, CircOut);

    public static readonly Easer ExpoIn = (t) => MathF.Pow(2, 10 * (t - 1));
    public static readonly Easer ExpoOut = Invert(ExpoIn);
    public static readonly Easer ExpoInOut = InOut(ExpoIn, ExpoOut);

    public static readonly Easer BackIn = (t) => t * t * (2.70158f * t - 1.70158f);
    public static readonly Easer BackOut = Invert(BackIn);
    public static readonly Easer BackInOut = InOut(BackIn, BackOut);

    public static readonly Easer BounceOut = (t) =>
        {
            if (t < BT1) return BM * t * t;
            if (t < BT2) return BM * (t - BO2) * (t - BO2) + BC2;
            if (t < BT3) return BM * (t - BO3) * (t - BO3) + BC3;
            return BM * (t - BO4) * (t - BO4) + BC4;
        };
    public static readonly Easer BounceIn = Invert(BounceOut);
    public static readonly Easer BounceInOut = InOut(BounceIn, BounceOut);

    public static Easer Invert(Easer easer)
    {
        return (t) => 1 - easer(1 - t);
    }

    public static Easer InOut(Easer easeIn, Easer easeOut)
    {
        return (t) => (t <= 0.5f) ? easeIn(t * 2) / 2 : 0.5f + easeOut(t * 2 - 1) / 2;
    }

    public static Easer Mirror(Easer easeIn, Easer easeOut)
    {
        return (t) => t < 0.5f ? easeIn(t * 2) : 1 - (easeOut((t - 0.5f) * 2f));
    }

    private const float PI = MathHelper.Pi;
    private const float PI_OVER_2 = MathHelper.PiOver2;
    private const float BT1 = 4f / 11f;
    private const float BT2 = 8f / 11f;
    private const float BO2 = 6f / 11f;
    private const float BC2 = 3f / 4f;
    private const float BT3 = 10f / 11f;
    private const float BO3 = 9f / 11f;
    private const float BC3 = 15f / 16f;
    private const float BO4 = 21f / 22f;
    private const float BC4 = 63f / 64f;
    private const float BM = 121f / 16f;
}
