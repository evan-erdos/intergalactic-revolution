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
        float energy, aeroFactor;
        new AudioSource audio;
        new Rigidbody rigidbody;
        LayerMask layerMask;
        Collider[] colliders = new Collider[32];
        FlightMode mode = FlightMode.Assisted;
        List<Spaceship> targets = new List<Spaceship>();
        List<IShipComponent> mechanics = new List<IShipComponent>();
        Stack<IDamageable> parts = new Stack<IDamageable>();
        List<ParticleSystem> hypertrail = new List<ParticleSystem>();

        [SerializeField] List<Blaster> blasters = new List<Blaster>();
        [SerializeField] List<Blaster> rockets = new List<Blaster>();
        [SerializeField] List<Blaster> others = new List<Blaster>();
        List<Blaster> weapons = new List<Blaster>();

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
        [SerializeField] float energyRate = 50;
        [SerializeField] float energyGain = 20;
        [SerializeField] float energyJump = 200;
        [SerializeField] float oversteer = 0.9f;
        [SerializeField] float topSpeed = 1500;
        [SerializeField] float maneuveringThrust = 100;
        [SerializeField] List<AudioClip> hitSounds = new List<AudioClip>();
        [SerializeField] RandList<AudioClip> sounds = new RandList<AudioClip>();
        [SerializeField] protected AudioClip modeClip;
        [SerializeField] protected AudioClip selectClip;
        [SerializeField] protected AudioClip hyperspaceClip;
        [SerializeField] protected GameObject pod;
        [SerializeField] protected GameObject explosion;
        [SerializeField] protected GameObject hyperspace;
        [SerializeField] protected AudioClip sound;
        [SerializeField] protected SpaceEvent onKill = new SpaceEvent();
        [SerializeField] protected SpaceEvent onJump = new SpaceEvent();
        [SerializeField] protected SpaceEvent onDamage = new SpaceEvent();
        public event SpaceAction KillEvent;
        public event SpaceAction JumpEvent;
        public event DamageAction DamageEvent;
        public bool IsDisabled {get;protected set;}
        public bool AirBrakes {get;protected set;}
        public float Roll {get;protected set;}
        public float Pitch {get;protected set;}
        public float Spin {get;protected set;}
        public float Yaw {get;protected set;}
        public float Shift {get;protected set;}
        public float RollAngle {get;protected set;}
        public float PitchAngle {get;protected set;}
        public Spaceship Target {get;protected set;}

        public int CargoSpace {get;protected set;} = 20;
        public float Throttle {get;protected set;} = 0;
        public float ForwardSpeed {get;protected set;}
        public float EnginePower {get;protected set;} = 800;
        public float CurrentPower {get;protected set;} = 800;
        public float AeroEffect {get;protected set;} = 0.02f;
        public float TopSpeed {get;protected set;} = 1500;
        public float Health {get;protected set;} = 400;
        public float MaxHealth {get;protected set;} = 400;
        public float EnergyCapacity {get;protected set;} = 2000;
        public float EnergyPotential => (Boost?-energyRate:energyGain);
        public float MaxThrottle => 2;
        public float EnergyJump => energyJump;
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
            (EnergyCapacity, Energy) = (energyCapacity, energyCapacity);
        }


        int nextWeapon = -1;
        public void SelectWeapon() {
            StartSemaphore(Selecting);
            IEnumerator Selecting() {
                weapons.ForEach(o => o.gameObject.SetActive(false));
                weapons.Clear();
                switch (++nextWeapon%3) {
                    case 0: weapons.AddRange(blasters); break;
                    case 1: weapons.AddRange(rockets); break;
                    case 2: weapons.AddRange(others); break; }
                weapons.ForEach(o => o.gameObject.SetActive(true));
                audio.PlayOneShot(selectClip);
                yield return new WaitForSeconds(0.3f);
            }
        }

        void Awake() {
            Reset();
            layerMask = LayerMask.NameToLayer("AI");
            // blasters.AddRange(GetComponentsInChildren<Blaster>());
            sounds.AddRange(hitSounds);
            mechanics.AddRange(GetComponentsInChildren<IShipComponent>());
            var query =
                from particles in GetComponentsInChildren<ParticleSystem>()
                where particles.name=="Hypertrail"
                select particles;
            hypertrail.AddRange(query);
            hypertrail.ForEach(o => o.gameObject.SetActive(false));
            parts = new Stack<IDamageable>(GetComponentsInChildren<IDamageable>());
            rigidbody = GetComponent<Rigidbody>();
            audio = gameObject.AddComponent<AudioSource>();
            (audio.clip, audio.loop, audio.playOnAwake) = (sound,true,false);
            onKill.AddListener((o,e) => OnKill());
            onJump.AddListener((o,e) => OnJump());
            onDamage.AddListener((o,e) => OnDamage(1));
        }

        IEnumerator Start() {
            blasters.ForEach(o => o.gameObject.SetActive(false));
            rockets.ForEach(o => o.gameObject.SetActive(false));
            others.ForEach(o => o.gameObject.SetActive(false));
            SelectWeapon();
            while (true) {
                yield return new WaitForSeconds(1);
                FindTargets();
                yield return null;
                targets.Sort((x,y) =>
                    transform.Distance(x.transform).CompareTo(
                        transform.Distance(y.transform)));
                if (targets.Count>0) Target = targets[0];
            }
        }

        void OnEnable() {
            KillEvent += onKill.Invoke;
            JumpEvent += onJump.Invoke;
            DamageEvent += (o,e) => OnDamage(e);
        }

        void OnDisable() {
            KillEvent -= onKill.Invoke;
            JumpEvent -= onJump.Invoke;
            DamageEvent -= (o,e) => OnDamage(e);
        }

        void FixedUpdate() => Energy += EnergyPotential*Time.fixedDeltaTime;
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
                } yield return new WaitForSeconds(0.05f);
            }
        }

        void SevereDamage() {
            if (0>=parts.Count) return;
            var part = parts.Pop();
            part.Damage(1000000);
            if (part is Blaster blaster) blaster.Disable();
        }

        void FindTargets(float radius=100000) {
            Physics.OverlapSphereNonAlloc(
                transform.position,radius,colliders,layerMask);
            foreach (var collider in colliders) {
                if (collider is null) return;
                if (collider.attachedRigidbody is null) continue;
                var ship = collider.attachedRigidbody.GetComponent<Spaceship>();
                if (ship) targets.Add(ship);
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
            bool PreFire() => weapons.Count>0;
            IEnumerator Firing() {
                // var blaster = blasters[++next%blasters.Count];
                var blaster = weapons[++next%weapons.Count];
                if (blaster.gameObject.activeSelf)
                    blaster.Fire(position, rotation, velocity);
                yield return new WaitForSeconds(blaster.Rate/weapons.Count);
            }
        }

        public void Move(
                        bool brakes = false,
                        bool boost = false,
                        float roll = 0,
                        float pitch = 0,
                        float yaw = 0,
                        float throttle = 0) {
            if (IsDisabled) return;
            Roll = Mathf.Clamp(roll,-1,1);
            Pitch = Mathf.Clamp(pitch,-1,1);
            Yaw = Mathf.Clamp(yaw,-1,1);
            Shift = Mathf.Clamp(throttle,-1,1);
            Spin = Mathf.Clamp(throttle,-1,1);
            AirBrakes = brakes;
            Boost = boost;
            ForwardSpeed = Mathf.Max(
                0,transform.InverseTransformDirection(rigidbody.velocity).z);

            switch (Mode) {
                case FlightMode.Manual: ManualFlight(); goto default;
                case FlightMode.Assisted: AssitedFlight(); goto default;
                case FlightMode.Navigation: NavigationFlight(); goto default;
                default: DefaultFlight(); break;
            }

            void ManualFlight() {
                (Throttle, Shift) = (0,-1); ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                rigidbody.AddForce(CalculateForce().ToVector());
                rigidbody.AddForce(CalculateThrust().ToVector());
                CalculateSpin();
                CalculateManeuverThrust();
            }

            void AssitedFlight() {
                (Throttle,CurrentPower) = ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                (rigidbody.velocity, rigidbody.rotation) = CaluclateAerodynamics();
                rigidbody.AddForce(CalculateForce().ToVector());
                rigidbody.AddTorque(CalculateTorque().ToVector());
                CalculateManeuverThrust();
            }

            void NavigationFlight() {
                CalculateRollAndPitchAngles();
                AutoLevel();
                (Throttle,CurrentPower) = ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                (rigidbody.velocity, rigidbody.rotation) = CaluclateAerodynamics();
                rigidbody.AddForce(CalculateForce().ToVector());
                rigidbody.AddTorque(CalculateTorque().ToVector());
                // CalculateSpin();
                CalculateManeuverThrust();
                // CalculateJump();
            }

            void DefaultFlight() {
                // CalculateRollAndPitchAngles();
                // AutoLevel();
                // (Throttle,Shift) = ControlThrottle();
                // (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                // (rigidbody.velocity, rigidbody.rotation) = CaluclateAerodynamics();
                // rigidbody.AddForce(CalculateForce().ToVector());
                // rigidbody.AddTorque(CalculateTorque().ToVector());
                // CalculateSpin();
                // CalculateManeuverThrust();
            }
        }

        (float,float,float) CalculateThrust() =>
            (transform.forward*(Boost?energyThrust:0)).ToTuple();

        void CalculateJump() {
            if (!Boost || !AirBrakes || Energy<20) return;
            Jump(Quaternion.LookRotation(Vector3.left),null);
        }

        public void Jump(Quaternion direction, StarSystem system) {
            StartSemaphore(Jumping);
            IEnumerator Jumping() {
                IsDisabled = true;
                while (--Throttle>0) yield return new WaitForSeconds(0.1f);
                (Throttle, Shift) = (0,0);
                (rigidbody.drag, rigidbody.angularDrag) = (10,10);
                while (Mathf.Abs(Quaternion.Dot(transform.rotation,direction))<0.9999f) {
                    yield return new WaitForFixedUpdate();
                    rigidbody.rotation = Quaternion.Slerp(
                        rigidbody.rotation, direction, Time.fixedDeltaTime); }
                audio.PlayOneShot(hyperspaceClip);
                Throttle = 20;
                ControlThrottle();
                rigidbody.AddForce(transform.forward*1000, ForceMode.Impulse);
                hypertrail.ForEach(o => {o.gameObject.SetActive(true);o.Play();});
                yield return new WaitForSeconds(5);
                Create(hyperspace);
                transform.position += transform.forward*100;
                rigidbody.AddForce(transform.forward*100000, ForceMode.Impulse);
                IsDisabled = false;
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
            pod.GetComponentsInChildren<ParticleSystem>().ForEach(o => o.Play());
        }

        void OnJump() => hypertrail.ForEach(o => o.Stop());

        void OnDamage(float damage) {
            if (hasJettisoned) return;
            Health -= damage;
            StartSemaphore(Damaging);
            IEnumerator Damaging() {
                audio.PlayOneShot(sounds.Pick(),1);
                if (damage>200) SevereDamage();
                if (Health<0 && !hasJettisoned) Kill();
                yield return new WaitForSeconds(0.1f);
            }
        }

        void Kill() { if (KillEvent!=null) KillEvent(this,new SpaceArgs()); }

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
                Create(explosion);
                yield return new WaitForSeconds(1);
                enabled = false;
                audio.Stop();
            }
        }

        void CalculateRollAndPitchAngles() {
            var flat = new Vector3(transform.forward.x,0,transform.forward.z);
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
            var bankedTurnAmount = Mathf.Sin(RollAngle);
            if (Roll==0) Roll = -RollAngle*rollLevelAuto;
            if (Pitch==0) Pitch = -PitchAngle*pitchLevelAuto - Mathf.Abs(
                bankedTurnAmount*bankedTurnAmount*turnPitchAuto);
        }

        (float,float) ControlThrottle() {
            if (isDisabled) return (0,0);
            var throt = Shift*Time.fixedDeltaTime*throttleEffect;
            throt = Mathf.Clamp(Throttle+throt,0,MaxThrottle);
            return (throt,throt*EnginePower+(Boost?energyThrust:0));
        }

        (float,float) CalculateDrag() {
            if (isDisabled) return (0, rigidbody.angularDrag);
            var (drag, angularDrag) = (0f,0f);
            drag += rigidbody.velocity.magnitude*dragEffect*0.5f;
            drag *= (AirBrakes)?airBrakesEffect:1;
            angularDrag += Mathf.Max(300,rigidbody.velocity.magnitude)/TopSpeed;
            angularDrag *= oversteer*((AirBrakes)?airBrakesEffect:1);
            angularDrag = Mathf.Max(4f,angularDrag);
            return (drag, angularDrag);
        }

        (Vector3,Quaternion) CaluclateAerodynamics() {
            if (rigidbody.velocity.magnitude<=0)
                return (rigidbody.velocity, rigidbody.rotation);
            aeroFactor = Vector3.Dot(
                transform.forward, rigidbody.velocity.normalized);
            aeroFactor *= aeroFactor;
            var velocity = Vector3.Lerp(
                rigidbody.velocity, transform.forward*ForwardSpeed,
                aeroFactor*ForwardSpeed*AeroEffect*Time.deltaTime);
            var rotation = Quaternion.identity;
            if (rigidbody.velocity.sqrMagnitude>0.1f)
                rotation = Quaternion.Slerp(
                    rigidbody.rotation,
                    Quaternion.LookRotation(rigidbody.velocity,transform.up),
                    AeroEffect*Time.deltaTime);
            return (velocity, rotation);
        }

        (float,float,float) CalculateForce() {
            var (Lift, zeroLiftSpeed) = (0,0); // (0.002f, 300);
            var forces = CurrentPower*transform.forward;
            var liftDirection = Vector3.Cross(rigidbody.velocity,transform.right);
            var zeroLift = Lift*Mathf.InverseLerp(zeroLiftSpeed,0,ForwardSpeed);
            var liftPower = ForwardSpeed*ForwardSpeed*zeroLift*aeroFactor;
            forces += liftPower*liftDirection.normalized;
            return (forces.x, forces.y, forces.z);
        }

        (float,float,float) CalculateTorque() {
            var torque = Vector3.zero;
            torque += Pitch*pitchEffect*transform.right;
            torque += Yaw*yawEffect*transform.up;
            torque -= Roll*rollEffect*transform.forward;
            // torque += bankedTurnEffect*bankedTurnAmount*transform.up
            torque *= Mathf.Clamp(ForwardSpeed,0,TopSpeed)*aeroFactor;
            torque *= Mathf.Max(1,rigidbody.angularDrag);
            return (torque.x, torque.y, torque.z);
        }

        void CalculateSpin(float threshold=0.5f) {
            if (Mathf.Abs(Spin)<=threshold) return;
            Throttle = 0;
            var direction = (0<Spin)?rigidbody.velocity:-rigidbody.velocity;
            rigidbody.rotation = Quaternion.Lerp(
                rigidbody.rotation,
                Quaternion.LookRotation(direction,transform.up),
                spinEffect*Time.fixedDeltaTime);
        }

        void CalculateManeuverThrust(float threshold=0.1f) {
            var wingspan = 4;
            ManeuverRoll();
            ManeuverPitch();
            ManeuverYaw();

            void ManeuverRoll() {
                if (Roll>threshold) ApplyBalancedForce(
                    force: -transform.up * maneuveringThrust * Roll,
                    displacement: transform.right*wingspan);
                if (Roll<-threshold) ApplyBalancedForce(
                    force: transform.up * maneuveringThrust * Roll,
                    displacement: -transform.right*wingspan);
            }

            void ManeuverPitch() {
                if (Pitch>threshold) ApplyBalancedForce(
                    force: transform.forward * maneuveringThrust * Pitch,
                    displacement: transform.up*wingspan);
                if (Pitch<-threshold) ApplyBalancedForce(
                    force: -transform.forward * maneuveringThrust * Pitch,
                    displacement: -transform.up*wingspan);
            }

            void ManeuverYaw() {
                if (Yaw>threshold) ApplyBalancedForce(
                    force: transform.right * maneuveringThrust * Yaw,
                    displacement: transform.forward*wingspan);
                if (Yaw<-threshold) ApplyBalancedForce(
                    force: -transform.right * maneuveringThrust * Yaw,
                    displacement: -transform.forward*wingspan);
            }
        }

        void ApplyBalancedForce(Vector3 force, Vector3 displacement) {
            rigidbody.AddForceAtPosition(force,transform.position+displacement);
            rigidbody.AddForceAtPosition(-force,transform.position-displacement);
        }
    }
}
