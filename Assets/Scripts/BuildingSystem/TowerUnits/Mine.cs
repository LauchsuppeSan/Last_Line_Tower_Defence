using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField]
    private GameObject explosionEffectPrefab;
    [SerializeField]
    private int ammo = 7;
    [SerializeField]
    private int rawDamagePerExplosion = 10;
    [SerializeField, Range(1,100)]
    private float triggerProbabilityByFullAmmo = 80f;

    private float triggerProbability;
    private float triggerProbabilityDregreaseValue;

    private void Start()
    {
        // Trigger settings
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(TGSInfos.cellSize.x * 0.1f, 10, TGSInfos.cellSize.y * 0.1f);
        boxCollider.center = Vector3.zero;
        
        // Explosion and ammo settings
        triggerProbability = triggerProbabilityByFullAmmo;
        triggerProbabilityDregreaseValue = triggerProbability / (ammo + 1); // +1 because triggerPropability should never reache a value of 0
    }

    private void UpdateTriggerProbability()
    {
        triggerProbability -= triggerProbabilityDregreaseValue;
    }
    
    private void UpdateLeftoverAmmo()
    {
        ammo--;
    }

    private void OnTriggerEnter(Collider colliderInTrigger)
    {
        IDamageableByTowers damageDealer;
        
        // Check if its damageble and cancle if not
        if(colliderInTrigger.gameObject.TryGetComponent<IDamageableByTowers>(out damageDealer) == false)
        { return; }

        float triggerValue = Random.Range(0f, 100);

        // If trigger value is to less do nothing
        if (!triggerValue.AlmostEqualOrLess(triggerProbability))
        { return; }

        if(ammo != 0)
        {
            UpdateTriggerProbability();
            UpdateLeftoverAmmo();
        }

        Instantiate(explosionEffectPrefab, this.transform.position, Quaternion.identity);
        
        float damage = damageDealer.CalculateComittedDamage(Types.DamageType.Explosion, rawDamagePerExplosion);
        damageDealer.DegreaseHealth(damage);
        if(damageDealer.IsDead())
        {
            Destroy(colliderInTrigger.gameObject);
        }
        else
        {
            damageDealer.AddStatus(Types.Status.Slowed, 5f, false);
        }
    }
}
