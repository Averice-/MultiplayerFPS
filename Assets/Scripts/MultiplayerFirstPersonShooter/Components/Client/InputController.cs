using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ShardStudios {

    public class InputController : MonoBehaviour
    {   

        private static InputController _instance;
        public static InputController Instance {
            get => _instance;
            private set {
                if( _instance == null )
                    _instance = value;
                else if ( _instance != value ){
                    Debug.Log($"{nameof(InputController)} instance already exists, destroying duplicate!");
                    Destroy(value);
                }
            }
        }

        private DefaultActions defaultActions;
        private AnimationHandler handler;

        private bool isJumpingThisFrame = false;
        private bool isPrimaryAttack = false;
        private bool isSecondaryAttack = false;

        private int equipmentSlotButton = -1;

        void Awake(){
            Instance = this;
        }

        void Start(){
            defaultActions = new DefaultActions();
            defaultActions.PlayerMovement.Enable();

            defaultActions.PlayerMovement.Jump.performed += JumpInputPressed;

            defaultActions.PlayerMovement.PrimaryAttack.started += PrimaryAttackPressed;
            defaultActions.PlayerMovement.SecondaryAttack.started += SecondaryAttackPressed;

            defaultActions.PlayerMovement.PrimaryAttack.canceled += PrimaryAttackCanceled;
            defaultActions.PlayerMovement.SecondaryAttack.canceled += SecondaryAttackCanceled;

            defaultActions.PlayerMovement.PrimarySlot1.performed += SelectSlotOne;
            defaultActions.PlayerMovement.SecondarySlot2.performed += SelectSlotTwo;

        }

        public void JumpInputPressed(InputAction.CallbackContext context){
            isJumpingThisFrame = true;
        }

        public int GetSlot(){
            return equipmentSlotButton;
        }

        public void SetAnimationHandler(AnimationHandler animHandler){
            handler = animHandler;
        }

        public bool GetJumpingStatus(){
            if( isJumpingThisFrame ){
                isJumpingThisFrame = false;
                return true;
            }
            return false;
        }

        public Vector2 GetMoveAxis(){
            Vector2 axis = defaultActions.PlayerMovement.Move.ReadValue<Vector2>();
            if( handler != null )
                handler.SetMovementFloats(axis);
            // handler.animator.SetFloat("RunningForward", axis.y);
            // handler.animator.SetFloat("RunningRight", axis.x);
            return axis;
        }

        public Vector2 GetLookAxis(){
            return defaultActions.PlayerMovement.Look.ReadValue<Vector2>();
        }

        public void PrimaryAttackPressed(InputAction.CallbackContext context){
            isPrimaryAttack = true;
        }
        public void PrimaryAttackCanceled(InputAction.CallbackContext context){
            isPrimaryAttack = false;
        }
        public void SecondaryAttackPressed(InputAction.CallbackContext context){
            isSecondaryAttack = true;
        }
        public void SecondaryAttackCanceled(InputAction.CallbackContext context){
            isSecondaryAttack = false;
        }

        public bool GetMouseButtonStatus(bool rightMouse = false){
            if( rightMouse ){
                return isSecondaryAttack;
            }
            return isPrimaryAttack;
        }

        public void SelectSlotOne(InputAction.CallbackContext context){
            equipmentSlotButton = 0;
        }
        public void SelectSlotTwo(InputAction.CallbackContext context){
            equipmentSlotButton = 1;
        }

    }

}
