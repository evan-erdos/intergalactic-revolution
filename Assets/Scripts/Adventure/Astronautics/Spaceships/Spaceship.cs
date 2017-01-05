/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class Spaceship : SpaceObject, ISpaceship {
        bool hasJettisoned, boost, isDisabled;
        float energy, bankedTurnAmount, aeroCoefficient, radius = 100000;
        new AudioSource audio;
        new Rigidbody rigidbody;
        LayerMask layerMask;
        Collider[] colliders = new Collider[32];
        FlightMode mode = FlightMode.Assisted;
        List<Spaceship> targets = new List<Spaceship>();
        List<IShipComponent> mechanics = new List<IShipComponent>();
        Stack<IDamageable> parts = new Stack<IDamageable>();
        List<ParticleSystem> hypertrail = new List<ParticleSystem>();
        List<Blaster> blasters = new List<Blaster>();
        [SerializeField] float health = 400;
        [SerializeField] float enginePower = 800;
        [SerializeField] float rollEffect = 1;
        [SerializeField] float pitchEffect = 1;
        [SerializeField] float yawEffect = 0.2f;
        [SerializeField] float spinEffect = 1;
        [SerializeField] float airBrakesEffect = 3;
        [SerializeField] float throttleEffect = 0.5f;
        [SerializeField] float aerodynamicEffect = 0.02f;
        [SerializeField] float dragEffect = 0.001f;
        [SerializeField] float energyThrust = 400;
        [SerializeField] float energyCapacity = 2000;
        [SerializeField] float energyRate = 20;
        [SerializeField] float energyGain = 20;
        [SerializeField] float oversteer = 0.9f;
        [SerializeField] float topSpeed = 1500;
        [SerializeField] float wingspan = 4;
        [SerializeField] float maneuveringThrust = 100;
        [SerializeField] List<AudioClip> hitSounds = new List<AudioClip>();
        [SerializeField] RandList<AudioClip> sounds = new RandList<AudioClip>();
        [SerializeField] protected AudioClip modeClip;
        [SerializeField] protected AudioClip hyperspaceClip;
        [SerializeField] protected GameObject pod;
        [SerializeField] protected GameObject explosion;
        [SerializeField] protected GameObject hyperspace;
        [SerializeField] protected AudioClip sound;
        [SerializeField] protected Blaster rockets;
        [SerializeField] protected SpaceEvent onKill = new SpaceEvent();
        [SerializeField] protected SpaceEvent onJump = new SpaceEvent();
        [SerializeField] protected SpaceEvent onDamage = new SpaceEvent();
        public event SpaceAction KillEvent;
        public event SpaceAction JumpEvent;
        public event DamageAction DamageEvent;
        public bool AirBrakes {get;protected set;}
        public bool IsDisabled {get;protected set;}
        public float Throttle {get;protected set;}
        public float ForwardSpeed {get;protected set;}
        public float EnginePower {get;protected set;}
        public float CurrentPower {get;protected set;}
        public float RollAngle {get;protected set;}
        public float PitchAngle {get;protected set;}
        public float RollInput {get;protected set;}
        public float PitchInput {get;protected set;}
        public float SpinInput {get;protected set;}
        public float YawInput {get;protected set;}
        public float SteepInput {get;protected set;}
        public float ThrottleInput {get;protected set;}
        public float AeroEffect {get;protected set;}
        public float TopSpeed {get;protected set;}
        public float Health {get;protected set;}
        public float MaxHealth {get;protected set;}
        public float EnergyCapacity {get; protected set;}
        public Spaceship Target {get;protected set;}
        public (float x,float y,float z) Velocity => rigidbody.velocity.ToTuple();
        public bool Boost {
            get { return boost && Energy>1; }
            protected set { boost = value; } }
        public float Energy {
            get { return energy; }
            protected set { energy = Mathf.Clamp(value, 0, EnergyCapacity); } }
        public FlightMode Mode {
            get { return mode; }
            set { mode = value; OnChangeMode(); } }

        public void Disable() => (isDisabled, Throttle) = (true, 0);
        public void Alarm() => audio.Play();
        public void Damage(float damage) => DamageEvent(this, damage);
        public void Reset() {
            (isDisabled, hasJettisoned) = (false, false);
            (Health, MaxHealth) = (health, health);
            (EnginePower, CurrentPower) = (enginePower, enginePower);
            (AeroEffect, TopSpeed) = (aerodynamicEffect, topSpeed);
            (Energy, EnergyCapacity) = (energy, energyCapacity);
        }

        void Awake() {
            Reset();
            layerMask = LayerMask.NameToLayer("AI");
            foreach (var blaster in GetComponentsInChildren<Blaster>())
                if (blaster.name=="High Energy Blaster") blasters.Add(blaster);
            sounds.AddRange(hitSounds);
            mechanics.AddRange(GetComponentsInChildren<IShipComponent>());
            var query =
                from particles in GetComponentsInChildren<ParticleSystem>()
                where particles.name=="Hypertrail"
                select particles;
            hypertrail.AddRange(query);
            if (0<hypertrail.Count)
                hypertrail.ForEach(o => o.gameObject.SetActive(false));
            parts = new Stack<IDamageable>(GetComponentsInChildren<IDamageable>());
            rigidbody = GetComponent<Rigidbody>();
            audio = gameObject.AddComponent<AudioSource>();
            (audio.clip, audio.loop, audio.playOnAwake) = (sound, true, false);
            onKill.AddListener((o,e) => OnKill());
            onJump.AddListener((o,e) => OnJump());
            onDamage.AddListener((o,e) => OnDamage(1));
        }

        IEnumerator Start() {
            StartCoroutine(RegenerateBoost());
            while (true) {
                yield return new WaitForSeconds(1);
                FindTargets();
                yield return null;
                targets.Sort((x,y) =>
                    transform.Distance(x.transform).CompareTo(
                        transform.Distance(y.transform)));
                if (targets.Count>0) Target = targets[0];
            }

            IEnumerator RegenerateBoost() {
                var wait = new WaitForSeconds(1);
                while (true) { yield return wait; Energy += energyGain; }
            }
        }

        void OnEnable() {
            KillEvent += onKill.Invoke;
            JumpEvent += onJump.Invoke;
            DamageEvent += (o,e) => OnDamage(e); // icky, use onDamage.Invoke
        }

        void OnDisable() {
            KillEvent -= onKill.Invoke;
            JumpEvent -= onJump.Invoke;
            DamageEvent -= (o,e) => OnDamage(e); // icky, use onDamage.Invoke
        }

        void FixedUpdate() => Energy -= (Boost)?energyRate*Time.fixedDeltaTime:0;
        void OnCollisionEnter(Collision c) => Damage(c.impulse.magnitude);

        void OnChangeMode() {
            StartSemaphore(ChangingMode);
            void Manual() => (AeroEffect, dragEffect) = (0,0);
            void Assisted() => (AeroEffect, dragEffect) = (aerodynamicEffect,0.0002f);
            void Navigation() => (AeroEffect, dragEffect) = (aerodynamicEffect*2,0);
            IEnumerator ChangingMode() {
                audio.PlayOneShot(modeClip);
                switch (mode) {
                    case FlightMode.Manual: Manual(); break;
                    case FlightMode.Assisted: Assisted(); break;
                    case FlightMode.Navigation: Navigation(); break;
                } yield return new WaitForSeconds(0.1f);
            }
        }

        void SevereDamage() {
            if (0>=parts.Count) return;
            var part = parts.Pop();
            part.Damage(1000000);
            if (!(part is Blaster blaster)) return;
            blaster.Disable();
            blasters.Remove(blaster);
        }

        void FindTargets() {
            Physics.OverlapSphereNonAlloc(
                position: transform.position,
                radius: radius,
                results: colliders,
                layerMask: layerMask);
            foreach (var collider in colliders) {
                if (collider is null) return;
                if (collider.attachedRigidbody is null) continue;
                var ship = collider.attachedRigidbody.GetComponent<Spaceship>();
                if (ship) targets.Add(ship);
            }
        }

        void OnDamage(float damage) {
            if (hasJettisoned) return;
            Health -= damage;
            StartSemaphore(Damaging);
            IEnumerator Damaging() {
                audio.PlayOneShot(sounds.Pick(),1);
                if (damage>200) SevereDamage();
                if (Health<0) KillEvent(this, new SpaceArgs());
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void Fire() =>
            Fire(transform.forward, transform.rotation, rigidbody.velocity);
        public void Fire(Vector3 position, Quaternion rotation, Vector3 velocity) =>
            Fire(position.ToTuple(), rotation, velocity.ToTuple());

        int next; // on the way out
        public void Fire(
                        (float,float,float) position,
                        Quaternion rotation,
                        (float,float,float) velocity) {
            if (PreFire()) StartSemaphore(Firing);
            bool PreFire() => blasters.Count>0;
            IEnumerator Firing() {
                var blaster = blasters[++next%blasters.Count];
                blaster.Fire(position, rotation, velocity);
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void FireRockets() => FireRockets(Target);
        public void FireRockets(SpaceObject target) => rockets.Fire(
            position: (target is null)
                ? transform.forward*200
                : target.transform.position,
            velocity: rigidbody.velocity);

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

            switch (Mode) {
                case FlightMode.Manual: ManualFlight(); break;
                case FlightMode.Assisted: AssitedFlight(); break;
                case FlightMode.Navigation: NavigationFlight(); break;
                default: DefaultFlight(); break;
            }

            void ManualFlight() {
                (Throttle, ThrottleInput) = (0,0);
                // CalculateRollAndPitchAngles();
                // AutoLevel();
                CalculateForwardSpeed();
                // ControlThrottle();
                CalculateDrag();
                // CaluclateAerodynamics();
                CalculateForce();
                // CalculateTorque();
                CalculateSpin();
                CalculateManeuverThrust();
                CalculateYawThrust();
                CalculateJump();
            }

            void AssitedFlight() {
                // CalculateRollAndPitchAngles();
                // AutoLevel();
                CalculateForwardSpeed();
                ControlThrottle();
                CalculateDrag();
                CaluclateAerodynamics();
                CalculateForce();
                CalculateTorque();
                CalculateManeuverThrust();
            }

            void NavigationFlight() {
                CalculateRollAndPitchAngles();
                AutoLevel();
                CalculateForwardSpeed();
                ControlThrottle();
                CalculateDrag();
                CaluclateAerodynamics();
                CalculateForce();
                CalculateTorque();
                CalculateSpin();
                CalculateManeuverThrust();
                CalculateJump();
                CalculateSpin();
            }

            void DefaultFlight() {
                CalculateRollAndPitchAngles();
                AutoLevel();
                CalculateForwardSpeed();
                ControlThrottle();
                CalculateDrag();
                CaluclateAerodynamics();
                CalculateForce();
                CalculateTorque();
                CalculateSpin();
                CalculateManeuverThrust();
                CalculateJump();
            }
        }



        void CalculateJump() {
            if (Mode!=FlightMode.Navigation) return;
            if (!Boost || !AirBrakes || Energy<20) return;
            Jump(Quaternion.LookRotation(Vector3.left), null);
        }

        public void Jump(Quaternion direction, StarSystem system) {
            StartSemaphore(Jumping);
            IEnumerator Jumping() {
                IsDisabled = true;
                //while (--Throttle>0) yield return new WaitForSeconds(0.1f);
                (Throttle, ThrottleInput) = (0,0);
                (rigidbody.drag, rigidbody.angularDrag) = (10,10);
                while (Mathf.Abs(Quaternion.Dot(transform.rotation,direction))<0.9999f) {
                    yield return new WaitForFixedUpdate();
                    rigidbody.rotation = Quaternion.Slerp(
                        rigidbody.rotation, direction, Time.fixedDeltaTime); }
                audio.PlayOneShot(hyperspaceClip);
                Throttle = 20;
                ControlThrottle();
                Boost = true;
                rigidbody.AddForce(transform.forward*1000, ForceMode.Impulse);
                hypertrail.ForEach(o => {o.gameObject.SetActive(true);o.Play();});
                yield return new WaitForSeconds(5);
                Instantiate(hyperspace, transform.position, Quaternion.identity);
                transform.position += transform.forward*100;
                rigidbody.AddForce(transform.forward*100000, ForceMode.Impulse);
                IsDisabled = false;
                hypertrail.ForEach(o => o.Stop());
                yield return new WaitForSeconds(1);
                hypertrail.ForEach(o => o.gameObject.SetActive(false));
                yield return new WaitForSeconds(3);
                JumpEvent(this, new SpaceArgs());
            }
        }

        public void Jettison() {
            if (!pod || hasJettisoned) return; hasJettisoned = true;
            pod.transform.SetParent(null);
            var rigidbody = pod.GetComponent<Rigidbody>();
            rigidbody.isKinematic = false;
            rigidbody.velocity = GetComponent<Rigidbody>().velocity;
            rigidbody.angularVelocity = GetComponent<Rigidbody>().angularVelocity;
            rigidbody.AddForce(transform.up*200, ForceMode.VelocityChange);
            Array.ForEach(pod.GetComponentsInChildren<ParticleSystem>(),
                o => o.Play());
        }

        void OnJump() => Instantiate(explosion, transform.position, transform.rotation);

        void OnKill() {
            if (hasJettisoned) return;
            if (Health<-2000) StartSemaphore(HaltAndCatchFire);
            else StartSemaphore(Killing);

            IEnumerator HaltAndCatchFire() {
                Disable(); Alarm();
                yield return new WaitForSeconds(2);
                while (0<parts.Count) { SevereDamage();
                    yield return new WaitForSeconds(0.1f); }
                StartCoroutine(Killing());
            }

            IEnumerator Killing() {
                if (hasJettisoned) yield break;
                while (0<parts.Count) { SevereDamage();
                    yield return new WaitForSeconds(0.1f); }
                Jettison();
                mechanics.ForEach(o => o.Disable());
                Instantiate(explosion, transform.position, transform.rotation);
                yield return new WaitForSeconds(1);
                Instantiate(explosion, transform.position, transform.rotation);
                enabled = false;
                audio.Stop();
            }
        }


        void CalculateRollAndPitchAngles() {
            var flat = new Vector3(transform.forward.x, 0, transform.forward.z);
            if (flat.sqrMagnitude<=0) return;
            flat.Normalize();
            var localFlatForward = transform.InverseTransformDirection(flat);
            PitchAngle = Mathf.Atan2(localFlatForward.y, localFlatForward.z);
            var plumb = Vector3.Cross(Vector3.up, flat);
            var localFlatRight = transform.InverseTransformDirection(plumb);
            RollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
        }

        void AutoLevel() {
            var (turnPitchAuto, pitchLevelAuto, rollLevelAuto) = (0,0,0);
            bankedTurnAmount = Mathf.Sin(RollAngle);
            if (RollInput==0) RollInput = -RollAngle*rollLevelAuto;
            if (PitchInput==0) PitchInput = -PitchAngle*pitchLevelAuto - Mathf.Abs(
                bankedTurnAmount*bankedTurnAmount*turnPitchAuto);
        }

        void CalculateForwardSpeed() => ForwardSpeed = Mathf.Max(
            0, transform.InverseTransformDirection(rigidbody.velocity).z);

        void ControlThrottle() {
            if (isDisabled) { ThrottleInput = 0; Throttle = 0; return; }
            var deltaThrottle = ThrottleInput*Time.fixedDeltaTime*throttleEffect;
            Throttle = Mathf.Clamp(Throttle+deltaThrottle, 0, 1.5f);
            CurrentPower = Throttle*EnginePower + (Boost?energyThrust:0);
        }

        void CalculateDrag() {
            if (isDisabled) { rigidbody.drag = 0; return; }
            rigidbody.drag = rigidbody.velocity.magnitude*dragEffect * 0.5f;
            rigidbody.drag *= (AirBrakes)?airBrakesEffect:1;
            var steeringDrag = Mathf.Max(300,rigidbody.velocity.magnitude)/TopSpeed;
            rigidbody.angularDrag = steeringDrag*oversteer;
            rigidbody.angularDrag *= (AirBrakes)?airBrakesEffect:1;
            rigidbody.angularDrag = Mathf.Max(4f,rigidbody.angularDrag);
        }

        void CaluclateAerodynamics() {
            if (rigidbody.velocity.magnitude<=0) return;
            aeroCoefficient = Vector3.Dot(
                transform.forward,
                rigidbody.velocity.normalized);
            aeroCoefficient *= aeroCoefficient;
            var newVelocity = Vector3.Lerp(
                rigidbody.velocity,
                transform.forward*ForwardSpeed,
                aeroCoefficient*ForwardSpeed*AeroEffect*Time.deltaTime);

            rigidbody.velocity = newVelocity;

            if (rigidbody.velocity.sqrMagnitude>0.1f)
                rigidbody.rotation = Quaternion.Slerp(
                    rigidbody.rotation,
                    Quaternion.LookRotation(rigidbody.velocity, transform.up),
                    AeroEffect*Time.deltaTime);
        }

        void CalculateForce() {
            var (Lift, zeroLiftSpeed) = (0,0); // (0.002f, 300);
            var forces = CurrentPower*transform.forward;
            var liftDirection = Vector3.Cross(rigidbody.velocity,transform.right);
            var zeroLift = Lift*Mathf.InverseLerp(zeroLiftSpeed,0,ForwardSpeed);
            var liftPower = ForwardSpeed*ForwardSpeed*zeroLift*aeroCoefficient;
            forces += liftPower*liftDirection.normalized;
            forces += transform.up * SteepInput;
            rigidbody.AddForce(forces);
        }

        void CalculateTorque() {
            var torque = Vector3.zero;
            var bankedTurnEffect = 0f;
            torque += PitchInput*pitchEffect*transform.right;
            torque += YawInput*yawEffect*transform.up;
            torque -= RollInput*rollEffect*transform.forward;
            torque += bankedTurnAmount*bankedTurnEffect*transform.up;
            var spin = Mathf.Clamp(ForwardSpeed,0,TopSpeed)*aeroCoefficient;
            var dragCompensation = Mathf.Max(1,rigidbody.angularDrag);
            rigidbody.AddTorque(torque*spin*dragCompensation);
        }

        void CalculateSpin(float threshold=0.1f) {
            if (Mathf.Abs(SpinInput)<=threshold) return;
            if (Mode!=FlightMode.Manual) return;
            Throttle = 0;
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

        void CalculateYawThrust(float threshold=0.1f) {
            if (YawInput>threshold) ApplyBalancedForce(
                force: -transform.right * maneuveringThrust * YawInput,
                displacement: transform.forward*3);
            if (YawInput<-threshold) ApplyBalancedForce(
                force: transform.right * maneuveringThrust * YawInput,
                displacement: -transform.forward*3);
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
