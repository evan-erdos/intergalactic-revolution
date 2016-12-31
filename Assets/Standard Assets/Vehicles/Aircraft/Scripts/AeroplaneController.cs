using System;
using UnityEngine;

namespace UnityStandardAssets.Vehicles.Aeroplane {
    [RequireComponent(typeof(Rigidbody))]
    public class AeroplaneController : MonoBehaviour {
        bool boost, isDisabled;
        float initDrag, initAngularDrag, aeroCoefficient;
        // initAeroEffect;
        float bankedTurnAmount, powerBoost = 80;
        new Rigidbody rigidbody;
        [SerializeField] float maxEnginePower = 40f;
        [SerializeField] float Lift = 0.002f;
        [SerializeField] float zeroLiftSpeed = 300;
        [SerializeField] float rollEffect = 1f;
        [SerializeField] float pitchEffect = 1f;
        [SerializeField] float yawEffect = 0.2f;
        [SerializeField] float bankedTurnEffect = 0.5f;
        [SerializeField] float aerodynamicEffect = 0.02f;
        [SerializeField] float turnPitchAuto = 0.5f;
        [SerializeField] float rollLevelAuto = 0.2f;
        [SerializeField] float pitchLevelAuto = 0.2f;
        [SerializeField] float airBrakesEffect = 3f;
        [SerializeField] float throttleEffect = 0.3f;
        [SerializeField] public float dragCoefficient = 0.001f;
        [SerializeField] float boostForce = 200f;
        [SerializeField] float boostCapacity = 200f;
        [SerializeField] float boostRate = 20f;
        [SerializeField] float steeringFactor = 0.9f;
        [SerializeField] float topSpeed = 1500f;
        [SerializeField] float wingspan = 4;
        [SerializeField] float maneuveringThrust = 10f;
        public bool AirBrakes {get;protected set;}
        public bool Boost {
            get { return boost && powerBoost>1; }
            private set { boost = value; } }
        public float MaxEnginePower {get { return maxEnginePower; } }
        public float MaxBoost {get { return powerBoost/boostCapacity; } }
        public float Throttle {get;protected set;}
        public float ForwardSpeed {get;protected set;}
        public float EnginePower {get;protected set;}
        public float RollAngle {get;protected set;}
        public float PitchAngle {get;protected set;}
        public float RollInput {get;protected set;}
        public float PitchInput {get;protected set;}
        public float YawInput {get;protected set;}
        public float SteepInput {get;protected set;}
        public float ThrottleInput {get;protected set;}
        public float AerodynamicEffect {get; set;}
        public float TopSpeed {get;protected set;}

        public void Immobilize() { isDisabled = true; }
        public void Reset() { isDisabled = false; }

        void Awake() {
            AerodynamicEffect = aerodynamicEffect;
            // initAeroEffect = AerodynamicEffect;
            powerBoost = boostCapacity;
            TopSpeed = topSpeed;
            rigidbody = GetComponent<Rigidbody>();
            initDrag = rigidbody.drag;
            initAngularDrag = rigidbody.angularDrag;
        }

        void FixedUpdate() {
            powerBoost += Time.fixedDeltaTime*(Boost?-boostRate:boostRate/2f);
            powerBoost = Mathf.Clamp(powerBoost, 0, boostCapacity);
        }

        public void Move(
                        float roll = 0f,
                        float pitch = 0f,
                        float yaw = 0f,
                        float steep = 0f,
                        float throttle = 0f,
                        bool airBrakes = false,
                        bool boost = false) {

            RollInput = Mathf.Clamp(roll, -1, 1);
            PitchInput = Mathf.Clamp(pitch, -1, 1);
            YawInput = Mathf.Clamp(yaw, -1, 1);
            SteepInput = Mathf.Clamp(steep, -1, 1);
            ThrottleInput = Mathf.Clamp(throttle, -1, 1);
            AirBrakes = airBrakes;
            Boost = boost;

            CalculateRollAndPitchAngles();
            AutoLevel();
            CalculateForwardSpeed();
            ControlThrottle();
            CalculateDrag();
            CaluclateAerodynamicEffect();
            CalculateForce();
            CalculateTorque();
            CalculateManeuverThrust();
        }

        void CalculateRollAndPitchAngles() {
            // calculate the flat forward direction (no y component)
            var flat = transform.forward;
            flat.y = 0;
            // if the flat forward vector is non-zero
            if (flat.sqrMagnitude > 0) {
                flat.Normalize();
                // calculate current pitch angle
                var localFlatForward = transform.InverseTransformDirection(flat);
                PitchAngle = Mathf.Atan2(localFlatForward.y, localFlatForward.z);
                // calculate current roll angle
                var plumb = Vector3.Cross(Vector3.up, flat);
                var localFlatRight = transform.InverseTransformDirection(plumb);
                RollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
            }
        }


        void AutoLevel() {
            // banked turn amount (between -1 and 1) is the sine of the roll
            bankedTurnAmount = Mathf.Sin(RollAngle);
            // auto level roll, if there's no roll input:
            if (RollInput==0) RollInput = -RollAngle*rollLevelAuto;
            if (PitchInput == 0f)
                PitchInput = -PitchAngle*pitchLevelAuto- Mathf.Abs(
                    bankedTurnAmount*bankedTurnAmount*turnPitchAuto);
        }


        // Forward speed is the speed in the planes's forward direction
        // (not the same as its velocity, eg if falling in a stall)
        void CalculateForwardSpeed() {
            ForwardSpeed = Mathf.Max(
                0, transform.InverseTransformDirection(rigidbody.velocity).z);
        }


        void ControlThrottle() {
            // override throttle if immobilized
            if (isDisabled) ThrottleInput = 0f;
            // Adjust throttle based on throttle input (or immobilized state)
            var deltaThrottle = ThrottleInput*Time.deltaTime*throttleEffect;
            Throttle = Mathf.Clamp(Throttle+deltaThrottle, 0, 1);
            // current engine power is just:
            EnginePower = Throttle*MaxEnginePower + (Boost?boostForce:0);
        }


        void CalculateDrag() {
            // increase the drag based on speed
            // (since a constant drag doesn't seem "Real" (tm) enough)
            var extraDrag = rigidbody.velocity.magnitude*dragCoefficient;
            // air brakes work by directly modifying drag
            rigidbody.drag = initDrag + extraDrag;
            rigidbody.drag *= (AirBrakes)?airBrakesEffect:1;
            // forward speed affects angular drag
            // at high forward speed it's much harder for the plane to spin
            var steeringDrag = Mathf.Max(300,ForwardSpeed)/TopSpeed;
            rigidbody.angularDrag = initAngularDrag+steeringDrag*steeringFactor;
            rigidbody.angularDrag *= (AirBrakes)?airBrakesEffect:1;
            rigidbody.angularDrag = Mathf.Max(4f,rigidbody.angularDrag);
        }


        void CaluclateAerodynamicEffect() {
            // will naturally try to align itself forward when moving
            if (rigidbody.velocity.magnitude<=0) return;
            aeroCoefficient = Vector3.Dot(
                transform.forward,
                rigidbody.velocity.normalized);
            // multipled by itself results in a desirable rolloff curve
            aeroCoefficient *= aeroCoefficient;
            // calculate new velocity by bending current velocity direction to
            // the the direction currently facing, by aeroFactor
            var newVelocity = Vector3.Lerp(
                rigidbody.velocity,
                transform.forward*ForwardSpeed,
                aeroCoefficient*ForwardSpeed*AerodynamicEffect*Time.deltaTime);

            rigidbody.velocity = newVelocity;
            // also rotate the plane towards the direction of movement

            rigidbody.rotation = Quaternion.Slerp(
                rigidbody.rotation,
                Quaternion.LookRotation(rigidbody.velocity, transform.up),
                AerodynamicEffect*Time.deltaTime);
        }


        void CalculateForce() {
            // Now calculate forces acting on the aeroplane:
            // we accumulate forces into this variable:
            var forces = Vector3.zero;
            // Add the engine power in the forward direction
            forces += EnginePower*transform.forward;
            // The direction that the lift force is applied is at right angles
            // to the plane's velocity (usually, this is 'up'!)
            var liftDirection = Vector3.Cross(
                rigidbody.velocity,
                transform.right).normalized;
            // The amount of lift drops off as the plane increases speed
            // in reality this occurs as the pilot retracts the flaps
            // shortly after takeoff, giving the plane less drag, but less lift.
            // because we don't simulate flaps
            var zeroLift = Lift*Mathf.InverseLerp(zeroLiftSpeed,0,ForwardSpeed);
            // Calculate and add the lift power
            var liftPower = ForwardSpeed*ForwardSpeed*zeroLift*aeroCoefficient;
            forces += liftPower*liftDirection;
            // add extra "torque" for the steep input
            forces += transform.up * SteepInput;

            rigidbody.AddForce(forces);
        }


        void CalculateTorque() {
            var torque = Vector3.zero;
            // Add torque for the pitch based on the pitch input.
            torque += PitchInput*pitchEffect*transform.right;
            // Add torque for the yaw based on the yaw input.
            torque += YawInput*yawEffect*transform.up;
            // Add torque for the roll based on the roll input.
            torque += -RollInput*rollEffect*transform.forward;
            // Add torque for banked turning.
            torque += bankedTurnAmount*bankedTurnEffect*transform.up;
            // or when not moving in the direction of the nose of the plane
            var spin = Mathf.Clamp(ForwardSpeed+10,0,TopSpeed)*aeroCoefficient;
            var dragCompensation = Mathf.Max(1,rigidbody.angularDrag);
            rigidbody.AddTorque(torque*spin*dragCompensation);
        }

        void CalculateManeuverThrust(float threshold=0.5f) {
            if (RollInput>threshold) ApplyBalancedForce(
                force: transform.up * maneuveringThrust * RollInput,
                displacement: transform.right*wingspan);
            if (RollInput<-threshold) ApplyBalancedForce(
                force: -transform.up * maneuveringThrust * RollInput,
                displacement: -transform.right*wingspan);
        }

        void ApplyBalancedForce(Vector3 force, Vector3 displacement) {
            rigidbody.AddForceAtPosition(
                force: force,
                position: transform.position + displacement);
            rigidbody.AddForceAtPosition(
                force: -force,
                position: transform.position - displacement);

            Debug.DrawRay(transform.position+displacement, force, Color.green);
            Debug.DrawRay(transform.position-displacement, -force, Color.cyan);
        }
    }
}
