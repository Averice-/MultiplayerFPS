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

        public Vector3 velocity {
            get => _velocity;
            private set {
                _velocity = value;
            }
        }

        void Start(){
            characterController = GetComponent<CharacterController>();
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
                }else{
                    _velocity.y = gravity * Time.fixedDeltaTime;
                }
            }

            return _velocity * Time.fixedDeltaTime;
        }

        public void SetLookAngle(Vector2 lookAxis){
            lookAngle = transform.rotation.eulerAngles;

            lookAngle.x = lookAxis.y * mouseSensitivity;
            lookAngle.y = lookAxis.x * mouseSensitivity;

            transform.Rotate(new Vector3(0f, lookAngle.y, 0f));
        }

        public void Move(){
            Vector3 moveAxis = GetUsableVelocity(isJumpingThisFrame);

            characterController.Move(transform.TransformDirection(moveAxis));
            isJumpingThisFrame = false; // Reset the jump now that we've used it.
        }

        void FixedUpdate(){
            isGrounded = Physics.Raycast(transform.position, -transform.up, distanceToGround + 0.1f);
            Move();
        }
    }

}
