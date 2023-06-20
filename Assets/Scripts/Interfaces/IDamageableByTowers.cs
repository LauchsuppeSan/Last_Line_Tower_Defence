using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Types;

public interface IDamageableByTowers
{
    public float CalculateComittedDamage(DamageType damageType, float incomingRawDamage);
    public void DegreaseHealth(float reductionValue);
    public bool IsDead();
    public void AddStatus(Types.Status statusToAdd, float effectDuration, bool toEntireGroup);
}
