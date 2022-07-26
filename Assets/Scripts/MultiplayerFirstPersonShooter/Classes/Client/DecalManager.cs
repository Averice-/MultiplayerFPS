using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public static class DecalManager
    {

        static int poolSize = 60;
        static Decal[] decalPool = new Decal[poolSize];

        static int initializedDecals = 0;
        static int lastDecalUsed = 0;

        public static void Decal(string decalName, Vector3 position, Vector3 normal, Vector2 offset){

            if( initializedDecals < poolSize ){

                GameObject newDecal = (GameObject)NetworkManager.Instantiate(Resources.Load($"Effects/{decalName}"), position, Quaternion.identity);
                decalPool[lastDecalUsed] = newDecal.GetComponent<Decal>();
                initializedDecals++;

            }

            Decal usingDecal = decalPool[lastDecalUsed];

            usingDecal.transform.forward = normal;
            usingDecal.transform.position = position;// - normal * 0.001f; // A bit into the surface so the projector works. deprecated, using DecalProjector.

            float randomSpin = Random.Range(-180f, 180f);
            usingDecal.transform.Rotate(0, 0, randomSpin, Space.Self);
            usingDecal.SetOffset(offset);


            usingDecal.MakeAlive(); // Activate and start it's life counter.

            lastDecalUsed++;
            if( lastDecalUsed > poolSize - 1 )
                lastDecalUsed = 0;

        }

    }

}
