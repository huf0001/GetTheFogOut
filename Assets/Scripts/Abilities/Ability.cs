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
    public float targetRadius = 10f;

    public string CollectibleName { get => collectibleName; }

    public AbilityEnum AbilityType
    {
        get { return abilityType; }
    }

    public abstract void Initialize(GameObject obj);
    public abstract void TriggerAbility(TileData tile);
}
