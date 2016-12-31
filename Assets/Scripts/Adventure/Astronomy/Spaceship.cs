/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Astronomy.Aeronautics {
    public class Spaceship : SpaceObject, ISpaceship {
        bool once, hasJettisoned, boost, isDisabled;
        int next;
        float initDrag, initAngularDrag, initAeroEffect, initDragCoefficient;
        float bankedTurnAmount, aeroCoefficient;
        float powerBoost = 80, radius = 100000;
        new AudioSource audio;
        new Rigidbody rigidbody;
        LayerMask layerMask;
        Collider[] colliders = new Collider[32];
        List<Spaceship> targets = new List<Spaceship>();
        List<IShipComponent> mechanics = new List<IShipComponent>();
        Stack<IDamageable> parts = new Stack<IDamageable>();
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
        [SerializeField] float throttleEffect = 0.5f;
        [SerializeField] float dragCoefficient = 0.001f;
        [SerializeField] float boostForce = 200f;
        [SerializeField] float boostCapacity = 200f;
        [SerializeField] float boostRate = 20f;
        [SerializeField] float steeringFactor = 0.9f;
        [SerializeField] float topSpeed = 1500f;
        [SerializeField] float spinEffect = 1f;
        [SerializeField] float wingspan = 4;
        [SerializeField] float maneuveringThrust = 100f;
        [SerializeField] float health = 400;
        [SerializeField] List<AudioClip> hitSounds = new List<AudioClip>();
        [SerializeField] RandList<AudioClip> sounds = new RandList<AudioClip>();
        [SerializeField] protected new GameObject camera;
        [SerializeField] protected GameObject view;
        [SerializeField] protected GameObject pod;
        [SerializeField] protected GameObject explosion;
        [SerializeField] protected GameObject fire;
        [SerializeField] protected AudioClip sound;
        protected List<Blaster> blasters = new List<Blaster>();
        [SerializeField] protected Blaster rockets;
        [SerializeField] protected SpaceEvent onKill;
        [SerializeField] protected SpaceEvent onDamage;
        public event SpaceAction KillEvent, DamageEvent;
        public bool AirBrakes {get;protected set;}
        public bool IsDisabled {get;protected set;}
        public float MaxEnginePower => maxEnginePower;
        public float MaxBoost => powerBoost/boostCapacity;
        public Vector3 Velocity => rigidbody.velocity;
        public float Throttle {get;protected set;}
        public float ForwardSpeed {get;protected set;}
        public float EnginePower {get;protected set;}
        public float RollAngle {get;protected set;}
        public float PitchAngle {get;protected set;}
        public float RollInput {get;protected set;}
        public float PitchInput {get;protected set;}
        public float SpinInput {get;protected set;}
        public float YawInput {get;protected set;}
        public float SteepInput {get;protected set;}
        public float ThrottleInput {get;protected set;}
        public float AerodynamicEffect {get; set;}
        public float TopSpeed {get;protected set;}
        public float Health {get;protected set;}
        public float MaxHealth {get;protected set;}
        public Spaceship Target {get;protected set;}
        public bool Boost {
            get { return boost && powerBoost>1; }
            private set { boost = value; } }
        public FlightMode Mode {
            get { return mode; }
            set { mode = value;
                switch (mode) {
                    case FlightMode.Manual:
                        AerodynamicEffect = 0;
                        dragCoefficient = 0;
                        bankedTurnEffect = 0;
                        turnPitchAuto = 0;
                        pitchLevelAuto = 0;
                        rollLevelAuto = 0;
                        break;
                    case FlightMode.Assisted:
                        bankedTurnEffect = 0.5f;
                        turnPitchAuto = 0.5f;
                        pitchLevelAuto = 0.2f;
                        rollLevelAuto = 0.2f;
                        AerodynamicEffect = initAeroEffect;
                        dragCoefficient = initDragCoefficient;
                        break;
                    case FlightMode.Directed:
                        bankedTurnEffect = 0;
                        turnPitchAuto = 0;
                        pitchLevelAuto = 0;
                        rollLevelAuto = 0;
                        AerodynamicEffect = initAeroEffect;
                        dragCoefficient = initDragCoefficient;
                        break;
                }
            }
        } FlightMode mode = FlightMode.Directed;

        protected override void Awake() { base.Awake();
            MaxHealth = health;
            Health = MaxHealth;
            layerMask = LayerMask.NameToLayer("AI");
            foreach (var blaster in GetComponentsInChildren<Blaster>())
                if (blaster.name=="High Energy Blaster") blasters.Add(blaster);
            sounds.AddRange(hitSounds);
            mechanics.AddRange(GetComponentsInChildren<IShipComponent>());
            parts = new Stack<IDamageable>(GetComponentsInChildren<IDamageable>());
            audio = gameObject.AddComponent<AudioSource>();
            rigidbody = GetComponent<Rigidbody>();
            audio.clip = sound;
            audio.loop = true;
            audio.playOnAwake = false;
            if (onKill==null) onKill = new SpaceEvent();
            onKill.AddListener((o,e) => OnKill());
            KillEvent += onKill.Invoke;

            onDamage.AddListener((o,e) => OnDamage(1));
            DamageEvent += onDamage.Invoke;

            AerodynamicEffect = aerodynamicEffect;
            initAeroEffect = AerodynamicEffect;
            initDragCoefficient = dragCoefficient;
            powerBoost = boostCapacity;
            TopSpeed = topSpeed;
            rigidbody = GetComponent<Rigidbody>();
            initDrag = rigidbody.drag;
            initAngularDrag = rigidbody.angularDrag;
        }


        IEnumerator Start() {
            while (true) {
                yield return new WaitForSeconds(0.5f);
                FindTargets();
                yield return null;
                targets.Sort((x,y) =>
                    transform.Distance(x.transform).CompareTo(
                        transform.Distance(y.transform)));
                if (targets.Count>0) Target = targets[0];
            }
        }

        void FixedUpdate() {
            powerBoost += Time.fixedDeltaTime*(Boost?-boostRate:boostRate/1.5f);
            powerBoost = Mathf.Clamp(powerBoost, 0, boostCapacity);
        }

        void OnCollisionEnter(Collision collision) {
            Damage(collision.impulse.magnitude);
            foreach (var point in collision.contacts) {
                var thing = point.thisCollider.GetComponentInParent<DetachNacelle>();
                if (thing) thing.Damage(collision.impulse.sqrMagnitude);
            }
        }

        public void Reset() { Health = MaxHealth; isDisabled = false; }
        public void SevereDamage() { if (parts.Count>0) parts.Pop().Damage(1000); }
        public void Disable() { IsDisabled = true; }
        public void Alarm() { audio.Play(); }
        void OnKill() { HaltAndCatchFire(); }

        void FindTargets() {
            Physics.OverlapSphereNonAlloc(
                position: transform.position,
                radius: radius,
                results: colliders,
                layerMask: layerMask);
            foreach (var collider in colliders) {
                if (collider==null || !collider.attachedRigidbody) continue;
                var ship = collider.attachedRigidbody.GetComponent<Spaceship>();
                if (ship) targets.Add(ship);
            }
        }

        public void Damage(float damage) {
            Health -= damage;
            DamageEvent(this, new SpaceArgs());
            if (damage>200) SevereDamage();
            if (Health<-200) Detonate();
            else if (Health<0) KillEvent(this, new SpaceArgs());
        }

        void OnDamage(float damage) {
            audio.PlayOneShot(sounds.Pick(),1);
        }


        public void Fire() => Fire(
            position: transform.forward,
            velocity: rigidbody.velocity,
            rotation: view.transform.rotation);

        public void Fire(Vector3 position, Vector3 velocity, Quaternion rotation) {
            StartSemaphore(Firing);
            IEnumerator Firing() {
                var blaster = blasters[++next%blasters.Count];
                // blaster.Fire(position,velocity); // rotation
                blaster.Fire(position,velocity); // rotation
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void FireRockets() => FireRockets(Target);
        public void FireRockets(SpaceObject target) {
            if (IsDisabled) return;
            rockets.Fire(
                position: (target!=null)
                    ? target.transform.position
                    : transform.forward*200f,
                velocity: rigidbody.velocity); }

        public void Move(
                        bool brakes = false,
                        bool boost = false,
                        float roll = 0,
                        float pitch = 0,
                        float yaw = 0,
                        float steep = 0,
                        float throttle = 0,
                        float spin=0) {
            if (IsDisabled) return;
            RollInput = Mathf.Clamp(roll, -1, 1);
            PitchInput = Mathf.Clamp(pitch, -1, 1);
            YawInput = Mathf.Clamp(yaw, -1, 1);
            SteepInput = Mathf.Clamp(steep, -1, 1);
            ThrottleInput = Mathf.Clamp(throttle, -1, 1);
            SpinInput = Mathf.Clamp(spin, -1, 1);
            AirBrakes = brakes;
            Boost = boost;

            CalculateRollAndPitchAngles();
            AutoLevel();
            CalculateForwardSpeed();
            ControlThrottle();
            CalculateDrag();
            CaluclateAerodynamicEffect();
            CalculateForce();
            CalculateTorque();
            CalculateSpin();
            CalculateManeuverThrust();
        }


        public void Jettison() {
            if (hasJettisoned) return; hasJettisoned = true;
            if (GetComponentInChildren<SpacePlayer>())
                GetComponentInChildren<SpacePlayer>().Restart();
            if (!pod) return;
            if (camera) camera.transform.SetParent(pod.transform);
            pod.transform.SetParent(null);
            var rigidbody = pod.GetComponent<Rigidbody>();
            rigidbody.isKinematic = false;
            rigidbody.velocity = GetComponent<Rigidbody>().velocity;
            rigidbody.angularVelocity = GetComponent<Rigidbody>().angularVelocity;
            rigidbody.AddForce(transform.up*20f, ForceMode.VelocityChange);
        }

        void HaltAndCatchFire() {
            StartSemaphore(HaltingAndCatchingFire);
            IEnumerator HaltingAndCatchingFire() {
                Disable();
                Alarm();
                yield return new WaitForSeconds(2);
                while (0<parts.Count) { SevereDamage();
                    yield return new WaitForSeconds(0.1f); }
                Detonate();
            }
        }

        void Detonate() {
            StartSemaphore(Detonating);
            IEnumerator Detonating() {
                Jettison();
                if (once) yield break; once = true;
                mechanics.ForEach(o => o.Disable());
                while (0<parts.Count) { SevereDamage();
                    yield return new WaitForSeconds(0.1f); }
                KillEvent(this, new SpaceArgs());
                yield return new WaitForSeconds(0.1f);
                Instantiate(
                    original: explosion,
                    position: transform.position,
                    rotation: transform.rotation);
                audio.Stop();
                enabled = false;
            }
        }

        void CalculateRollAndPitchAngles() {
            var flat = transform.forward;
            flat.y = 0;
            if (flat.sqrMagnitude<=0) return;
            flat.Normalize();
            var localFlatForward = transform.InverseTransformDirection(flat);
            PitchAngle = Mathf.Atan2(localFlatForward.y, localFlatForward.z);
            var plumb = Vector3.Cross(Vector3.up, flat);
            var localFlatRight = transform.InverseTransformDirection(plumb);
            RollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
        }

        void AutoLevel() {
            bankedTurnAmount = Mathf.Sin(RollAngle);
            if (RollInput==0) RollInput = -RollAngle*rollLevelAuto;
            if (PitchInput==0) PitchInput = -PitchAngle*pitchLevelAuto - Mathf.Abs(
                bankedTurnAmount*bankedTurnAmount*turnPitchAuto);
        }

        void CalculateForwardSpeed() => ForwardSpeed = Mathf.Max(
            0, transform.InverseTransformDirection(rigidbody.velocity).z);

        void ControlThrottle() {
            if (isDisabled) ThrottleInput = 0f;
            var deltaThrottle = ThrottleInput*Time.deltaTime*throttleEffect;
            Throttle = Mathf.Clamp(Throttle+deltaThrottle, 0, 1.5f);
            EnginePower = Throttle*MaxEnginePower + (Boost?boostForce:0);
        }

        void CalculateDrag() {
            var extraDrag = rigidbody.velocity.magnitude*dragCoefficient;
            rigidbody.drag = initDrag + extraDrag;
            rigidbody.drag *= (AirBrakes)?airBrakesEffect:1;
            var steeringDrag = Mathf.Max(300,rigidbody.velocity.magnitude)/TopSpeed;
            rigidbody.angularDrag = initAngularDrag+steeringDrag*steeringFactor;
            rigidbody.angularDrag *= (AirBrakes)?airBrakesEffect:1;
            rigidbody.angularDrag = Mathf.Max(4f,rigidbody.angularDrag);
        }

        void CaluclateAerodynamicEffect() {
            if (rigidbody.velocity.magnitude<=0) return;
            aeroCoefficient = Vector3.Dot(
                transform.forward,
                rigidbody.velocity.normalized);
            aeroCoefficient *= aeroCoefficient;
            var newVelocity = Vector3.Lerp(
                rigidbody.velocity,
                transform.forward*ForwardSpeed,
                aeroCoefficient*ForwardSpeed*AerodynamicEffect*Time.deltaTime);

            rigidbody.velocity = newVelocity;

            if (rigidbody.velocity.sqrMagnitude>0)
                rigidbody.rotation = Quaternion.Slerp(
                    rigidbody.rotation,
                    Quaternion.LookRotation(rigidbody.velocity, transform.up),
                    AerodynamicEffect*Time.deltaTime);
        }

        void CalculateForce() {
            var forces = Vector3.zero;
            forces += EnginePower*transform.forward;
            var liftDirection = Vector3.Cross(rigidbody.velocity,transform.right);
            var zeroLift = Lift*Mathf.InverseLerp(zeroLiftSpeed,0,ForwardSpeed);
            var liftPower = ForwardSpeed*ForwardSpeed*zeroLift*aeroCoefficient;
            forces += liftPower*liftDirection.normalized;
            forces += transform.up * SteepInput;
            rigidbody.AddForce(forces);
        }

        void CalculateTorque() {
            var torque = Vector3.zero;
            torque += PitchInput*pitchEffect*transform.right;
            torque += YawInput*yawEffect*transform.up;
            torque += -RollInput*rollEffect*transform.forward;
            torque += bankedTurnAmount*bankedTurnEffect*transform.up;
            var spin = Mathf.Clamp(ForwardSpeed,0,TopSpeed)*aeroCoefficient;
            var dragCompensation = Mathf.Max(1,rigidbody.angularDrag);
            rigidbody.AddTorque(torque*spin*dragCompensation);
        }

        void CalculateSpin(float threshold=0.1f) {
            if (Mathf.Abs(SpinInput)<=threshold) return;
            if (Mode!=FlightMode.Manual) return;
            var direction =  Quaternion.LookRotation(
                (0<SpinInput)?rigidbody.velocity:-rigidbody.velocity, transform.up);
            rigidbody.rotation = Quaternion.Slerp(
                rigidbody.rotation, direction, spinEffect*Time.fixedDeltaTime);
        }

        void CalculateManeuverThrust(float threshold=0.1f) {
            if (RollInput>threshold) ApplyBalancedForce(
                force: -transform.up * maneuveringThrust * RollInput,
                displacement: transform.right*wingspan);
            if (RollInput<-threshold) ApplyBalancedForce(
                force: transform.up * maneuveringThrust * RollInput,
                displacement: -transform.right*wingspan);
        }

        void ApplyBalancedForce(Vector3 force, Vector3 displacement) {
            rigidbody.AddForceAtPosition(
                force: force,
                position: transform.position + displacement);
            rigidbody.AddForceAtPosition(
                force: -force,
                position: transform.position - displacement);

#if _DEBUG
            Debug.DrawRay(transform.position+displacement, force/100f, Color.green);
            Debug.DrawRay(transform.position-displacement, -force/100f, Color.cyan);
#endif
        }
    }
}
