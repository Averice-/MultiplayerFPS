using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios {

    public class Bullet
    {  
        Vector3 startPos;
        Vector3 direction;

        HitInfo hitInfo;

        public Bullet(Player owner, Vector3 pos, Vector3 direction, float damage, float distance = 2500f, float penetrationForce = 0f){
            
            Debug.Log($"Bullet owner[{owner.id}]");
            RaycastHit rayHit;
            #if !SERVER
                Debug.DrawRay(pos, direction * 10, Color.green, 1, false);
            #endif
            if( Physics.Raycast(pos, direction, out rayHit, distance) ){

                hitInfo = new HitInfo(owner, rayHit);

                #if SERVER

                    if( hitInfo.hitObject != null ){

                        Health objectHealth = hitInfo.hitObject.GetComponent<Health>();
                        if( objectHealth != null ){
                            objectHealth.Modify(owner, -damage);
                        }

                    }

                #endif

                if( hitInfo.hitPlayer ){
                    Debug.Log($"Bullet hit Player[{hitInfo.player.id}]");
                }else{
                    #if !SERVER
                        DecalManager.Decal(rayHit.point, rayHit.normal, new Vector2(Random.Range(0, 3), 3));
                    #endif

                }
            }

        }
    }

}
