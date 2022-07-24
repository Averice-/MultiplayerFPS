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
            
            RaycastHit rayHit;
            #if SERVER
                 LagCompensation.BeginCompensation(NetworkManager.tick, (uint)NetworkManager.frameDifference);
                    
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
                        DecalManager.Decal("BulletDecal", rayHit.point, rayHit.normal, new Vector2(Random.Range(0, 3), 3));
                    #endif

                }
            }
             #if SERVER
                 LagCompensation.EndCompensation();
             #endif

        }
    }

}
