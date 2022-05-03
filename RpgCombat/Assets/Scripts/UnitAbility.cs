using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Targeting
{
    Self,
    Ally,
    AllAllies,
    Enemy,
    AllEnemies,
    AllExceptCaster,
    AllUnits
};
public enum OTTargeting
{
    Caster,
    Target,
    AllAllies,
    AllEnemies,
    AllUnits
};

public enum StatEffect
{
    Health,
    Speed,
    Damage,
    Defense,
    Mana,
    CurrentTurnTracker
};

public enum StatBased
{
    Health,
    Speed,
    Damage,
    Defense
}

public class UnitAbility : MonoBehaviour
{
    [Tooltip("Name of the ability")]
    public string AbilityName;
    [Tooltip("What the ability targets")]
    public Targeting AbilityTarget;
    [Tooltip("Mana cost of this ability")]
    public float ManaCost;

    [Header("Initial Hit")]
    [Tooltip("What stat to affect\n" +
        "Health - Unit Health\nSpeed - Rate of unit's turns\nDamage - How strong each attack is\nDefense - How much each attack taken is reduced by\nCurrentTurnTracker - How close unit is to next turn")]
    public StatEffect StatAffected;
    [Tooltip("If yes, ability will use a flat non-scaling value.\nIf no, ability strength will be based on one of the caster's stats with a multiplier")]
    public bool FlatValue;
    [Tooltip("Flat Value to determine strong to affect the selected stat\nSafe to ignore if not using Flat Value")]
    public float FlatStrength;
    [Tooltip("What stat the ability strength should be based on\nSafe to ignore if using Flat Value\n" +
        "Health - Unit Health\nSpeed - Rate of unit's turns\nDamage - How strong each attack is\nDefense - How much each attack taken is reduced by")]
    public StatBased StatBasedOn;
    [Tooltip("The multiplier applied to the stat\nMake this value negative to heal units instead of damage them\nSafe to ignore if using Flat Value")]
    public float StatBasedMultiplier;

    [Header("Buffs, Debuffs, and Over Time Effects")]
    [Tooltip("Does the ability apply an Over Time effect")]
    public bool OverTimeEffect;
    [Tooltip("Target of the Over Time effect")]
    public OTTargeting OTTarget;
    [Tooltip("What stat does the Over Time effect apply to")]
    public StatEffect OTStatEffect;
    [Tooltip("How strong the Over Time effect will be\nNegative values will increase the stat")]
    public float OTStrength;
    [Tooltip("Multiplier applied to strength after each 'tick' of the effect\nLeave it at 1.0 to not scale")]
    public float OTStrengthScaling = 1.0f;
    [Tooltip("Duration of Over Time effect")]
    public int OTDuration;
    [Tooltip("Is this a 1 time stat change")]
    public bool applyOnce;
    [Tooltip("Should the effect reverse itself when the duration ends")]
    public bool reverseAtEnd;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
