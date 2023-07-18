using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Mine : MonoBehaviour, IActivityBlock
{
    [SerializeField]
    private GameObject explosionEffectPrefab;
    [SerializeField]
    private int ammo = 7;
    [SerializeField]
    private int rawDamagePerExplosion = 10;
    [SerializeField, Range(1,100)]
    private float triggerProbabilityByFullAmmo = 80f;
    [SerializeField, Tooltip("In Seconds")]
    private float constructionTime;

    private float triggerProbability;
    private float triggerProbabilityDregreaseValue;
    private bool _blockAllActivities;
    private Slider inConstructionbar;
    private GameObject constructionBarObj;

    private void Start()
    {
        // Trigger settings
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(TGSInfos.cellSize.x * 0.1f, 10, TGSInfos.cellSize.y * 0.1f);
        boxCollider.center = Vector3.zero;
        
        // Explosion and ammo settings
        triggerProbability = triggerProbabilityByFullAmmo;
        triggerProbabilityDregreaseValue = triggerProbability / (ammo + 1); // +1 because triggerPropability should never reache a value of 0
        
        // WIP
        // Eigentlich muss jeder turm seine eigene bar haben
        inConstructionbar = 
            Utils
            .GetComonentFromAllChilds<Slider>(this.transform, new List<Slider>())
            .First(x => x.gameObject.tag == "Constructionbar");

        inConstructionbar.transform.parent.GetComponent<Canvas>().worldCamera = Camera.main;
        inConstructionbar.maxValue = constructionTime;
        constructionBarObj = inConstructionbar.gameObject;
        constructionBarObj.SetActive(false);
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
        if(blockAllActivities) 
        { return; }

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

        Instantiate(explosionEffectPrefab, this.transform.position, Quaternion.Euler(-90f, 0f, 0f));
        
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

    public bool blockAllActivities
    {
        get => _blockAllActivities;
        set => _blockAllActivities = value;
    }

    public void StartConstructionTimer()
    {
        StartCoroutine(ConstructionTimer());
    }

    private IEnumerator ConstructionTimer()
    {
        float elapsedTime = 0;
        constructionBarObj.SetActive(true);

        while (elapsedTime < constructionTime) 
        { 
            yield return new WaitForEndOfFrame();
            elapsedTime += Time.deltaTime;
            inConstructionbar.value = elapsedTime;
        }

        blockAllActivities = false;
        constructionBarObj.SetActive(false);
    }
}
