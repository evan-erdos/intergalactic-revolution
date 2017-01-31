/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using A=Adventure;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class Spaceship : SpaceObject, ISpaceship, ICreatable<SpaceshipProfile> {
        [SerializeField] protected SpaceshipProfile profile;
        float Shift, Brakes, rollAngle, pitchAngle, energyJumpFactor = 9;
        float rollEffect=1, pitchEffect=1, yawEffect=0.2f, spinEffect=1;
        float brakesEffect=3, throttleEffect=0.5f, dragEffect=0.001f;
        float energyLoss=50, energyGain=20, maneuveringEnergy=100;
        Pool hyperspaces;
        new AudioSource audio;
        new Rigidbody rigidbody;
        AudioClip modeClip, changeClip, selectClip, hyperspaceClip, alarmClip;
        GameObject explosion, hyperspace;
        List<AudioClip> hitSounds = new List<AudioClip>();
        Stack<IDamageable> parts = new Stack<IDamageable>();
        List<ITrackable> targets = new List<ITrackable>();
        List<IShipComponent> mechanics = new List<IShipComponent>();
        List<ParticleSystem> hypertrail = new List<ParticleSystem>();
        List<Weapon> weapons = new List<Weapon>();
        List<FlightMode> modes = new List<FlightMode> {
            FlightMode.Manual, FlightMode.Assisted};
        [SerializeField] List<Weapon> blasters = new List<Weapon>();
        [SerializeField] List<Weapon> rockets = new List<Weapon>();
        [SerializeField] protected SpaceEvent onKill = new SpaceEvent();
        [SerializeField] protected SpaceEvent onJump = new SpaceEvent();
        [SerializeField] protected SpaceEvent onDamage = new SpaceEvent();
        public event SpaceAction KillEvent;
        public event SpaceAction JumpEvent;
        public event DamageAction DamageEvent;
        public bool IsDisabled {get;protected set;} = false;
        public bool IsDead {get;protected set;} = false;
        public FlightMode Mode {get;protected set;} = FlightMode.Assisted;
        public int CargoSpace {get;protected set;} = 20; // tons
        public float Mass {get;protected set;} = 40; // tons
        public float Throttle {get;protected set;} = 0; // [0...1]
        public float EnginePower {get;protected set;} = 800; // kN
        public float CurrentPower {get;protected set;} = 800; // kN
        public float AeroEffect {get;protected set;} = 2; // drag coefficient
        public float TopSpeed {get;protected set;} = 1500; // m/s
        public float Health {get;protected set;} = 12000; // kN
        public float MaxHealth {get;protected set;} = 12000; // kN
        public float EnergyCapacity {get;protected set;} = 8000; // kN/L
        public float EnergyThrust {get;protected set;} = 6000; // kN
        public float ForwardSpeed {get;protected set;} = 0; // m/s
        public float Speed => rigidbody.velocity.magnitude; // m/s
        public float EnergyJump => energyJumpFactor*Mass;
        public float EnergyPotential => (Boost>0)?-energyLoss:energyGain;
        public float BoostPower => (Energy>1 && Boost>0.1f)?EnergyThrust:0;
        public float Energy {
            get { return Mathf.Clamp(energy,0,EnergyCapacity); }
            set { energy = value; } } float energy = 8000;
        public List<Vector3> Pivots {get;protected set;}
        public (float roll,float pitch,float yaw) Control {get;protected set;}
        public float Boost {get; protected set;}
        public Vector3 Velocity => rigidbody.velocity;
        public IWeapon Weapon {get;protected set;}
        public ITrackable Target {get;protected set;}
        public StarSystem CurrentSystem {get;protected set;}
        public StarProfile Destination {get;protected set;}
        public StarProfile[] Stars {get;set;} = new StarProfile[1];
        public void Kill() => KillEvent?.Invoke(this,new SpaceArgs());
        public void Jump() => JumpEvent?.Invoke(this,new SpaceArgs());
        public void Alarm() => audio.Play();
        public void Damage(float damage) => DamageEvent?.Invoke(this,damage);
        public void Fire() => Fire(Target);
        public void Fire(Vector3 position, Vector3 velocity) =>
            Fire(position.tuple(), velocity.tuple());
        public void Disable() =>
            (IsDisabled,rigidbody.drag,rigidbody.angularDrag) = (true,0,0);
        public void Reset() =>
            (IsDisabled,IsDead,rigidbody.mass,Health,Energy) =
                (false,false,Mass,MaxHealth,EnergyCapacity);

        public void Create(SpaceshipProfile profile) =>
            (Mass, Health, EnginePower,
            rollEffect, pitchEffect, yawEffect,
            spinEffect, brakesEffect, throttleEffect,
            AeroEffect, dragEffect, energyLoss,
            energyGain, EnergyThrust, EnergyCapacity,
            TopSpeed, maneuveringEnergy, Pivots,
            hitSounds, modeClip, changeClip,
            selectClip, hyperspaceClip, alarmClip,
            explosion, hyperspace) =
                (profile.Mass, profile.Health, profile.EnginePower,
                profile.RollEffect, profile.PitchEffect, profile.YawEffect,
                profile.SpinEffect, profile.BrakesEffect, profile.ThrottleEffect,
                profile.AeroEffect, profile.DragEffect, profile.EnergyLoss,
                profile.EnergyGain, profile.EnergyThrust, profile.EnergyCapacity,
                profile.TopSpeed, profile.ManeuveringEnergy, profile.Pivots,
                profile.hitSounds, profile.modeClip, profile.changeClip,
                profile.selectClip, profile.hyperspaceClip, profile.alarmClip,
                profile.explosion, profile.hyperspace);

        int nextSystem = -1; // gross
        public void SelectSystem() {
            StartSemaphore(Hyperspacing);
            IEnumerator Hyperspacing() {
                if (0>=Stars.Length) yield break;
                audio.PlayOneShot(selectClip);
                Destination = Stars[++nextSystem%Stars.Length];
                yield return new WaitForSeconds(0.1f);
            }
        }

        List<ITrackable> trackables = new List<ITrackable>();
        int nextTarget = -1; // ick
        public void SelectTarget() {
            StartSemaphore(Selecting);
            IEnumerator Selecting() {
                if (0>=targets.Count) yield break;
                audio.PlayOneShot(selectClip);
                Target = targets[++nextTarget%targets.Count];
                yield return new WaitForSeconds(0.1f);
            }
        }

        IEnumerable<ITrackable> GetTargets() {
            while (true) foreach (var o in trackables) yield return o; }

        int nextCamera = -1;
        public void ToggleView() {
            StartSemaphore(Toggling);
            IEnumerator Toggling() {
                var pivot = Pivots[++nextCamera%Pivots.Count];
                PlayerCamera.Pivot = pivot;
                yield return new WaitForSeconds(0.2f);
            }
        }

        int nextWeapon = -1;
        public void SelectWeapon() {
            StartSemaphore(Selecting);
            IEnumerator Selecting() {
                weapons.ForEach(o => o.gameObject.SetActive(false));
                weapons.Clear();
                nextWeapon = (1+nextWeapon)%2;
                switch (nextWeapon) {
                    case 0: weapons.Add(blasters); break;
                    case 1: weapons.Add(rockets); break; }
                nextFire = 0;
                weapons.ForEach(o => o.gameObject.SetActive(true));
                audio.PlayOneShot(changeClip);
                if (0<weapons.Count) Weapon = weapons.First();
                yield return new WaitForSeconds(0.1f);
            }
        }


        int nextMode = 0; // on the way out
        public void ChangeMode() {
            if (!IsDisabled) StartSemaphore(ChangingMode);
            IEnumerator ChangingMode() {
                audio.PlayOneShot(modeClip);
                Mode = modes[++nextMode%modes.Count];
                switch (Mode) {
                    case FlightMode.Manual: ChangeDrag(0,0); break;
                    case FlightMode.Assisted: ChangeDrag(AeroEffect); break;
                    case FlightMode.Navigation: ChangeDrag(AeroEffect,0); break;
                } yield return new WaitForSeconds(0.05f);
            }
        }

        void Awake() {
            Create(profile);
            mechanics.Add(GetComponentsInChildren<IShipComponent>());
            var query =
                from particles in GetComponentsInChildren<ParticleSystem>()
                where particles.name=="Hypertrail"
                select particles;
            hypertrail.Add(query);
            hypertrail.ForEach(o => o.gameObject.SetActive(false));
            parts = new Stack<IDamageable>(GetComponentsInChildren<IDamageable>());
            (rigidbody, audio) = (Get<Rigidbody>(), GetOrAdd<AudioSource>());
            (audio.clip,audio.loop, audio.playOnAwake) = (alarmClip,true,false);
            Reset();
            onKill.AddListener((o,e) => OnKill());
            onJump.AddListener((o,e) => OnHyperJump());
            onDamage.AddListener((o,e) => OnDamage(1));
            if (hyperspace==null) return;
            var hyperspaceInstances = new List<GameObject>();
            for (var i=0;i<2;++i) {
                var instance = Create(hyperspace);
                var projectile = instance.Get<IProjectile>();
                hyperspaceInstances.Add(instance);
                instance.transform.parent = transform;
                instance.transform.localPosition = Vector3.zero;
                instance.gameObject.layer = gameObject.layer;
                instance.gameObject.SetActive(false);
            } hyperspaces = new Pool(hyperspaceInstances);
        }

        IEnumerator Start() {
            var radius = 100000;
            var layerMask = 1<<LayerMask.NameToLayer("AI");
            var results = new Collider[32];
            blasters.ForEach(o => o.gameObject.SetActive(false));
            rockets.ForEach(o => o.gameObject.SetActive(false));
            SelectWeapon(); ChangeMode();
            while (true) {
                yield return new WaitForSeconds(1);
                Physics.OverlapSphereNonAlloc(
                    rigidbody.position,radius,results,layerMask);
                yield return null;
                foreach (var result in results) {
                    if (result?.attachedRigidbody is null) continue;
                    var ship = result.attachedRigidbody.Get<ITrackable>();
                    if (!targets.Contains(ship)) targets.Add(ship);
                } yield return null;
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
        void OnCollisionEnter(Collision c) => Damage(c.impulse.magnitude/4);

        void ChangeDrag(float aeroEffect, float dragCoefficient=0.0002f) {
            StartSemaphore(ChangingAero);
            StartSemaphore(ChangingDrag);
            IEnumerator ChangingAero() {
                var (time, speed, smooth, max) = (0f,0f,0.125f,100f);
                while (Mathf.Abs(AeroEffect-aeroEffect)>0.25f) yield return Wait(
                    wait: new WaitForFixedUpdate(),
                    func: () => AeroEffect = Mathf.SmoothDamp(
                        current: AeroEffect,
                        target: aeroEffect,
                        currentVelocity: ref speed,
                        smoothTime: smooth,
                        maxSpeed: max,
                        deltaTime: time+=Time.fixedDeltaTime/4));
            }

            IEnumerator ChangingDrag() {
                var (time, speed, smooth, max) = (0f,0f,0.125f,100f);
                while (dragEffect!=dragCoefficient) yield return Wait(
                    wait: new WaitForFixedUpdate(),
                    func: () => dragEffect = Mathf.SmoothDamp(
                        current: dragEffect,
                        target: dragCoefficient,
                        currentVelocity: ref speed,
                        smoothTime: smooth,
                        maxSpeed: max,
                        deltaTime: time+=Time.fixedDeltaTime/4));
            }
        }

        public void Fire(ITrackable target) {
            if (target is null) Fire(transform.forward.tuple(),(0,0,0));
            else if (PreFire()) StartSemaphore(Firing);
            bool PreFire() => !IsDisabled && !IsDead && weapons.Count>0;
            IEnumerator Firing() {
                var blaster = weapons[(++nextFire%weapons.Count)];
                blaster?.Fire(blaster.Target = target);
                yield return new WaitForSeconds(1/(blaster.Rate*weapons.Count));
            }
        }

        int nextFire = -1; // on the way out
        public void Fire(
                        (float,float,float) position,
                        (float,float,float) velocity) {
            if (PreFire()) StartSemaphore(Firing);
            bool PreFire() => !IsDisabled && !IsDead && weapons.Count>0;
            IEnumerator Firing() {
                var blaster = weapons[(++nextFire%weapons.Count)];
                blaster.Target = Target;
                blaster?.Fire(position,velocity);
                yield return new WaitForSeconds(1/(blaster.Rate*weapons.Count));
            }
        }

        public void Move() {
            var (brakes,boost,throttle,roll,pitch,yaw) = (0f,0f,0f,0f,0f,0f);
            // insert clever fallback ship autopiloting algorithm here
            Move(brakes,boost,throttle,roll,pitch,yaw);
        }

        public void Move(
                        float brakes = 0,
                        float boost = 0,
                        float throttle = 0,
                        float roll = 0,
                        float pitch = 0,
                        float yaw = 0) {
            if (IsDisabled || IsDead) return;
            Control = (Clamp(roll), Clamp(pitch), Clamp(yaw));
            (Brakes,Boost,Shift) = (Clamp(brakes), Clamp(boost), Clamp(throttle));
            var spin = Clamp(throttle);
            ForwardSpeed = transform.InverseTransformDirection(rigidbody.velocity).z;
            ForwardSpeed = Mathf.Max(0,ForwardSpeed);
            float Clamp(float input) => Mathf.Clamp(input,-1,1);

            switch (Mode) {
                case FlightMode.Manual: ManualFlight(); break;
                case FlightMode.Assisted: AssistedFlight(); break;
                case FlightMode.Navigation: NavigationFlight(); break;
                default: DefaultFlight(); break;
            }

            void ManualFlight() {
                (Throttle,CurrentPower,Shift) = (0,0,-1); ControlThrottle();
                (rigidbody.drag,rigidbody.angularDrag) = CalculateDrag();
                rigidbody.AddForce(CalculateThrust().vect()*2);
                CalculateSpin(spin);
                CalculateManeuverThrust();
            }

            void AssistedFlight() {
                (Throttle,CurrentPower) = ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                var aeroCoefficient = ComputeCoefficient();
                (rigidbody.velocity,rigidbody.rotation) = CalculateAerodynamics();
                rigidbody.AddForce(CalculateForce());
                rigidbody.AddTorque(CalculateTorque(aeroCoefficient).vect());
                CalculateManeuverThrust();
            }

            void NavigationFlight() {
                CaclulateAngles();
                AutoLevel();
                (Throttle,CurrentPower) = ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                var aeroCoefficient = ComputeCoefficient();
                (rigidbody.velocity, rigidbody.rotation) =
                    CalculateAerodynamics(aeroCoefficient);
                rigidbody.AddForce(CalculateForce());
                rigidbody.AddTorque(CalculateTorque(aeroCoefficient).vect());
                CalculateManeuverThrust();
                // HyperJump(Quaternion.LookRotation(Vector3.left),null);
            }

            void DefaultFlight() {
                CaclulateAngles();
                AutoLevel();
                (Throttle,Shift) = ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                var aeroCoefficient = ComputeCoefficient();
                (rigidbody.velocity,rigidbody.rotation) =
                    CalculateAerodynamics(aeroCoefficient);
                var liftForce = CalculateLift(0).vect();
                rigidbody.AddForce(liftForce+CalculateForce().vect());
                rigidbody.AddTorque(CalculateTorque(aeroCoefficient).vect());
                CalculateSpin(spin); // HyperJump();
                CalculateManeuverThrust();
            }
        }

        (float,float,float) CalculateThrust() =>
            (transform.forward*BoostPower).tuple();

        void OnHyperJump() => hypertrail.ForEach(o => o.Stop());

        public void HyperJump() {
            if (Energy>=EnergyJump/2) StartSemaphore(Jumping);
            IEnumerator Jumping() {
                Energy -= EnergyJump/2;
                rigidbody.AddForce(transform.forward*10000,ForceMode.Impulse);
                hypertrail.ForEach(o => {o.gameObject.SetActive(true);o.Play();});
                yield return new WaitForFixedUpdate();
                hyperspaces.Create(transform.position);
                yield return new WaitForFixedUpdate();
                transform.position += transform.forward*1000;
                yield return new WaitForSeconds(1);
                hypertrail.ForEach(o => o.Stop());
                yield return new WaitForSeconds(1);
                hypertrail.ForEach(o => o.gameObject.SetActive(false));
            }
        }

        public void HyperJump(Quaternion direction) {
            if (Energy>=EnergyJump/2) StartSemaphore(Jumping);
            IEnumerator Jumping() {
                IsDisabled = true;
                while (--Throttle>0) yield return new WaitForSeconds(0.1f);
                (Throttle, Shift) = (0,0);
                (rigidbody.drag, rigidbody.angularDrag) = (10,10);
                while (Mathf.Abs(Quaternion.Dot(transform.rotation,direction))<0.999f)
                    yield return Wait(
                        wait: new WaitForFixedUpdate(),
                        func: () => rigidbody.rotation = Quaternion.Slerp(
                            rigidbody.rotation, direction, Time.fixedDeltaTime));
                audio.PlayOneShot(hyperspaceClip);
                Throttle = 20;
                ControlThrottle();
                rigidbody.AddForce(transform.forward*1000, ForceMode.Impulse);
                hypertrail.ForEach(o => {o.gameObject.SetActive(true);o.Play();});
                yield return new WaitForSeconds(5);
                hyperspaces.Create(transform.position,Quaternion.identity);
                transform.position += transform.forward*100;
                rigidbody.AddForce(transform.forward*100000, ForceMode.Impulse);
                IsDisabled = false;
                Jump();
            }
        }

        void SevereDamage() {
            if (0>=parts.Count) return;
            var part = parts.Pop();
            part.Damage(1000000);
            if (part is Weapon blaster) blaster.Disable();
        }

        void OnDamage(float damage) {
            if (IsDead) return;
            Health -= damage;
            StartSemaphore(Damaging);
            IEnumerator Damaging() {
                if (!IsDisabled) audio.PlayOneShot(hitSounds.Pick(),1);
                if (damage>200) SevereDamage();
                if (Health<0) Kill();
                yield return new WaitForSeconds(0.1f);
            }
        }

        void OnKill() {
            if (IsDead) return; IsDead = true;
            if (Health<-2000) StartSemaphore(HaltAndCatchFire);
            else StartSemaphore(Killing);

            IEnumerator HaltAndCatchFire() {
                Disable(); Alarm();
                yield return new WaitForSeconds(2);
                while (0<parts.Count) yield return Wait(
                    wait: new WaitForSeconds(0.1f),
                    func: () => SevereDamage());
                StartCoroutine(Killing());
            }

            IEnumerator Killing() {
                Disable();
                while (0<parts.Count) SevereDamage();
                mechanics.ForEach(o => o.Disable());
                var instance = Create(explosion);
                instance.transform.parent = transform;
                yield return new WaitForSeconds(1);
                enabled = false;
                audio.Stop();
            }
        }

        void CaclulateAngles() {
            var flat = new Vector3(transform.forward.x,0,transform.forward.z);
            if (flat.sqrMagnitude<=0) return;
            flat.Normalize();
            var localFlatForward = transform.InverseTransformDirection(flat);
            pitchAngle = Mathf.Atan2(localFlatForward.y, localFlatForward.z);
            var plumb = Vector3.Cross(Vector3.up, flat);
            var localFlatRight = transform.InverseTransformDirection(plumb);
            rollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
        }

        void AutoLevel() {
            var (pitchTurn,pitchAuto,rollAuto,(roll,pitch,yaw)) = (0,0,0,Control);
            var turnBanking = Mathf.Sin(rollAngle);
            if (roll==0) Control = (-rollAngle*rollAuto, pitch, yaw);
            if (pitch==0) Control =
                (roll,
                -pitchAngle*pitchAuto-Mathf.Abs(Mathf.Pow(turnBanking,2)*pitchTurn),
                yaw);
        }

        (float,float) ControlThrottle() {
            if (IsDisabled) return (0,0);
            var throt = Shift*throttleEffect*Time.fixedDeltaTime;
            throt = Mathf.Clamp(Throttle+throt,0,1);
            return (throt,throt*EnginePower+BoostPower);
        }

        (float,float) CalculateDrag() {
            if (IsDisabled) return (0, rigidbody.angularDrag);
            var (drag, angularDrag) = (0f,0f);
            drag += rigidbody.velocity.magnitude*dragEffect*0.5f;
            drag *= (Brakes>0)?brakesEffect:1;
            angularDrag += Mathf.Max(300,rigidbody.velocity.magnitude)/TopSpeed;
            angularDrag *= (Brakes>0)?brakesEffect:1;
            angularDrag = Mathf.Max(4f,angularDrag);
            return (drag,angularDrag);
        }

        float ComputeCoefficient() =>
            Mathf.Pow(Vector3.Dot(transform.forward,rigidbody.velocity.normalized),2);

        (Vector3,Quaternion) CalculateAerodynamics(float aeroFactor=1) {
            if (rigidbody.velocity.magnitude<=0)
                return (rigidbody.velocity, rigidbody.rotation);
            var velocity = Vector3.Lerp(
                rigidbody.velocity, transform.forward*ForwardSpeed,
                aeroFactor*ForwardSpeed*AeroEffect*Time.fixedDeltaTime);
            var rotation = Quaternion.identity;
            if (0.1f<rigidbody.velocity.sqrMagnitude)
                rotation = Quaternion.Slerp(
                    rigidbody.rotation,
                    Quaternion.LookRotation(rigidbody.velocity,transform.up),
                    aeroFactor*AeroEffect*Time.fixedDeltaTime);
            return (velocity, rotation);
        }

        (float,float,float) CalculateLift(float aeroFactor=1) {
            var (Lift, zeroLiftSpeed) = (0,0); // (0.002f,300)
            var forces = CurrentPower*transform.forward;
            var liftDirection = Vector3.Cross(rigidbody.velocity,transform.right);
            var zeroLift = Lift*Mathf.InverseLerp(zeroLiftSpeed,0,ForwardSpeed);
            var liftPower = ForwardSpeed*ForwardSpeed*zeroLift*aeroFactor;
            forces += liftPower*liftDirection.normalized;
            return (forces.x, forces.y, forces.z);
        }

        (float,float,float) CalculateForce() {
            var velocity = rigidbody.velocity;
            var factor = velocity.magnitude/2;
            var forces = transform.forward*CurrentPower;
            forces -= Brakes*velocity.normalized*factor*brakesEffect;
            return (forces.x, forces.y, forces.z);
        }

        (float,float,float) CalculateTorque(float aeroFactor=1) {
            var torque = Vector3.zero;
            torque += Control.pitch*pitchEffect*transform.right;
            torque += Control.yaw*yawEffect*transform.up;
            torque -= Control.roll*rollEffect*transform.forward;
            // torque += bankedTurnEffect*turnBanking*transform.up
            torque *= Mathf.Clamp(ForwardSpeed,0,TopSpeed)*aeroFactor;
            torque *= Mathf.Max(1,rigidbody.angularDrag);
            return (torque.x, torque.y, torque.z);
        }

        void CalculateSpin(float spin, float threshold=0.5f) {
            if (Mathf.Abs(spin)<=threshold) return;
            Throttle = 0;
            var direction = (0<spin)?rigidbody.velocity:-rigidbody.velocity;
            if (0.1f<direction.sqrMagnitude)
                rigidbody.rotation = Quaternion.Slerp(
                    rigidbody.rotation,
                    Quaternion.LookRotation(direction,Vector3.up),
                    spinEffect*Time.fixedDeltaTime);
        }

        void CalculateManeuverThrust(float threshold=0.1f, float wingspan=4) {
            ManeuverRoll(); ManeuverPitch(); ManeuverYaw();

            void ManeuverRoll() {
                if (Control.roll>threshold) ApplyBalancedForce(
                    force: -transform.up*maneuveringEnergy*Control.roll,
                    displacement: transform.right*wingspan);
                if (Control.roll<-threshold) ApplyBalancedForce(
                    force: transform.up*maneuveringEnergy*Control.roll,
                    displacement: -transform.right*wingspan);
            }

            void ManeuverPitch() {
                if (Control.pitch>threshold) ApplyBalancedForce(
                    force: transform.forward*maneuveringEnergy*Control.pitch,
                    displacement: transform.up*wingspan);
                if (Control.pitch<-threshold) ApplyBalancedForce(
                    force: -transform.forward*maneuveringEnergy*Control.pitch,
                    displacement: -transform.up*wingspan);
            }

            void ManeuverYaw() {
                if (Control.yaw>threshold) ApplyBalancedForce(
                    force: transform.right*maneuveringEnergy*Control.yaw,
                    displacement: transform.forward*wingspan);
                if (Control.yaw<-threshold) ApplyBalancedForce(
                    force: -transform.right*maneuveringEnergy*Control.yaw,
                    displacement: -transform.forward*wingspan);
            }
        }

        void ApplyBalancedForce(Vector3 force, Vector3 displacement) {
            rigidbody.AddForceAtPosition(force,transform.position+displacement);
            rigidbody.AddForceAtPosition(-force,transform.position-displacement);
        }
    }
}
