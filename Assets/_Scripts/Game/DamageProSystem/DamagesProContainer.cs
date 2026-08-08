using DamageNumbersPro;
using UnityEngine;

public class DamagesProContainer : MonoBehaviour
{
    public enum DamageNumberPreset
    {
        Bleed,
        BloodSpread,
        BloodText,
        Clear,
        Comic,
        Decimals,
        DotSeparation,
        Experience,
        Flames,
        Gold,
        Mana,
        NeonText,
        Percentage,
        PixelHeal,
        PixelText,
        Poison,
        RedGlow,
        RedShadow,
        SawNeon,
        SpreadUp
    }

    [Header("3D")]
    public DamageNumber Bleed;
    public DamageNumber BloodSpread;
    public DamageNumber BloodText;
    public DamageNumber Clear;
    public DamageNumber Comic;
    public DamageNumber Decimals;
    public DamageNumber DotSeparation;
    public DamageNumber Experience;
    public DamageNumber Flames;
    public DamageNumber Gold;
    public DamageNumber Mana;
    public DamageNumber NeonText;
    public DamageNumber Percentage;
    public DamageNumber PixelHeal;
    public DamageNumber PixelText;
    public DamageNumber Poison;
    public DamageNumber RedGlow;
    public DamageNumber RedShadow;
    public DamageNumber SawNeon;
    public DamageNumber SpreadUp;

    public DamageNumber GetPrefab(DamageNumberPreset preset)
    {
        return preset switch
        {
            DamageNumberPreset.Bleed => Bleed,
            DamageNumberPreset.BloodSpread => BloodSpread,
            DamageNumberPreset.BloodText => BloodText,
            DamageNumberPreset.Clear => Clear,
            DamageNumberPreset.Comic => Comic,
            DamageNumberPreset.Decimals => Decimals,
            DamageNumberPreset.DotSeparation => DotSeparation,
            DamageNumberPreset.Experience => Experience,
            DamageNumberPreset.Flames => Flames,
            DamageNumberPreset.Gold => Gold,
            DamageNumberPreset.Mana => Mana,
            DamageNumberPreset.NeonText => NeonText,
            DamageNumberPreset.Percentage => Percentage,
            DamageNumberPreset.PixelHeal => PixelHeal,
            DamageNumberPreset.PixelText => PixelText,
            DamageNumberPreset.Poison => Poison,
            DamageNumberPreset.RedGlow => RedGlow,
            DamageNumberPreset.RedShadow => RedShadow,
            DamageNumberPreset.SawNeon => SawNeon,
            DamageNumberPreset.SpreadUp => SpreadUp,
            _ => null
        };
    }
}
