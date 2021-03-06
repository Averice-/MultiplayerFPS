using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


namespace ShardStudios {
    
    public class MovementController : MonoBehaviour
    {

        private Vector3 _velocity = Vector3.zero;

        private bool isGrounded = false;
        private bool isJumpingThisFrame = false;

        CharacterController characterController;
        Vector3 lookAngle;

        [Header("Movement Settings")]
        public float gravity = -9.81f;
        public float linearDamping = 1f;
        public float forceMultiplier = 0.5f;
        public float jumpHeight = 10f;
        public float distanceToGround = 1f;
        public float mouseSensitivity = 30f;

        [Space(10)]
        [Header("First Person Settings")]
        public bool isFpsCharacter = false;
        public Transform eyePosition;
        public AnimationHandler handler;

        public Vector3 velocity {
            get => _velocity;
            set {
                _velocity = value;
            }
        }

        void Start(){
            characterController = GetComponent<CharacterController>();
            handler = GetComponent<AnimationHandler>();
        }

        public void AddForce(Vector3 force){
            _velocity += force * forceMultiplier;
        }

        public void Jump(){
            isJumpingThisFrame = true;
        }

        // Get the velocity 100% ready to apply to character controller.
        public Vector3 GetUsableVelocity(bool jumping = false){
            _velocity = Vector3.Lerp(_velocity, new Vector3(0f, _velocity.y, 0f), Time.fixedDeltaTime * linearDamping); // Crude damping

            if( _velocity.y > gravity*2 )
                _velocity.y += gravity * Time.fixedDeltaTime;

            if( isGrounded ){
                if( jumping ){
                    _velocity.y = jumpHeight * forceMultiplier;
                }else if( _velocity.y <= 0f){
                    _velocity.y = -2f;
                }
            }
            handler.animator.SetBool("InAir", !isGrounded);

            return _velocity * Time.fixedDeltaTime;
        }

        // SETTINGS TO MAKE SO FAR;
        // VOLUME; slider
        // SENSITIVITY; slider
        // INVERT MOUSE Y; checkbox

        public void SetLookAngle(Vector2 lookAxis){
            lookAngle = transform.rotation.eulerAngles;

            lookAngle.x = lookAxis.y * mouseSensitivity;
            lookAngle.y = lookAxis.x * mouseSensitivity;

            if( isFpsCharacter ) {// Check Inverted setting.
                float rotationX = eyePosition.eulerAngles.x + -lookAngle.x;
                if( rotationX > 180 ){
                    rotationX -= 360;
                }
                rotationX = Mathf.Clamp(rotationX, -88f, 88f);
                eyePosition.rotation = Quaternion.Euler(rotationX, eyePosition.eulerAngles.y, eyePosition.eulerAngles.z);
            }

            transform.Rotate(new Vector3(0f, lookAngle.y, 0f));
        }

        public Vector2 GetLookAngle(){
            if( isFpsCharacter )
                return new Vector2(eyePosition.eulerAngles.x, transform.eulerAngles.y);

            return new Vector2(transform.eulerAngles.x, transform.eulerAngles.y);
        }

        public void SetLookAngleFromVector2(Vector2 angle){

            if( isFpsCharacter ){
                float rotationX = angle.x;
                if( rotationX > 180 ){
                    rotationX -= 360;
                }
                rotationX = Mathf.Clamp(rotationX, -88f, 88f);
                eyePosition.rotation = Quaternion.Euler(rotationX, eyePosition.eulerAngles.y, eyePosition.eulerAngles.z);
                transform.rotation = Quaternion.Euler(transform.eulerAngles.x, angle.y, transform.eulerAngles.z);
                return;
            }
            transform.rotation = Quaternion.Euler(angle.x, angle.y, transform.eulerAngles.z);
            
        }

        public void Move(){
            Vector3 moveAxis = GetUsableVelocity(isJumpingThisFrame);

            characterController.Move(transform.TransformDirection(moveAxis));
            isJumpingThisFrame = false; // Reset the jump now that we've used it.
        }

        void FixedUpdate(){
            //isGrounded = Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), distanceToGround + 0.15f);
            isGrounded = Physics.SphereCast(new Ray(transform.position, transform.TransformDirection(Vector3.down)), 0.7f, .5f);
            Move();
        }
    }

}
