using System.Collections;
using System.Collections.Generic;
using Abilities;
using UnityEngine;

public abstract class Ability : ScriptableObject, ICollectible
{
    [SerializeField] protected string collectibleName = "New Ability";
    [SerializeField] protected AbilityEnum abilityType;
    public AudioClip sound;
    public Sprite sprite;
    public float baseCoolDown = 1f;
    public int targetRadius = 10;
    public int powerCost;
    public int duration;

    public string CollectableName { get => collectibleName; }

    public AbilityEnum AbilityType
    {
        get { return abilityType; }
    }

    public abstract void TriggerAbility(TileData tile);
}
