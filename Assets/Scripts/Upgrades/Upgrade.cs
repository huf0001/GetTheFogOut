using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "Upgrade")]
public class Upgrade : ScriptableObject
{
    public BuildingType buildingType;
    public int pathNum;
    public int upgradeNum;
    public float amount;
}
