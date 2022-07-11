using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShardStudios{

    public class HitInfo{

        public bool hitPlayer = false;
        public Player player;
        public GameObject hitObject;

        public HitInfo(RaycastHit hit){
            GameObject hitObject = hit.transform.gameObject;
            if( hitObject != null ){
                NetworkedEntity networkedEntity = hitObject.GetComponent<NetworkedEntity>();
                if( networkedEntity != null && networkedEntity.isNetworkedPlayer ){
                    hitPlayer = true;
                    player = networkedEntity.GetOwner();
                }
            }
        }
    }

}