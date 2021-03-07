using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class RigidbodyFirstPersonController : MonoBehaviour
    {
        [Serializable]
        public class MovementSettings
        {
            public float ForwardSpeed = 8.0f;   // Speed when walking forward
            public float BackwardSpeed = 4.0f;  // Speed when walking backwards
            public float StrafeSpeed = 4.0f;    // Speed when walking sideways
            public float RunMultiplier = 2.0f;  // Speed when sprinting
            public KeyCode RunKey = KeyCode.LeftShift;
            public float JumpForce = 30f;
            public AnimationCurve SlopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));
            [HideInInspector] public float CurrentTargetSpeed = 8f;

#if !MOBILE_INPUT
            private bool m_Running;
            public PlayerStaminaBar playerStaminaBar;
#endif

            private void Start()
            {
                playerStaminaBar.GetComponent<PlayerStaminaBar>();
            }

            public void UpdateDesiredTargetSpeed(Vector2 input)
            {
                if (input == Vector2.zero) return;

                // Strafe
                if (input.x > 0 || input.x < 0)
                {
                    CurrentTargetSpeed = StrafeSpeed;
                }

                // Backwards
                if (input.y < 0)
                {
                    CurrentTargetSpeed = BackwardSpeed;
                }

                // Forwards
                // This is handled last because, if strafing and moving forward at the same time, forward speed should take precedence
                if (input.y > 0)
                {

                    CurrentTargetSpeed = ForwardSpeed;
                }
#if !MOBILE_INPUT
                if (Input.GetKey(RunKey))
                {
                    if (playerStaminaBar.canSprint == true)
                    {
                        CurrentTargetSpeed *= RunMultiplier;
                        playerStaminaBar.canRegen = false;
                        m_Running = true;
                    }
                }
                else
                {
                    m_Running = false;
                }
#endif
            }

#if !MOBILE_INPUT
            public bool Running
            {
                get { return m_Running; }
            }
#endif
        }

        [Serializable]
        public class AdvancedSettings
        {
            public float groundCheckDistance = 0.01f;                       // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
            public float stickToGroundHelperDistance = 0.5f;                // stops the character
            public float slowDownRate = 20f;                                // rate at which the controller comes to a stop when there is no input
            public bool airControl;                                         // can the user control the direction that is being moved in the air
            [Tooltip("set it to 0.1 or more if you get stuck in wall")]
            public float shellOffset;                                       // reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
        }

        public PlayerStaminaBar playerStaminaBar;
        public Camera cam;
        public MovementSettings movementSettings = new MovementSettings();
        public MouseLook mouseLook = new MouseLook();
        public AdvancedSettings advancedSettings = new AdvancedSettings();

        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;
        private float m_YRotation;
        private Vector3 m_GroundContactNormal;
        private bool m_Jump, m_PreviouslyGrounded, m_Jumping, m_IsGrounded;
        [Space(10)]

        [Header("FMOD Events")]
        [EventRef] [SerializeField] private string eventJump;       // Played when player jumps
        [EventRef] [SerializeField] string eventFootstep;           // Played on a cooldown when player is moving
        EventInstance eventInstJump;

        // Player's current speed:
        // Gets the current speed the player is moving:
        float currentSpeed
        {
            get
            {
                return movementSettings.CurrentTargetSpeed;
            }
        }

        // Checks if cooldown is active and how long the cooldown should last
        float cooldownLength = 0.5f;
        bool cooldownActive = false;

        // Values to delay the sound by 
        [SerializeField] float strafeSoundDelay = .7f;
        [SerializeField] float walkSoundDelay = .5f;
        [SerializeField] float RunSoundDelay = .2f;

        // If footstepDebug is true/enabled, runs debug messages
        [SerializeField] bool footstepDebug;

        // movementSound() is triggered within the fixed update function
        // movementSound() controls when sounds are played
        private void movementSound()
        {
            // Checks if playing is on the ground
            if (Grounded)
            {
                // Checks the player's current speed: If within range, sets cooldown length as the respective delay.

                // Strafe
                if (currentSpeed >= movementSettings.StrafeSpeed - 0.5f && currentSpeed <= movementSettings.StrafeSpeed + 0.5f)
                {
                    cooldownLength = strafeSoundDelay;
                }

                else
                // Walk
                if (currentSpeed >= movementSettings.ForwardSpeed - 0.5f && currentSpeed <= movementSettings.ForwardSpeed + 0.5f)
                {
                    cooldownLength = walkSoundDelay;
                }

                else
                // Sprint
                if (currentSpeed >= (movementSettings.ForwardSpeed * movementSettings.RunMultiplier) - 0.5f && currentSpeed <= (movementSettings.ForwardSpeed * movementSettings.RunMultiplier) + 0.5f)
                {
                    cooldownLength = RunSoundDelay;
                }

                // If player is jumping, plays jump sound.
                if (Jumping)
                {
                    eventInstJump = RuntimeManager.CreateInstance(eventJump);          // Grabs our event instance and creates a new event instance based on the string we called
                    eventInstJump.start();                                             // Starts event instance, hence playing the sound.
                }

                // Checks if a delay is currently running/cooldown active. If not, plays footstep sound and resets delay.
                if (!cooldownActive)
                {
                    // Debug.Log("Footstep SFX Played");
                    RuntimeManager.PlayOneShot(eventFootstep);
                    cooldownActive = true;
                    StartCoroutine(Cooldown());
                }
            }
            // Variables that pre-exist in the script that we need to get access to:
            if (footstepDebug)
            {
                print(Grounded);
                print(Running);
                print(Jumping);
                print(GetInput());
                print(currentSpeed);
                print(Velocity);
            }
        }
        // Simple IEnumerator to delay the sound by cooldownLength seconds
        public IEnumerator Cooldown()
        {
            yield return new WaitForSeconds(cooldownLength);
            cooldownActive = false;
        }

        public Vector3 Velocity
        {
            get { return m_RigidBody.velocity; }
        }

        public bool Grounded
        {
            get { return m_IsGrounded; }
        }

        public bool Jumping
        {
            get { return m_Jumping; }
        }

        public bool Running
        {
            get
            {
#if !MOBILE_INPUT
                return movementSettings.Running;
#else
	            return false;
#endif
            }
        }

        private void Start()
        {
            m_RigidBody = GetComponent<Rigidbody>();
            m_Capsule = GetComponent<CapsuleCollider>();
            mouseLook.Init(transform, cam.transform);
        }

        private void Update()
        {
            RotateView()
;
            // Debug.Log(currentSpeed);

            if (Input.GetButtonDown("Jump") && !m_Jump)
            {
                if (playerStaminaBar.staminaBar.value > playerStaminaBar.jumpStamina)
                {
                    m_Jump = true;
                }
            }
        }

        private void FixedUpdate()
        {

            // FMOD: Checks that the rigidbody is moving and then calls the function
            if (GetComponent<Rigidbody>().velocity.magnitude > .1)
            {
                movementSound();
            }

            GroundCheck();
            Vector2 input = GetInput();

            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon) && (advancedSettings.airControl || m_IsGrounded))
            {
                // always move along the camera forward as it is the direction that it being aimed at
                Vector3 desiredMove = cam.transform.forward * input.y + cam.transform.right * input.x;
                desiredMove = Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized;

                desiredMove.x = desiredMove.x * movementSettings.CurrentTargetSpeed;
                desiredMove.z = desiredMove.z * movementSettings.CurrentTargetSpeed;
                desiredMove.y = desiredMove.y * movementSettings.CurrentTargetSpeed;
                if (m_RigidBody.velocity.sqrMagnitude <
                    (movementSettings.CurrentTargetSpeed * movementSettings.CurrentTargetSpeed))
                {
                    m_RigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
                }
            }

            if (m_IsGrounded)
            {
                m_RigidBody.drag = 5f;

                if (m_Jump)
                {
                    m_RigidBody.drag = 0f;
                    m_RigidBody.velocity = new Vector3(m_RigidBody.velocity.x, 0f, m_RigidBody.velocity.z);
                    m_RigidBody.AddForce(new Vector3(0f, movementSettings.JumpForce, 0f), ForceMode.Impulse);
                    m_Jumping = true;
                }

                if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
                {
                    m_RigidBody.Sleep();
                }
            }
            else
            {
                m_RigidBody.drag = 0f;
                if (m_PreviouslyGrounded && !m_Jumping)
                {
                    StickToGroundHelper();
                }
            }
            m_Jump = false;
        }

        private float SlopeMultiplier()
        {
            float angle = Vector3.Angle(m_GroundContactNormal, Vector3.up);
            return movementSettings.SlopeCurveModifier.Evaluate(angle);
        }

        private void StickToGroundHelper()
        {
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height / 2f) - m_Capsule.radius) +
                                   advancedSettings.stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
                {
                    m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
                }
            }
        }

        private Vector2 GetInput()
        {

            Vector2 input = new Vector2
            {
                x = Input.GetAxis("Horizontal"),
                y = Input.GetAxis("Vertical")
            };
            movementSettings.UpdateDesiredTargetSpeed(input);
            return input;
        }

        private void RotateView()
        {
            //avoids the mouse looking if the game is effectively paused
            if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

            // get the rotation before it's changed
            float oldYRotation = transform.eulerAngles.y;

            mouseLook.LookRotation(transform, cam.transform);

            if (m_IsGrounded || advancedSettings.airControl)
            {
                // Rotate the rigidbody velocity to match the new direction that the character is looking
                Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
                m_RigidBody.velocity = velRotation * m_RigidBody.velocity;
            }
        }

        /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
        private void GroundCheck()
        {
            m_PreviouslyGrounded = m_IsGrounded;
            RaycastHit hitInfo;
            if (Physics.SphereCast(transform.position, m_Capsule.radius * (1.0f - advancedSettings.shellOffset), Vector3.down, out hitInfo,
                                   ((m_Capsule.height / 2f) - m_Capsule.radius) + advancedSettings.groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                m_IsGrounded = true;
                m_GroundContactNormal = hitInfo.normal;
            }
            else
            {
                m_IsGrounded = false;
                m_GroundContactNormal = Vector3.up;
            }
            if (!m_PreviouslyGrounded && m_IsGrounded && m_Jumping)
            {
                m_Jumping = false;
            }
        }
    }
}