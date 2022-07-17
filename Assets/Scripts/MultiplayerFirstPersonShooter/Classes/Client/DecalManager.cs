using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public static class DecalManager
    {

        static int poolSize = 30;
        static Decal[] decalPool = new Decal[poolSize];

        static int initializedDecals = 0;
        static int lastDecalUsed = 0;

        public static void Decal(Vector3 position, Vector3 normal, Vector2 offset){

            if( initializedDecals < poolSize ){

                GameObject newDecal = (GameObject)NetworkManager.Instantiate(Resources.Load("Effects/BulletDecal"), position, Quaternion.identity);
                decalPool[lastDecalUsed] = newDecal.GetComponent<Decal>();
                initializedDecals++;

            }

            Decal usingDecal = decalPool[lastDecalUsed];

            usingDecal.transform.up = normal;
            usingDecal.transform.position = position + normal * 0.001f; // A bit above the surface to stop z-fighting.
            usingDecal.SetOffset(offset);


            usingDecal.MakeAlive(); // Activate and start it's life counter.

            lastDecalUsed++;
            if( lastDecalUsed > poolSize - 1 )
                lastDecalUsed = 0;

        }

    }

}
