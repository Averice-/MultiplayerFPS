using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

namespace ShardStudios {

    public class AnimationHandler : MonoBehaviour
    {

        public Animator animator;

        Vector2 lastSentFloats = Vector2.zero;

        string[] weaponAnimBools = new string[] {
            "EquipAssaultRifle",
            "EquipHandgun"
        };

        ushort id;
        
        public float movementBlendSpeed = 5f;

        void Start(){
            id = GetComponent<NetworkedPlayer>().ownerId;
        }

        public void EquipAnim(string animName){

            for( int i = 0; i < weaponAnimBools.Length; i++ ){
                animator.SetBool(weaponAnimBools[i], animName == weaponAnimBools[i]);
            }
            
        }

        #if SERVER

            public void SendMovementFloats(){

                Vector2 floats = new Vector2(animator.GetFloat("RunningRight"),animator.GetFloat("RunningForward"));
                if( floats != lastSentFloats ){
                    lastSentFloats = floats;
                    Message message = Message.Create(MessageSendMode.unreliable, MessageID.PlayerMoveAnims);
                    message.AddUShort(id);
                    message.AddVector2(floats);

                    NetworkManager.GameServer.Server.SendToAll(message, id);
                }
            }

            void FixedUpdate(){
                if( NetworkManager.tick % 4 == 0 ){
                    SendMovementFloats();
                }
            }

        #else

            [MessageHandler((ushort)MessageID.PlayerMoveAnims)]
            private static void ReceivePlayerMoveAnims(Message message){

                ushort id = message.GetUShort();
                Vector2 animFloats = message.GetVector2();

                Player ply = Player.GetById(id);
                if( ply != null ){
                    ply.handler.SetMovementFloats(animFloats, true); // Lerp already applied, force values without lerping.
                }

            }

        #endif

        public void SetMovementFloats(Vector2 floats, bool force = false){
            Vector2 values = new Vector2(animator.GetFloat("RunningRight"), animator.GetFloat("RunningForward"));

            if( force ){
                animator.SetFloat("RunningRight", floats.x);
                animator.SetFloat("RunningForward", floats.y);
                return;
            }
            Vector2 lerpedValue = Vector2.Lerp(values, floats, movementBlendSpeed * Time.fixedDeltaTime);
            animator.SetFloat("RunningForward", lerpedValue.y);
            animator.SetFloat("RunningRight", lerpedValue.x);
        }


    }

}
