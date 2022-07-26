using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public enum SpawnType {
        Random,
        RoundRobin,
        Farest
    }

    public class SpawnController : MonoBehaviour
    {
        #if SERVER

            private static SpawnController _instance;
            public static SpawnController Instance {
                get => _instance;
                private set {
                    if( _instance == null )
                        _instance = value;
                    else if ( _instance != value ){
                        Debug.Log($"{nameof(SpawnController)} instance already exists, destroying duplicate!");
                        Destroy(value);
                    }
                }
            }

            void Awake(){
                Instance = this;
            }

            private List<SpawnPoint> spawns = new List<SpawnPoint>();
            private int lastUsedSpawn = 0;

            [Header("Spawn Controller Settings")]
            public bool isTeamBased = false;
            public SpawnType spawnType = SpawnType.Random;

            public static void AddSpawnPoint(SpawnPoint point){
                Instance.spawns.Add(point);
            }

            public bool IsSpawnBlocked(SpawnPoint point){

                Collider[] sphereHitColliders = Physics.OverlapSphere(point.transform.position+new Vector3(0f, 1f, 0f), 1f);
                bool blocked = false;
                for( int i = 0; i < sphereHitColliders.Length; i++ ){
                    if( sphereHitColliders[i].tag == "DynamicEntity" ){
                        blocked = true;
                        break;
                    }
                }
                return blocked;

            }

            public static SpawnPoint GetSpawn(){

                SpawnPoint chosenSpawn = Instance.spawns[0];

                switch( Instance.spawnType ){

                    case SpawnType.Random:
                        int nextSpawn = Random.Range(0, Instance.spawns.Count);
                        while( Instance.IsSpawnBlocked(Instance.spawns[nextSpawn]) ){
                            nextSpawn = Random.Range(0, Instance.spawns.Count);
                        }
                        chosenSpawn = Instance.spawns[nextSpawn];
                        break;

                    case SpawnType.RoundRobin:
                        if( Instance.lastUsedSpawn > Instance.spawns.Count ){
                            Instance.lastUsedSpawn = 0;
                        }
                        while( Instance.IsSpawnBlocked(Instance.spawns[Instance.lastUsedSpawn]) ){
                            Instance.lastUsedSpawn++;
                            if( Instance.lastUsedSpawn > Instance.spawns.Count ){
                                Instance.lastUsedSpawn = 0;
                            }
                        }
                        chosenSpawn = Instance.spawns[Instance.lastUsedSpawn];
                        Instance.lastUsedSpawn++;
                        break;

                    case SpawnType.Farest:
                        // Implement;
                        break;

                }

                return chosenSpawn;

            }

        #endif
    }


}
