using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using RiptideNetworking;

namespace ShardStudios {

    public class Health : MonoBehaviour
    {

        public float maxHealth = 100;
        public float health = 100;
        public bool canRegen = false;
        public float regenDelay = 5f;
        public float regenAmountPerSecond = 10;

        [SerializeField] NetworkedEntity owner;

        // Events;
        public UnityEvent<Player> OnHealthZero = new UnityEvent<Player>();
        public UnityEvent<Player, float> OnTakeDamage = new UnityEvent<Player, float>();
        public UnityEvent OnRegenStart = new UnityEvent();

        float regenStartTime = 0f;
        bool isRegenerating = false;

        public void Modify(Player attacker, float amount){

            health = Mathf.Clamp(health + amount, 0f, maxHealth);
            if( amount < 0 ){
                OnTakeDamage.Invoke(attacker, amount);
                regenStartTime = regenDelay;
                isRegenerating = false;
            }

            #if SERVER

                Message healthMessage = Message.Create(MessageSendMode.reliable, MessageID.EntityTakeDamage);
                healthMessage.AddUInt(owner.id);
                healthMessage.AddUShort(attacker.id);
                healthMessage.AddFloat(amount);

                NetworkManager.GameServer.Server.SendToAll(healthMessage);

            #endif

            if( health <= 0 )
                OnHealthZero.Invoke(attacker);

        }

        void Update(){

            regenStartTime -= Time.deltaTime;

            if( health < maxHealth && regenStartTime <= 0f && !isRegenerating && canRegen ){
                isRegenerating = true;
                OnRegenStart.Invoke();
            }

            if( isRegenerating ){
                health += regenAmountPerSecond * Time.deltaTime;
                if( health >= maxHealth ){
                    isRegenerating = false;
                }
            }

        }

        void OnDestroy(){
            OnHealthZero.RemoveAllListeners();
        }

        [MessageHandler((ushort)MessageID.EntityTakeDamage)]
        public static void EntityTakeDamage(Message message){

            NetworkedEntity entity = NetworkedEntity.GetEntityById(message.GetUInt());
            Player attacker = Player.GetById(message.GetUShort());
            float amount = message.GetFloat();

            if( entity != null ){

                Health entityHealth = entity.GetComponent<Health>();
                if( entityHealth != null ){
                    entityHealth.Modify(attacker, amount);
                }

            }
            
        }



    }

}
