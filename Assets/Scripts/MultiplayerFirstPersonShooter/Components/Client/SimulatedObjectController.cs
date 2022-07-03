using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;

namespace ShardStudios {

    public class SimulationState {
        public Vector3 position;
        public Vector3 velocity;
        public Quaternion rotation;
        public uint tick;
    }

    public class InputState {
        public Vector2 input;
        public Quaternion rotation; // rotation will not be server authoritative.
        public byte jumping;
        public uint tick;
    }


    [RequireComponent(typeof(NetworkedEntity))]
    public class SimulatedObjectController : MonoBehaviour
    {
    

        #if !SERVER

            InputController inputController;
            MovementController movementController;
            CharacterController characterController;
            NetworkedEntity networkedEntity;

            private const int CACHE_SIZE = 1024;
            private SimulationState[] simulationStates = new SimulationState[CACHE_SIZE];

            private static SimulationState activeServerState;

            private InputState[] inputStates = new InputState[CACHE_SIZE];
            private InputState activeInputState;
            private uint lastReconciledFrame = 0;

            private Vector2 moveAxis;
            private Vector2 lookAxis;
            private bool isJumping = false;
            
            private static uint id;
            // interpolating settings.
            private static float interpolationMultiplier = 0.55f;
            private static float rotationMultiplier = 0.88f;
            private static float snapThreshold = 0.5f;

            private static float toleranceMagnitude = snapThreshold * snapThreshold;


            void Start(){
                inputController = InputController.Instance;
                movementController = GetComponent<MovementController>();
                networkedEntity = GetComponent<NetworkedEntity>();

                id = networkedEntity.id;
            }


            void Update(){

                moveAxis = inputController.GetMoveAxis();
                lookAxis = inputController.GetLookAxis();

                movementController.SetLookAngle(lookAxis);

                activeInputState = new InputState {
                    input = moveAxis,
                    rotation = transform.rotation,
                    jumping = (byte)0
                };

            }


            [MessageHandler((ushort)MessageID.ReceiveSimulationState)]
            private static void ReceiveSimulationState(Message message){

                uint entId = message.GetUInt();
                NetworkedEntity simulatedEntity = NetworkedEntity.Entities[entId];
                if( simulatedEntity != null ){
                    
                    SimulationState stateReceived = new SimulationState {
                        position = message.GetVector3(),
                        velocity = message.GetVector3(),
                        rotation = message.GetQuaternion(),
                        tick = message.GetUInt()
                    };

                    if( entId == id ){
                        if( activeServerState == null || activeServerState.tick < stateReceived.tick ){
                            activeServerState = stateReceived;
                        }
                    }else{
                        UpdateSimulatedPlayer(simulatedEntity, stateReceived);
                    }

                }

            }

            private static void UpdateSimulatedPlayer(NetworkedEntity entity, SimulationState state){

                if( entity.lastSimulatedTick < state.tick ){

                    if( (state.position - entity.transform.position).sqrMagnitude > toleranceMagnitude ){
                        entity.transform.position = Vector3.Lerp(entity.transform.position, state.position, interpolationMultiplier);
                    }
                    entity.transform.rotation = Quaternion.Lerp(entity.transform.rotation, state.rotation, rotationMultiplier);
                    entity.movementController.velocity = state.velocity;
                    entity.lastSimulatedTick = state.tick;

                }

            }

            public void SendInput(){

                Message message = Message.Create(MessageSendMode.unreliable, MessageID.ClientSendInput);
                message.AddUInt(networkedEntity.id);
                message.AddVector2(activeInputState.input);
                message.AddQuaternion(activeInputState.rotation);
                message.AddByte(activeInputState.jumping);
                message.AddUInt(activeInputState.tick);

                NetworkManager.GameClient.Client.Send(message);
            }


            void FixedUpdate(){

                if( activeInputState == null )
                    return;
;
                activeInputState.tick = NetworkManager.tick;
                isJumping = inputController.GetJumpingStatus();   

                if( isJumping ){
                    movementController.Jump();
                    activeInputState.jumping = (byte)1;
                }

                movementController.AddForce(new Vector3(moveAxis.x, 0f, moveAxis.y).normalized);

                if( NetworkManager.GameClient.Client != null && NetworkManager.GameClient.IsConnected() ){
                    SendInput();
                    Reconcile();
                }

                uint index = activeInputState.tick % CACHE_SIZE;

                simulationStates[index] = GetSimulationState(activeInputState);
                inputStates[index] = activeInputState;


            }

            public SimulationState GetSimulationState(InputState inputState){
                return new SimulationState {
                    position = transform.position,
                    velocity = movementController.velocity,
                    rotation = transform.rotation,
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
                    transform.rotation = activeServerState.rotation;
                    movementController.velocity = activeServerState.velocity;

                    lastReconciledFrame = activeServerState.tick;

                    return;

                }

                if( (activeServerState.position - cachedSimulationState.position).sqrMagnitude > toleranceMagnitude ){

                    Debug.Log("Out of sync, interpolating..");

                    transform.position = Vector3.Lerp(cachedSimulationState.position, activeServerState.position, interpolationMultiplier);
                    movementController.velocity = activeServerState.velocity;
                    transform.rotation = Quaternion.Lerp(cachedSimulationState.rotation, activeServerState.rotation, rotationMultiplier);

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

                        movementController.Move();

                        SimulationState resyncedSimulationState = GetSimulationState(rewoundInputState);
                        resyncedSimulationState.tick = rewindTick;
                        simulationStates[rewindCache] = resyncedSimulationState;

                        ++rewindTick;
                    }
                }

                lastReconciledFrame = activeServerState.tick;
            }

        #endif

    }

}
