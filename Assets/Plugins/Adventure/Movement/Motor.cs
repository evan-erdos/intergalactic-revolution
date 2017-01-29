/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-01-17 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Movement {
    public class Motor : BaseObject, IMotor {
        Transform last;

        public float Volume => dash?0.2f:duck?0.05f:0.1f;
        public float Rate => dash?0.15f:duck?0.3f:0.2f;

        public void Move(Vector3 move, bool duck, bool jump) =>
            inputMove = new Vector3(move.x, 0f, move.z);

        public bool OnMove(Person actor, StoryArgs args) => false;

        void Awake() => cr = GetComponent<CharacterController>();

        public enum Transfer { None, Initial, PermaTransfer, PermaLocked }

        bool wait, newPlatform, recentlyLanded;
        uint massPlayer = 80;
        float
            maxSpeed = 57.2f,
            dampingGround = 30f,
            dampingAirborne = 20f,
            lastStartTime = 0,
            lastEndTime = -100f,
            tgtCrouch = 0,
            tgtCrouchLand = 1.5f;
        public float
            modSprint = 1.6f,
            modCrouch = 0.8f,
            speedAnterior = 16f,
            speedLateral = 12f,
            speedPosterior = 10f,
            speedVertical = 1f,
            deltaHeight = 2f,
            weightPerp = 0,
            weightSteep = 0.5f,
            extraHeight = 4.1f,
            slidingSpeed = 15f,
            lateralControl = 1f,
            speedControl = 0.4f,
            deltaCrouch = 1f,
            landingDuration = 0.15f,
            terminalVelocity = 30f;
        [SerializeField] Transfer transfer = Transfer.PermaTransfer;
        [SerializeField] AnimationCurve responseSlope =
            new AnimationCurve(
                new Keyframe(-90,1),
                new Keyframe(90,0));
        CollisionFlags hitFlags;
        Vector3 inputMove = Vector3.zero;
        Vector3 jumpDir = Vector3.zero;
        Vector3 platformVelocity = Vector3.zero;
        Vector3 groundNormal = Vector3.zero;
        Vector3 lastGroundNormal = Vector3.zero;
        Vector3 hitPoint = Vector3.zero;
        Vector3 lastHitPoint = new Vector3(Mathf.Infinity,0,0);
        Vector3 activeLocalPoint = Vector3.zero;
        Vector3 activeGlobalPoint = Vector3.zero;
        Transform hitPlatform, activePlatform, playerGraphics;
        Quaternion activeLocalRotation, activeGlobalRotation;
        Matrix4x4 lastMatrix;
        CharacterController cr;
        ControllerColliderHit lastColl;
        protected bool jump, dash, duck;

        public bool IsJumping {
            get { return isJumping; }
            protected set {
                if (isJumping!=value) {
                    if (value && !WasJumping) OnJump();
                    else OnLand();
                }
                WasJumping = isJumping;
                isJumping = value;
            }
        } bool isJumping = false;

        public bool IsDisabled {get;set;}
        public bool WasGrounded {get;set;}
        public bool IsGrounded {get;protected set;}
        public bool WasJumping {get;protected set;}
        public bool grounded => groundNormal.y>0.01;
        public bool IsSliding => IsGrounded && TooSteep();
        public bool IsSprinting => dash;
        public Vector3 LocalPosition => transform.localPosition;
        public Vector3 Velocity {
            get { return velocity; }
            protected set {
                velocity = value;
                lastVelocity = Vector3.zero;
                IsGrounded = false;
            }
        } Vector3 velocity = Vector3.zero;

        public Vector3 lastVelocity {get;protected set;}

        public void Kill() {
            IsDisabled = true;
            var rb = GetComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.freezeRotation = false;
            rb.AddForce(velocity,ForceMode.VelocityChange);
            if (GetComponent<Look>())
                GetComponent<Look>().enabled = false;
            cr.enabled = false;
            this.enabled = false;
        }


        public void Kill(Actor actor, StoryArgs args) => Kill();

        public void OnJump() { }

        public IEnumerator Landed() {
            if (!wait) {
                wait = true;
                recentlyLanded = true;
                yield return new WaitForSeconds(landingDuration);
                recentlyLanded = false;
                wait = false;
            }
        }

        public void OnLand() => StartCoroutine(Landed());

        void Update() {
            if (Mathf.Abs(Time.timeScale)<0.01f) return;
            if (modSprint==0 || modCrouch==0 || speedAnterior==0
            || speedLateral==0 || speedPosterior==0) return;
            var dirVector = new Vector3(
                x: Input.GetAxis("Horizontal"),
                y: 0,
                z: Input.GetAxis("Vertical"));
            if (dirVector != Vector3.zero) {
                var dirLength = dirVector.magnitude;
                dirVector /= dirLength;
                dirLength = Mathf.Min(1f,dirLength);
                dirLength = dirLength * dirLength;
                dirVector = dirVector * dirLength;
            } inputMove = transform.rotation * dirVector;
            if (!IsDisabled) UpdateFunction();
        }

        void FixedUpdate() {
            if ((Mathf.Abs(Time.timeScale)>0.1f) && activePlatform != null) {
                if (!newPlatform)
                    platformVelocity = (activePlatform.localToWorldMatrix.MultiplyPoint3x4(activeLocalPoint) - lastMatrix.MultiplyPoint3x4(activeLocalPoint))/((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
                lastMatrix = activePlatform.localToWorldMatrix;
                newPlatform = false;
            } else platformVelocity = Vector3.zero;
        }

        public void OnCollisionEvent(Collision collision) { }

        void UpdateFunction() {
            var tempVelocity = velocity;
            var moveDistance = Vector3.zero;
            tempVelocity = applyDeltaVelocity(tempVelocity);
            if (MoveWithPlatform()) {
                var newGlobalPoint = activePlatform.TransformPoint(activeLocalPoint);
                moveDistance = (newGlobalPoint - activeGlobalPoint);
                if (moveDistance != Vector3.zero) cr.Move(moveDistance);
                var newGlobalRotation = activePlatform.rotation*activeLocalRotation;
                var rotationDiff = newGlobalRotation * Quaternion.Inverse(activeGlobalRotation);
                var yRotation = rotationDiff.eulerAngles.y;
                if (yRotation!=0) transform.Rotate(0,yRotation,0);
            }
            var lastPosition = transform.position;
            var currentMovementOffset = tempVelocity * ((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
            var pushDownOffset = Mathf.Max(
                cr.stepOffset,
                new Vector3(
                    x: currentMovementOffset.x,
                    y: 0,
                    z: currentMovementOffset.z).magnitude);
            if (IsGrounded) currentMovementOffset -= pushDownOffset*Vector3.up;
            hitPlatform = null;
            groundNormal = Vector3.zero;
            // This one moves the user and returns the direction of the hit
            hitFlags = cr.Move(currentMovementOffset);
            lastHitPoint = hitPoint;
            lastGroundNormal = groundNormal;
            if (activePlatform != hitPlatform && hitPlatform != null) {
                activePlatform = hitPlatform;
                lastMatrix = hitPlatform.localToWorldMatrix;
                newPlatform = true;
            }
            var oldHVelocity = new Vector3(tempVelocity.x,0,tempVelocity.z);
            velocity = (transform.position-lastPosition)
                / ((Mathf.Abs(Time.deltaTime)>0.01f)
                    ? Time.deltaTime
                    : 0.01f);
            var newHVelocity = new Vector3(velocity.x, 0, velocity.z);
            if (oldHVelocity != Vector3.zero) {
                var projectedNewVelocity = Vector3.Dot(
                    newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
                velocity = oldHVelocity
                    * Mathf.Clamp01(projectedNewVelocity)
                    + velocity.y * Vector3.up;
            } else velocity = new Vector3(0, velocity.y, 0);
            if (velocity.y<tempVelocity.y-0.001) {
                if (velocity.y<0) velocity = new Vector3(
                    x: velocity.x,
                    y: tempVelocity.y,
                    z: velocity.z);
                else WasJumping = false;
            } if (IsGrounded && !grounded) {
                IsGrounded = false;
                if ((transfer==Transfer.Initial
                || transfer==Transfer.PermaTransfer)) {
                    lastVelocity = platformVelocity;
                    velocity += platformVelocity;
                } transform.position += pushDownOffset * Vector3.up;
            } else if (!IsGrounded && grounded) {
                IsGrounded = true;
                IsJumping = false;
                SubtractNewPlatformVelocity();
                if (velocity.y<-terminalVelocity)
                    Kill(null,new StoryArgs());
            } if (MoveWithPlatform()) {
                activeGlobalPoint =
                    transform.position
                    + Vector3.up*(cr.center.y-cr.height*0.5f+cr.radius);
                activeLocalPoint = activePlatform.InverseTransformPoint(activeGlobalPoint);
                activeGlobalRotation = transform.rotation;
                activeLocalRotation = Quaternion.Inverse(
                    activePlatform.rotation) * activeGlobalRotation;
            }
            slidingSpeed = duck?4f:15f;
            tgtCrouch = duck?1.62f:2f;
            if (recentlyLanded) tgtCrouch = tgtCrouchLand;
            if (Mathf.Abs(deltaHeight-tgtCrouch)<0.01f)
                deltaHeight = tgtCrouch;
            deltaHeight = Mathf.SmoothDamp(
                deltaHeight, tgtCrouch,
                ref deltaCrouch, 0.06f,
                64, Time.smoothDeltaTime);
            cr.height = deltaHeight;
            cr.center = Vector3.up*(deltaHeight/2f);
        }

        Vector3 applyDeltaVelocity(Vector3 tempVelocity) {
            // the horizontal to calculate direction from the IsJumping event
            Vector3 desiredVelocity;
            if (IsGrounded && TooSteep()) {
            // and to support walljumping I need to change horizontal here
                desiredVelocity = new Vector3(
                    x: groundNormal.x,
                    y: 0,
                    z: groundNormal.z).normalized;
                var projectedMoveDir = Vector3.Project(
                    inputMove, desiredVelocity);
                desiredVelocity = desiredVelocity+projectedMoveDir*speedControl
                    + (inputMove - projectedMoveDir) * lateralControl;
                desiredVelocity *= slidingSpeed;
            } else desiredVelocity = GetDesiredHorizontalVelocity();
            if (transfer==Transfer.PermaTransfer) {
                desiredVelocity += lastVelocity;
                desiredVelocity.y = 0;
            } if (IsGrounded)
                desiredVelocity = AdjustGroundVelocityToNormal(
                    desiredVelocity, groundNormal);
            else tempVelocity.y = 0;
            // Enforce zero on Y because the axes are calculated separately
            var maxDelta = GetMaxAcceleration(IsGrounded) * Time.deltaTime;
            var velocityChangeVector = (desiredVelocity - tempVelocity);
            if (velocityChangeVector.sqrMagnitude > maxDelta * maxDelta)
                velocityChangeVector = velocityChangeVector.normalized * maxDelta;
            if (IsGrounded) tempVelocity += velocityChangeVector;
            if (IsGrounded) tempVelocity.y = Mathf.Min(velocity.y, 0);
            if (!jump) { // This second section aplies only the vertical axis motion but
                // the reason I've conjoined these two is because I now have an
                WasJumping = false;
                // interaction between the user's vertical & horizontal vectors
                lastEndTime = -100;
            } if (jump && lastEndTime<0) lastEndTime = Time.time;
            if (IsGrounded)
                tempVelocity.y = Mathf.Min(0,tempVelocity.y) - -Physics.gravity.y * Time.deltaTime;
            else {
                tempVelocity.y = velocity.y - -Physics.gravity.y*2*Time.deltaTime;
                if (IsJumping && WasJumping) {
                   if (Time.time<lastStartTime + extraHeight / CalculateJumpVerticalSpeed(speedVertical))
                        tempVelocity += jumpDir * -Physics.gravity.y*2 * Time.deltaTime;
                } tempVelocity.y = Mathf.Max(tempVelocity.y, -maxSpeed);
            } if (IsGrounded) {
                if (Time.time-lastEndTime<0.2) {
                    IsGrounded = false;
                    IsJumping = true;
                    lastStartTime = Time.time;
                    lastEndTime = -100;
                    WasJumping = true;
                    if (TooSteep()) jumpDir = Vector3.Slerp(
                        Vector3.up, groundNormal, weightSteep);
                    else jumpDir = Vector3.Slerp(
                        Vector3.up, groundNormal, weightPerp);
                    tempVelocity.y = 0;
                    tempVelocity += jumpDir * CalculateJumpVerticalSpeed(speedVertical);
                    if (transfer==Transfer.Initial
                    || transfer==Transfer.PermaTransfer) {
                        lastVelocity = platformVelocity;
                        tempVelocity += platformVelocity;
                    }
                } else WasJumping = false;
            } else if (cr.collisionFlags==CollisionFlags.Sides)
                Vector3.Slerp(Vector3.up,lastColl.normal,lateralControl);
            return tempVelocity;
        }


        void OnControllerColliderHit(ControllerColliderHit hit) {
            //Player.OnCollisionEnter(hit.collider);
            var other = hit.collider.attachedRigidbody;
            lastColl = hit;
            if (other && hit.moveDirection.y>-0.05)
                other.velocity = new Vector3(
                    hit.moveDirection.x,0,hit.moveDirection.z)
                    *(massPlayer+other.mass)/(2*-Physics.gravity.y);
            if (hit.normal.y>0
            && hit.normal.y>groundNormal.y
            && hit.moveDirection.y<0) {
                if ((hit.point - lastHitPoint).sqrMagnitude>0.001
                || lastGroundNormal==Vector3.zero)
                    groundNormal = hit.normal;
                else groundNormal = lastGroundNormal;
                hitPlatform = hit.collider.transform;
                lastVelocity = Vector3.zero;
                hitPoint = hit.point;
            }
        }

        IEnumerator SubtractNewPlatformVelocity() {
            if (transfer==Transfer.Initial
            || transfer==Transfer.PermaTransfer) {
                if (newPlatform) {
                    var platform = activePlatform;
                    yield return new WaitForFixedUpdate();
                    yield return new WaitForFixedUpdate();
                    if (IsGrounded && platform==activePlatform) yield break;
                } velocity -= platformVelocity;
            }
        }

        Vector3 GetDesiredHorizontalVelocity() {
            var dirDesired = transform.InverseTransformDirection(inputMove);
            var maxSpeed = 0.0f;
            if (dirDesired != Vector3.zero) {
                var zAxisMult = ((dirDesired.z>0)
                    ? speedAnterior
                    : speedPosterior) / speedLateral;
                if (dash && IsGrounded) zAxisMult *= modSprint;
                else if (duck && IsGrounded) zAxisMult *= modCrouch;
                var temp = new Vector3(
                    x: dirDesired.x,
                    y: 0,
                    z: dirDesired.z / zAxisMult).normalized;
                maxSpeed = new Vector3(
                    x: temp.x,
                    y: 0,
                    z: temp.z*zAxisMult).magnitude * speedLateral;
            } if (IsGrounded) {
                var angle = Mathf.Asin(velocity.normalized.y) * Mathf.Rad2Deg;
                maxSpeed *= responseSlope.Evaluate(angle);
            } return transform.TransformDirection(dirDesired * maxSpeed);
        }

        bool MoveWithPlatform() =>
            (IsGrounded || transfer==Transfer.PermaLocked) && (activePlatform);

        Vector3 AdjustGroundVelocityToNormal(
                        Vector3 vector,
                        Vector3 normal) =>
            Vector3.Cross(Vector3.Cross(
                Vector3.up,vector),normal).normalized * vector.magnitude;

        float GetMaxAcceleration(bool IsGrounded) =>
            IsGrounded ? dampingGround : dampingAirborne;

        float CalculateJumpVerticalSpeed(float tgtHeight) =>
            Mathf.Sqrt(2*tgtHeight*-Physics.gravity.y*2);

        bool isTouchingCeiling() =>
            (hitFlags&CollisionFlags.CollidedAbove)!=0;

        bool TooSteep() =>
            (groundNormal.y <= Mathf.Cos(cr.slopeLimit*Mathf.Deg2Rad));

    }
}
