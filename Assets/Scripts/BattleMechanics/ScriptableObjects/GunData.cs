using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="Gun", menuName ="Weapon/Gun")]
public class GunData : ScriptableObject
{
    public string Name;
    public float Damage;
    public float MaxDistance;
    public int CurrentAmmo;
    public int MagSize;
    public float FireRate;
    //public float ReloadTime;
    //public bool Reloading;
}
