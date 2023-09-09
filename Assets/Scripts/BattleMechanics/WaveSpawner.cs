using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private float countdown;
    [SerializeField] GameObject SpawnPoint;

    public Wave[] Waves;
    public int CurrentWaveIndex = 0;

    private bool nextWave = true;

    void Start()
    {
        for (int i = 0; i < Waves.Length; i++)
        {
            Waves[i].EnemiesLeft = Waves[i].enemies.Length;
        }
    }

    void Update()
    {
        if (nextWave)
        {
            countdown -= Time.deltaTime;
        }

        if (countdown <= 0)
        {
            nextWave = true;

            countdown = Waves[CurrentWaveIndex].TimeToNextWave;
            StartCoroutine(SpawnWave());
        }

        if (Waves[CurrentWaveIndex].EnemiesLeft == 0)
        {
            nextWave = true;

            CurrentWaveIndex++;
        }

        if(CurrentWaveIndex >= Waves.Length)
        {
            Debug.Log("U survived uwu");
        }
    }

    private IEnumerator SpawnWave()
    {
        if (CurrentWaveIndex < Waves.Length)
        {
            for (int i = 0; i < Waves[CurrentWaveIndex].enemies.Length; i++)
            {
                Enemy enemy = Instantiate(Waves[CurrentWaveIndex].enemies[i], SpawnPoint.transform.position, Quaternion.identity, SpawnPoint.transform);

                //enemy.transform.SetParent(this.transform);

                yield return new WaitForSeconds(Waves[CurrentWaveIndex].TimeToNextEnemy);
            }
        }
    }

    [System.Serializable]
    public class Wave
    {
        public Enemy[] enemies;
        public float TimeToNextEnemy;
        public float TimeToNextWave;

        [HideInInspector] public int EnemiesLeft;
    }
}
