using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class Bullet
    {  
        Vector3 startPos;
        Vector3 direction;

        HitInfo hitInfo;

        public Bullet(Vector3 pos, Vector3 direction, float distance = Mathf.Infinity, float penetrationForce = 0f){

            RaycastHit rayHit;
            #if !SERVER
                Debug.DrawRay(pos, direction * 10, Color.green, 1, false);
            #endif
            if( Physics.Raycast(pos, direction, out rayHit, distance) ){

                hitInfo = new HitInfo(rayHit);
                if( hitInfo.hitPlayer ){
                    Debug.Log($"Bullet hit Player[{hitInfo.player.id}]");
                }
            }

        }
    }

}
