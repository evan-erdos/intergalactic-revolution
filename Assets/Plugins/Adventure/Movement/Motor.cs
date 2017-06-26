/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-01-17 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Movement {
    public class Motor : Object, IMotor {
        bool wait, newPlatform, recentlyLanded;
        uint massPlayer = 80;
        float
            dampingGround = 30, dampingAirborne = 20,
            lastStartTime = 0, lastEndTime = -100,
            tgtCrouch = 0, tgtCrouchLand = 1.5f, maxSpeed = 57.2f;
        protected bool jump, dash, duck;
        public float
            modSprint = 1.6f, modCrouch = 0.8f,
            speedAnterior = 16, speedLateral = 12,
            speedPosterior = 10, speedVertical = 1,
            deltaHeight = 2, weightPerp = 0,
            weightSteep = 0.5f, extraHeight = 4.1f,
            slidingSpeed = 15, lateralControl = 1,
            speedControl = 0.4f, deltaCrouch = 1,
            landingDuration = 0.15f, terminalVelocity = 30;
        [SerializeField] Transfer transfer = Transfer.PermaTransfer;
        [SerializeField] AnimationCurve responseSlope =
            new AnimationCurve(new Keyframe(-90,1), new Keyframe(90,0));
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

        public enum Transfer { None, Initial, PermaTransfer, PermaLocked }
        public float Volume => dash?0.2f:duck?0.05f:0.1f;
        public float Rate => dash?0.15f:duck?0.3f:0.2f;
        public void Move(Vector3 move, bool duck, bool jump) => inputMove = new Vector3(move.x, 0, move.z);
        public bool OnMove(Person actor, StoryArgs args) => false;
        public bool IsJumping {
            get { return isJumping; }
            protected set {
                if (isJumping!=value) { if (value && !WasJumping) OnJump(); else OnLand(); }
                (WasJumping,isJumping) = (isJumping,value); }
        } bool isJumping = false;

        public bool IsDisabled {get;set;}
        public bool WasGrounded {get;set;}
        public bool IsGrounded {get;protected set;}
        public bool WasJumping {get;protected set;}
        public bool grounded => groundNormal.y>0.01;
        public bool IsSliding => IsGrounded && TooSteep();
        public bool IsSprinting => dash;
        public Vector3 LocalPosition => transform.localPosition;
        public Vector3 lastVelocity {get;protected set;}
        public Vector3 Velocity {
            get { return velocity; }
            protected set {
                (IsGrounded,velocity,lastVelocity) = (false,value,Vector3.zero); }
        } Vector3 velocity = Vector3.zero;

        public void Kill() {
            IsDisabled = true;
            var rb = Get<Rigidbody>();
            (rb.isKinematic,rb.useGravity,rb.freezeRotation) = (false,true,false);
            rb.AddForce(velocity,ForceMode.VelocityChange);
            if (Get<Look>()) Get<Look>().enabled = false;
            (cr.enabled, this.enabled) = (false,false);
        }


        public void Kill(Actor actor, StoryArgs args) => Kill();
        public void OnJump() { }
        public void OnLand() => StartCoroutine(Landed());

        public IEnumerator Landed() {
            if (wait) yield break;
            wait = true;
            recentlyLanded = true;
            yield return new WaitForSeconds(landingDuration);
            recentlyLanded = false;
            wait = false;
        }

        void Awake() => cr = Get<CharacterController>();

        void Update() {
            if (Mathf.Abs(Time.timeScale)<0.001f) return;
            if (modSprint==0 || modCrouch==0 || speedAnterior==0 || speedLateral==0 || speedPosterior==0) return;
            var dirVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            if (dirVector != Vector3.zero) {
                var dirLength = dirVector.magnitude;
                dirVector /= dirLength;
                dirLength = Mathf.Min(1,dirLength);
                dirLength = dirLength * dirLength;
                dirVector = dirVector * dirLength;
            } inputMove = transform.rotation * dirVector;
            if (!IsDisabled) MoveMotor();
        }

        public void OnCollisionEvent(Collision collision) { }

        void FixedUpdate() {
            if ((Mathf.Abs(Time.timeScale)>0.1f) && activePlatform != null) {
                if (!newPlatform) platformVelocity =
                    (activePlatform.localToWorldMatrix.MultiplyPoint3x4(activeLocalPoint)
                    - lastMatrix.MultiplyPoint3x4(activeLocalPoint))
                    / ((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
                lastMatrix = activePlatform.localToWorldMatrix;
                newPlatform = false;
            } else platformVelocity = Vector3.zero;
        }

        void OnControllerColliderHit(ControllerColliderHit hit) {
            var other = hit.collider.attachedRigidbody; // Player.OnCollisionEnter(hit.collider);
            lastColl = hit;
            if (other && hit.moveDirection.y>-0.05) other.velocity = new Vector3(
                hit.moveDirection.x,0,hit.moveDirection.z) * (massPlayer+other.mass)/(2*-Physics.gravity.y);
            if (hit.normal.y>0 && hit.normal.y>groundNormal.y && hit.moveDirection.y<0) {
                groundNormal = ((hit.point-lastHitPoint).sqrMagnitude>0.001 || lastGroundNormal==Vector3.zero)
                    ? hit.normal : lastGroundNormal;
                (hitPlatform, lastVelocity, hitPoint) = (hit.collider.transform, Vector3.zero, hit.point);
            }
        }


        void MoveMotor() {
            var (tempVelocity, moveDistance) = (velocity, Vector3.zero);
            tempVelocity = ApplyDeltaVelocity(tempVelocity);
            if (MoveWithPlatform()) CalculatePlatformMove();
            var lastPosition = transform.position;
            var curOffset = tempVelocity * ((Mathf.Abs(Time.deltaTime)>0.01f)?Time.deltaTime:0.01f);
            var pushDownOffset = Mathf.Max(cr.stepOffset, new Vector3(curOffset.x, 0, curOffset.z).magnitude);
            if (IsGrounded) curOffset -= pushDownOffset*Vector3.up;
            (hitPlatform, groundNormal) = (null, Vector3.zero);
            // This one moves the user and returns the direction of the hit
            hitFlags = cr.Move(curOffset);
            (lastHitPoint,lastGroundNormal) = (hitPoint, groundNormal);
            if (activePlatform != hitPlatform && hitPlatform != null)
                (activePlatform, lastMatrix, newPlatform) =
                    (hitPlatform, hitPlatform.localToWorldMatrix, true);
            var oldHVelocity = new Vector3(tempVelocity.x,0,tempVelocity.z);
            velocity = (transform.position-lastPosition)
                / ((Mathf.Abs(Time.deltaTime)>0.01f) ? Time.deltaTime : 0.01f);
            var newHVelocity = new Vector3(velocity.x, 0, velocity.z);
            if (oldHVelocity != Vector3.zero)
                velocity = oldHVelocity * Mathf.Clamp01(
                    Vector3.Dot(newHVelocity, oldHVelocity)
                    / oldHVelocity.sqrMagnitude) + velocity.y * Vector3.up;
            else velocity = new Vector3(0, velocity.y, 0);
            if (velocity.y<tempVelocity.y-0.001) {
                if (velocity.y<0) velocity = new Vector3(
                    x: velocity.x, y: tempVelocity.y, z: velocity.z);
                else WasJumping = false;
            } if (IsGrounded && !grounded) {
                IsGrounded = false;
                if ((transfer==Transfer.Initial || transfer==Transfer.PermaTransfer)) {
                    lastVelocity = platformVelocity;
                    velocity += platformVelocity;
                } transform.position += pushDownOffset * Vector3.up;
            } else if (!IsGrounded && grounded) {
                (IsGrounded, IsJumping) = (true, false);
                SubtractNewPlatformVelocity();
                if (velocity.y<-terminalVelocity) Kill(null,new StoryArgs());
            } if (MoveWithPlatform()) {
                activeGlobalPoint =
                    transform.position + Vector3.up*(cr.center.y-cr.height*0.5f+cr.radius);
                activeLocalPoint = activePlatform.InverseTransformPoint(activeGlobalPoint);
                activeGlobalRotation = transform.rotation;
                activeLocalRotation = Quaternion.Inverse(activePlatform.rotation) * activeGlobalRotation;
            }
            slidingSpeed = duck?4f:15f;
            tgtCrouch = duck?1.62f:2f;
            if (recentlyLanded) tgtCrouch = tgtCrouchLand;
            if (Mathf.Abs(deltaHeight-tgtCrouch)<0.01f) deltaHeight = tgtCrouch;
            deltaHeight = Mathf.SmoothDamp(
                deltaHeight, tgtCrouch, ref deltaCrouch, 0.06f, 64, Time.smoothDeltaTime);
            cr.height = deltaHeight;
            cr.center = Vector3.up*(deltaHeight/2f);

            void CalculatePlatformMove() {
                var newGlobalPoint = activePlatform.TransformPoint(activeLocalPoint);
                moveDistance = (newGlobalPoint - activeGlobalPoint);
                if (moveDistance != Vector3.zero) cr.Move(moveDistance);
                var newGlobalRotation = activePlatform.rotation*activeLocalRotation;
                var rotationDiff = newGlobalRotation * Quaternion.Inverse(activeGlobalRotation);
                var yRotation = rotationDiff.eulerAngles.y;
                if (yRotation!=0) transform.Rotate(0,yRotation,0);
            }


            Vector3 ApplyDeltaVelocity(Vector3 vect) {
                // the horizontal to calculate direction from the IsJumping event
                Vector3 lerpVelocity;
                if (IsGrounded && TooSteep()) {
                    // and to support walljumping I need to change horizontal here
                    lerpVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
                    var projectedMoveDir = Vector3.Project(inputMove, lerpVelocity);
                    lerpVelocity = lerpVelocity+projectedMoveDir*speedControl
                        + (inputMove - projectedMoveDir) * lateralControl;
                    lerpVelocity *= slidingSpeed;
                } else lerpVelocity = GetDesiredHorizontalVelocity();
                if (transfer==Transfer.PermaTransfer) {
                    lerpVelocity += lastVelocity; lerpVelocity.y = 0; }
                if (IsGrounded) lerpVelocity = AdjustGroundVelocityToNormal(lerpVelocity,groundNormal);
                else vect.y = 0;
                // Enforce zero on Y because the axes are calculated separately
                var maxDelta = GetMaxAcceleration(IsGrounded) * Time.deltaTime;
                var velocityChangeVector = (lerpVelocity - vect);
                if (velocityChangeVector.sqrMagnitude > maxDelta * maxDelta)
                    velocityChangeVector = velocityChangeVector.normalized * maxDelta;
                if (IsGrounded) vect += velocityChangeVector;
                if (IsGrounded) vect.y = Mathf.Min(velocity.y, 0);
                if (!jump) (WasJumping, lastEndTime) = (false,-100);
                if (jump && lastEndTime<0) lastEndTime = Time.time;
                if (IsGrounded) {
                    vect.y = velocity.y - -Physics.gravity.y*2*Time.deltaTime;
                    if (IsJumping && WasJumping) {
                       if (Time.time<lastStartTime + extraHeight / CalculateJumpVerticalSpeed(speedVertical))
                            vect += jumpDir * -Physics.gravity.y*2 * Time.deltaTime;
                    } vect.y = Mathf.Max(vect.y, -maxSpeed);
                } else vect.y = Mathf.Min(0,vect.y) - -Physics.gravity.y * Time.deltaTime;

                if (IsGrounded) {
                    if (Time.time-lastEndTime<0.2) {
                        (IsGrounded, IsJumping, WasJumping) = (false, true, true);
                        (lastStartTime, lastEndTime) = (Time.time, -100);
                        jumpDir = Vector3.Slerp(Vector3.up, groundNormal, TooSteep()?weightSteep:weightPerp);
                        vect.y = 0;
                        vect += jumpDir * CalculateJumpVerticalSpeed(speedVertical);
                        if (transfer==Transfer.Initial || transfer==Transfer.PermaTransfer) {
                            lastVelocity = platformVelocity; vect += platformVelocity; }
                    } else WasJumping = false;
                } else if (cr.collisionFlags==CollisionFlags.Sides)
                    Vector3.Slerp(Vector3.up,lastColl.normal,lateralControl);
                return vect;
            }

            Vector3 GetDesiredHorizontalVelocity() {
                var (dirDesired, maxSpeed) = (transform.InverseTransformDirection(inputMove), 0f);
                if (dirDesired != Vector3.zero) {
                    var zAxisMult = ((dirDesired.z>0) ? speedAnterior : speedPosterior) / speedLateral;
                    if (dash && IsGrounded) zAxisMult *= modSprint;
                    else if (duck && IsGrounded) zAxisMult *= modCrouch;
                    var temp = new Vector3(dirDesired.x, 0, dirDesired.z / zAxisMult).normalized;
                    maxSpeed = new Vector3(temp.x, 0, temp.z*zAxisMult).magnitude * speedLateral;
                } if (IsGrounded) maxSpeed *= responseSlope.Evaluate(
                    Mathf.Asin(velocity.normalized.y) * Mathf.Rad2Deg);
                return transform.TransformDirection(dirDesired * maxSpeed);
            }

            IEnumerator SubtractNewPlatformVelocity() {
                if (transfer!=Transfer.Initial && transfer!=Transfer.PermaTransfer) yield break;
                velocity -= platformVelocity;
                if (!newPlatform) yield break;
                var platform = activePlatform;
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                if (IsGrounded && platform==activePlatform) yield break;
            }

            bool MoveWithPlatform() => (IsGrounded || transfer==Transfer.PermaLocked) && (activePlatform);
            // bool IsTouchingCeiling() => (hitFlags&CollisionFlags.CollidedAbove)!=0;
            float GetMaxAcceleration(bool IsGrounded) => IsGrounded ? dampingGround : dampingAirborne;
            float CalculateJumpVerticalSpeed(float tgtHeight) => Mathf.Sqrt(2*tgtHeight*-Physics.gravity.y*2);
            Vector3 AdjustGroundVelocityToNormal(Vector3 vector, Vector3 normal) =>
                Vector3.Cross(Vector3.Cross(Vector3.up,vector),normal).normalized * vector.magnitude;
        }

        bool TooSteep() => (groundNormal.y <= Mathf.Cos(cr.slopeLimit*Mathf.Deg2Rad));
    }
}
