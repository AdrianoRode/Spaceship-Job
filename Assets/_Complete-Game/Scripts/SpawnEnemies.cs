using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;
using Unity.Mathematics;

public class SpawnEnemies : MonoBehaviour
{
    public bool usingJobs;
    public GameObject enemyPrefab;

    public List<Transform> enemiesTransform;
    public Transform player;
    public Transform enemy;

    private static readonly ProfilerMarker ProfilerWithJobs =
        new ProfilerMarker(ProfilerCategory.Scripts, "JobsTester.WithJobs");

    private static readonly ProfilerMarker ProfilerNoJobs =
        new ProfilerMarker(ProfilerCategory.Scripts, "JobsTester.WithoutJobs");

    private TransformAccessArray _transformAccessArray;
    public struct JobSystem: IJobParallelForTransform
    {
        public Transform playerLocal;
        public List<Transform> nativeEnemiesTransform;

        public void Execute(int index, TransformAccess transform)
        {
            var enemyPos = nativeEnemiesTransform[index];
            var playerPos = playerLocal.transform.position;
            enemyPos.position = Vector3.Lerp(enemyPos.position, playerPos, Time.deltaTime);
        }
    }
    void Start()
    {
        var randomPos = Random.Range(-6, 6);
        
        for (int i = 0; i < 300; i++)
        {
            var enemyInstantiated = Instantiate(enemyPrefab, new Vector3(randomPos, 0, 20f), Quaternion.identity);
            enemiesTransform.Add(enemyInstantiated.transform);
        }
        
        _transformAccessArray = new TransformAccessArray(enemiesTransform.ToArray(), 4);
    }

    void Update()
    {
        if (usingJobs)
        {
            using (ProfilerWithJobs.Auto())
            {
                var jobSystem = new JobSystem()
                {
                    playerLocal = player,
                    nativeEnemiesTransform = enemiesTransform
                };
                var jobHandle = jobSystem.Schedule(_transformAccessArray);
                jobHandle.Complete();
                _transformAccessArray.Dispose();
            }
            
        }
        else
        {
            using (ProfilerNoJobs.Auto())
            {
                var enemyPos = enemy.transform.position;
                var playerPos = player.transform.position;
            
                float t = 0f;
                t = Mathf.Clamp01(t);
                transform.position = Vector3.Lerp(enemyPos, playerPos, t);
            }
        }
    }
}


