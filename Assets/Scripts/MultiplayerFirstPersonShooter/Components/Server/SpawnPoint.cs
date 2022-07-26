using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class SpawnPoint : MonoBehaviour
    {

        public int teamIndex = 0;

        #if SERVER

            void Start()
            {
                SpawnController.AddSpawnPoint(this); 
            }

        #endif

    }

}
