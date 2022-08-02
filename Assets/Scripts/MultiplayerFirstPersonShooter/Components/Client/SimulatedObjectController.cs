
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

namespace ShardStudios {

    public class SimulationState {
        public Vector3 position;
        public Vector3 velocity;
        public Vector2 rotation;
        //public Quaternion rotation;
        public uint tick;
    }

    public class InputState {
        public Vector2 input;
        //public Quaternion rotation; // rotation will not be server authoritative.
        public Vector2 rotation;
        public byte jumping;
        public byte primaryAttack;
        public byte secondaryAttack;
        public uint tick;
    }


    [RequireComponent(typeof(NetworkedPlayer))]
    public class SimulatedObjectController : MonoBehaviour
    {
    

        #if !SERVER

            InputController inputController;
            MovementController movementController;
            CharacterController characterController;
            NetworkedPlayer networkedEntity;
            Player owner;

            private const int CACHE_SIZE = 1024;
            private SimulationState[] simulationStates = new SimulationState[CACHE_SIZE];

            private static SimulationState activeServerState;

            private InputState[] inputStates = new InputState[CACHE_SIZE];
            private InputState activeInputState;

            private uint lastReconciledFrame = 0;

            private Vector2 moveAxis;
            private Vector2 lookAxis;
            private bool isJumping = false;
            
            // private static uint id;
            // interpolating settings.
            private static float interpolationMultiplier = 0.5f;
            //private static float rotationMultiplier = 0.3f;
            private static float snapThreshold = 0.3f;

            private static float toleranceMagnitude = snapThreshold * snapThreshold;

            // public GameObject ServerRep;
            // public GameObject ClientRep;
            void Start(){
                inputController = InputController.Instance;
                movementController = GetComponent<MovementController>();
                networkedEntity = GetComponent<NetworkedPlayer>();
                owner = networkedEntity.GetOwner();
                //id = networkedEntity.id;
                // if( owner?.isLocalPlayer == true ){
                //     ServerRep = (GameObject)Instantiate(Resources.Load("ServerRepresentation"), transform.position, transform.rotation);
                //     ClientRep = (GameObject)Instantiate(Resources.Load("ClientRepresentation"), transform.position, transform.rotation);
                // }
            }


            void Update(){
                
                if( owner?.isLocalPlayer == false )
                    return;

                moveAxis = inputController.GetMoveAxis();
                lookAxis = inputController.GetLookAxis();

                //movementController.SetLookAngle(lookAxis);

                activeInputState = new InputState {
                    input = moveAxis,
                    rotation = movementController.GetLookAngle(),//transform.rotation,
                    jumping = (byte)0,
                    primaryAttack = inputController.GetMouseButtonStatus() ? (byte)1 : (byte)0,
                    secondaryAttack = inputController.GetMouseButtonStatus(true) ? (byte)1 : (byte)0
                };

                InputWeaponSlot(inputController.GetSlot());

            }


            [MessageHandler((ushort)MessageID.ReceiveSimulationState)]
            private static void ReceiveSimulationState(Message message){

                uint entId = message.GetUInt();
                if( NetworkedEntity.Entities.ContainsKey(entId) ){

                    NetworkedPlayer simulatedEntity = (NetworkedPlayer)NetworkedEntity.Entities[entId];
                    if( simulatedEntity != null ){
                        

                        SimulationState stateReceived = new SimulationState {
                            position = message.GetVector3(),
                            velocity = message.GetVector3(),
                            rotation = message.GetVector2(),//message.GetQuaternion(),
                            tick = message.GetUInt()
                        };

                        UpdateSimulatedPlayer(simulatedEntity, stateReceived);


                    }

                }

            }

            private void InputWeaponSlot(int slot){
                if( owner.equipment != null && slot != owner.equipment.selectedEquipment && slot != -1 ){
                    owner.equipment.ChangeSelectedItem((EquipmentSlot)slot);

                    Message message = Message.Create(MessageSendMode.reliable, MessageID.PlayerChangeWeapon);
                    message.AddInt(slot);

                    NetworkManager.GameClient.Client.Send(message);
                }
            }

            [MessageHandler((ushort)MessageID.PlayerUpdateSelectedWeapon)]
            private static void PlayerUpdateSelectedWeapon(Message message){
                Player player = Player.GetById(message.GetUShort());
                EquipmentSlot slot = (EquipmentSlot)message.GetInt();

                player.equipment.ChangeSelectedItem(slot);
            }

            [MessageHandler((ushort)MessageID.ReceiveOwnSimulationState)]
            private static void ReceiveOwnSimulationState(Message message){

                uint entId = message.GetUInt();
                NetworkedPlayer simulatedEntity = (NetworkedPlayer)NetworkedEntity.Entities[entId];

                if( simulatedEntity != null ){

                    SimulationState stateReceived = new SimulationState {
                        position = message.GetVector3(),
                        velocity = message.GetVector3(),
                        rotation = message.GetVector2(),//message.GetQuaternion(),
                        tick = message.GetUInt()
                    };

                    if( activeServerState == null || activeServerState.tick < stateReceived.tick ){
                        activeServerState = stateReceived;
                        // temp
                        // SimulatedObjectController soc = simulatedEntity.GetComponent<SimulatedObjectController>();
                        // soc.ServerRep.transform.position = stateReceived.position;
                        // SimulationState cacheSt = soc.simulationStates[stateReceived.tick % CACHE_SIZE];
                        // if( cacheSt != null ){
                        //     soc.ClientRep.transform.position = cacheSt.position;
                        // }
                    }
                }
            }

            private static void UpdateSimulatedPlayer(NetworkedPlayer entity, SimulationState state){

                if( entity.lastSimulatedTick < state.tick ){

                    if( (state.position - entity.transform.position).magnitude > snapThreshold ){
                        entity.transform.position = Vector3.Lerp(entity.transform.position, state.position, interpolationMultiplier);
                    }
                    //entity.transform.rotation = Quaternion.Lerp(entity.transform.rotation, state.rotation, rotationMultiplier);
                    entity.lastSimulatedTick = state.tick;

                    if( entity.movementController != null ){
                        entity.movementController.velocity = state.velocity;
                        entity.movementController.SetLookAngleFromVector2(state.rotation);//Vector2.Lerp(entity.movementController.GetLookAngle(), state.rotation, rotationMultiplier), true);
                    }

                    Physics.SyncTransforms();

                }

            }

            public void SendInput(){
                
                Message message = Message.Create(MessageSendMode.unreliable, MessageID.ClientSendInput);
                message.AddUInt(networkedEntity.id);
                message.AddVector2(activeInputState.input);
                message.AddVector2(activeInputState.rotation); // Quaternion
                message.AddByte(activeInputState.jumping);
                message.AddByte(activeInputState.primaryAttack);
                message.AddByte(activeInputState.secondaryAttack);
                message.AddUInt(activeInputState.tick);

                NetworkManager.GameClient.Client.Send(message);

            }


            void FixedUpdate(){

                if( activeInputState == null || owner?.isLocalPlayer == false )
                    return;
;
                activeInputState.tick = NetworkManager.tick;
                isJumping = inputController.GetJumpingStatus();   

                if( isJumping ){
                    movementController.Jump();
                    activeInputState.jumping = (byte)1;
                }

                movementController.SetLookAngle(lookAxis);
                movementController.AddForce(new Vector3(moveAxis.x, 0f, moveAxis.y).normalized);
                networkedEntity.GetOwner().PrimaryAttack(activeInputState.primaryAttack == (byte)1);

                uint index = activeInputState.tick % CACHE_SIZE;

                simulationStates[index] = GetSimulationState(activeInputState);
                inputStates[index] = activeInputState;

                if( NetworkManager.GameClient.Client != null && NetworkManager.GameClient.IsConnected() ){
                    SendInput();
                    Reconcile();
                }

            }

            public SimulationState GetSimulationState(InputState inputState){
                return new SimulationState {
                    position = transform.position,
                    velocity = movementController.velocity,
                    rotation = movementController.GetLookAngle(), //transform.rotation,
                    tick = inputState.tick
                };
            }

            void Reconcile(){

                if( activeServerState == null || activeServerState.tick <= lastReconciledFrame )
                    return;

                uint cacheIndex = activeServerState.tick % CACHE_SIZE;
                InputState cachedInputState = inputStates[cacheIndex];
                SimulationState cachedSimulationState = simulationStates[cacheIndex];


                if( cachedInputState == null || cachedSimulationState == null ){

                    transform.position = activeServerState.position;
                    //transform.rotation = activeServerState.rotation;
                    movementController.SetLookAngleFromVector2(activeServerState.rotation);
                    movementController.velocity = activeServerState.velocity;

                    lastReconciledFrame = activeServerState.tick;

                    Physics.SyncTransforms();

                    return;

                }

                float positionDifference = (activeServerState.position - cachedSimulationState.position).magnitude;
                if(  positionDifference > snapThreshold ){
                    
                    //Quaternion currentRotation = transform.rotation;              // We'll replace this after our calculations. Should stop shitty reconciliation jitter from rotation without
                     Vector2 currentRotation = movementController.GetLookAngle();   // causing our replay reconciliation to be off by bad rotation amounts.

                    Debug.Log($"Out of sync, interpolating...[{positionDifference}]");

                    transform.position = Vector3.Lerp(cachedSimulationState.position, activeServerState.position, interpolationMultiplier);
                    movementController.velocity = Vector3.Lerp(cachedSimulationState.velocity, activeServerState.velocity, interpolationMultiplier);
                    //transform.rotation = activeServerState.rotation;
                    movementController.SetLookAngleFromVector2(activeServerState.rotation);

                    Physics.SyncTransforms();

                    uint rewindTick = activeServerState.tick;

                    while( rewindTick < NetworkManager.tick ){

                        uint rewindCache = rewindTick % CACHE_SIZE;

                        InputState rewoundInputState = inputStates[rewindCache];
                        SimulationState rewoundSimulationState = simulationStates[rewindCache];

                        if( rewoundInputState == null || rewoundSimulationState == null ){
                            ++rewindTick;
                            continue;
                        }

                        movementController.AddForce(new Vector3( rewoundInputState.input.x, 0f, rewoundInputState.input.y).normalized);
                        if( rewoundInputState.jumping == (byte)1 )
                            movementController.Jump();

                        //transform.rotation = rewoundInputState.rotation;
                        movementController.SetLookAngleFromVector2(rewoundInputState.rotation);
                        movementController.Move();

                        //Physics.SyncTransforms();

                        SimulationState resyncedSimulationState = GetSimulationState(rewoundInputState);
                        resyncedSimulationState.tick = rewindTick;
                        simulationStates[rewindCache] = resyncedSimulationState;

                        ++rewindTick;
                    }

                    //transform.rotation = currentRotation;
                    movementController.SetLookAngleFromVector2(currentRotation);
                    Physics.SyncTransforms();
                }

                lastReconciledFrame = activeServerState.tick;
            }

        #endif

    }

}
