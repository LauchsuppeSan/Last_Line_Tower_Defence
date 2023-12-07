using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float countdown;

    private WaveSpawner waveSpawner;
    // Start is called before the first frame update
    void Start()
    {
        waveSpawner = GetComponentInParent<WaveSpawner>();
    }

    // Update is called once per frame
    void Update()
    {
      transform.Translate(transform.forward * speed * Time.deltaTime);
        
        countdown -= Time.deltaTime;

        if(countdown <= 0)
        {
            Destroy(gameObject);

            waveSpawner.Waves[waveSpawner.CurrentWaveIndex].EnemiesLeft--;
        }
    }
}
