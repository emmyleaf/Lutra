namespace Lutra.Examples.Microgames.LType;

public readonly record struct WeaponData
(
    float FiringSpeed,
    int Damage,
    int Range
)
{
    public static readonly WeaponData Plasma = new WeaponData(0.4f, 15, 20);
    public static readonly WeaponData DarkMatter = new WeaponData(0.3f, 30, 30);
    public static readonly WeaponData Missile = new WeaponData(0.5f, 50, 50);
    public static readonly WeaponData Gauss = new WeaponData(0.2f, 15, 10);
    public static readonly WeaponData Disruptor = new WeaponData(0.35f, 40, 40);
    public static readonly WeaponData DUCK = new WeaponData(0.5f, 1, 100);
    public static readonly WeaponData Mine = new WeaponData(1f, 100, 200);

    public static WeaponData Get(Weapon weapon) => weapon switch
    {
        Weapon.Plasma => Plasma,
        Weapon.DarkMatter => DarkMatter,
        Weapon.Missile => Missile,
        Weapon.Gauss => Gauss,
        Weapon.Disruptor => Disruptor,
        Weapon.DUCK => DUCK,
        Weapon.Mine => Mine,
        _ => new WeaponData() // This should never happen but y'know, C# enums...
    };
}
