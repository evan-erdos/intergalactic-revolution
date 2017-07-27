/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Adventure.Astronautics;

namespace Adventure.Astronautics.Spaceships {
    public class Spaceship : Adventure.Object, ISpaceship, ICreatable<ShipProfile> {
        float Shift, rollAngle, pitchAngle;
        float energyJumpFactor = 9, energyLoss=50, energyGain=20, maneuveringEnergy=100;
        float rollEffect=1, pitchEffect=1, yawEffect=0.2f;
        float brakesEffect=3, thrustEffect=0.5f, linearEffect=800, dragEffect=0.001f;
        Pool<Transform> hyperspaces = new Pool<Transform>();
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
        List<FlightMode> modes = new List<FlightMode> { FlightMode.Manual, FlightMode.Assisted };
        [SerializeField] protected ShipProfile profile;
        [SerializeField] List<Weapon> blasters = new List<Weapon>();
        [SerializeField] List<Weapon> rockets = new List<Weapon>();
        [SerializeField] protected Event<RealityArgs> onKill = new Event<RealityArgs>();
        [SerializeField] protected Event<FlightArgs> onMove = new Event<FlightArgs>();
        [SerializeField] protected Event<TravelArgs> onJump = new Event<TravelArgs>();
        [SerializeField] protected Event<CombatArgs> onHit = new Event<CombatArgs>();
        [SerializeField] protected Event<AttackArgs> onFire = new Event<AttackArgs>();
        public event AdventureAction<RealityArgs> KillEvent;
        public event AdventureAction<FlightArgs> MoveEvent;
        public event AdventureAction<TravelArgs> JumpEvent;
        public event AdventureAction<CombatArgs> HitEvent;
        public event AdventureAction<AttackArgs> FireEvent;
        public FlightMode Mode {get;protected set;} = FlightMode.Assisted;
        public bool IsDisabled {get;protected set;} = false;
        public bool IsDead {get;protected set;} = false;
        public int CargoSpace {get;protected set;} = 20; // tons
        public float Mass {get;protected set;} = 40; // tons
        public float Throttle {get;protected set;} = 0; // [0...1]
        public float EnginePower {get;protected set;} = 800; // kN
        public float CurrentPower {get;protected set;} = 800; // kN
        public float AeroEffect {get;protected set;} = 2; // drag coeff
        public float TopSpeed {get;protected set;} = 1500; // m/s
        public float Health {get;protected set;} = 12000; // kN
        public float MaxHealth {get;protected set;} = 12000; // kN
        public float EnergyCapacity {get;protected set;} = 8000; // kN/L
        public float EnergyThrust {get;protected set;} = 6000; // kN
        public float ForwardSpeed {get;protected set;} = 0; // m/s
        public float Speed => rigidbody.velocity.magnitude; // m/s
        public float EnergyJump => energyJumpFactor*Mass;
        public float EnergyPotential => (Thrust>0)?-energyLoss:energyGain;
        public float ThrustPower => (Energy>1 && Thrust>0.1f)?EnergyThrust:0;
        public float Energy {
            get { return Mathf.Clamp(energy,0,EnergyCapacity); }
            set { energy = value; } } float energy = 8000;
        public List<Vector3> Pivots {get;protected set;}
        public float Thrust {get;protected set;}
        public float Lift {get;protected set;}
        public float Strafe {get;protected set;}
        public (float roll, float pitch, float yaw) Control {get;protected set;}
        public Vector3 Velocity => rigidbody.velocity;
        public IWeapon Weapon {get;protected set;}
        public ITrackable Target {get;protected set;}
        public StarSystem CurrentSystem {get;protected set;}
        public SpobProfile Destination {get;protected set;}
        public SpobProfile[] Spobs {get;set;} = new SpobProfile[1];
        public void Damage(float damage) => HitEvent(new CombatArgs { Sender = this, Damage = damage });
        public void Kill(RealityArgs e=null) => KillEvent(e ?? new RealityArgs { Sender = this });
        public void Jump(TravelArgs e=null) => JumpEvent(e ?? new TravelArgs { Sender = this, Destination = Destination });

        public void Disable() => (IsDisabled, rigidbody.drag, rigidbody.angularDrag) = (true,0,0);
        public void Reset() => (IsDisabled, IsDead, rigidbody.mass, Health, Energy) = (false,false,Mass,MaxHealth,EnergyCapacity);
        public void Alarm() => audio.Play();

        public void Create(ShipProfile o) =>
            (name, Mass, Health, EnginePower, rollEffect, pitchEffect, yawEffect,
            brakesEffect, thrustEffect, AeroEffect, dragEffect, energyLoss, energyGain,
            EnergyThrust, EnergyCapacity, TopSpeed, maneuveringEnergy, Pivots, hitSounds,
            modeClip, changeClip, selectClip, hyperspaceClip, alarmClip, explosion, hyperspace) =
                (o.Name, o.Mass, o.Health, o.EnginePower, o.RollEffect, o.PitchEffect,
                o.YawEffect, o.BrakesEffect, o.ThrottleEffect, o.AeroEffect, o.DragEffect,
                o.EnergyLoss, o.EnergyGain, o.EnergyThrust, o.EnergyCapacity, o.TopSpeed,
                o.ManeuveringEnergy, o.Pivots, o.hitSounds, o.modeClip, o.changeClip,
                o.selectClip, o.hyperspaceClip, o.alarmClip, o.explosion, o.hyperspace);

        int nextSystem = -1; // gross
        public void SelectSystem() { StartAsync(Hyperspacing);
            // StartSemaphore(Hyperspacing); IEnumerator Hyperspacing() {
            async Task Hyperspacing() {
                if (0>=Spobs.Length) return;
                Destination = Spobs[++nextSystem%Spobs.Length];
                audio.PlayOneShot(selectClip); await 0.15;
            }
        }

        List<ITrackable> trackables = new List<ITrackable>();
        int nextTarget = -1; // ick
        public void SelectTarget() { StartSemaphore(Selecting);
            IEnumerator Selecting() {
                if (0>=targets.Count) yield break;
                audio.PlayOneShot(selectClip);
                if (targets[++nextTarget%targets.Count] is ITrackable o) Target = o;
                // print($"Target: {Target?.Name ?? "None"}");
                yield return new WaitForSeconds(0.1f);
            }
        }

        IEnumerable<ITrackable> GetTargets() { while (true) foreach (var o in trackables) yield return o; }

        int nextCamera = -1;
        public void ToggleView() {
            StartSemaphore(Toggling);
            IEnumerator Toggling() {
                var pivot = Pivots[++nextCamera%Pivots.Count];
                PlayerCamera.Pivot = pivot;
                yield return new WaitForSeconds(0.1f);
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
                yield return new WaitForSeconds(0.5f);
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
                } yield return new WaitForSeconds(0.2f);
            }

            void ChangeDrag(float aeroEffect, float dragCoefficient=0.0002f) {
                StartSemaphore(ChangingDrag); StartAsync(ChangingAero);
                async Task ChangingAero() {
                    var (time, speed, smooth, max) = (0f,0f,0.125f,100f);
                    while (Mathf.Abs(AeroEffect-aeroEffect)>0.25f) {
                        await AsyncTools.ToFixedUpdate();
                        AeroEffect = Mathf.SmoothDamp(
                            current: AeroEffect, target: aeroEffect,
                            currentVelocity: ref speed, smoothTime: smooth,
                            maxSpeed: max, deltaTime: time+=Time.fixedDeltaTime/4);
                    }
                }

                IEnumerator ChangingDrag() {
                    var (time, speed, smooth, max) = (0f,0f,0.125f,100f);
                    while (dragEffect!=dragCoefficient) yield return Wait(new WaitForFixedUpdate(), () =>
                        dragEffect = Mathf.SmoothDamp(
                            current: dragEffect, target: dragCoefficient,
                            currentVelocity: ref speed, smoothTime: smooth,
                            maxSpeed: max, deltaTime: time+=Time.fixedDeltaTime/4));
                }
            }
        }

        void Awake() {
            (rigidbody, audio) = (Get<Rigidbody>(), GetOrAdd<AudioSource>());
            (audio.clip, audio.loop, audio.playOnAwake) = (alarmClip,true,false);
            mechanics = GetChildren<IShipComponent>();
            parts = new Stack<IDamageable>(GetChildren<IDamageable>());
            KillEvent += e => onKill?.Call(e); onKill.Add(e => OnKill());
            JumpEvent += e => onJump?.Call(e); onJump.Add(e => OnHyperJump());
            HitEvent += e => onHit?.Call(e); onHit.Add(e => OnHit(e));
            MoveEvent += e => onMove?.Call(e); // onMove.Add(OnMove());
            FireEvent += e => onFire?.Call(e); // onFire.Add(OnFire());
            Create(profile);
            var query =
                from particles in GetChildren<ParticleSystem>()
                where particles.name=="Hypertrail" select particles;
            hypertrail.Add(query);
            hypertrail.ForEach(o => o.gameObject.SetActive(false));
            Reset();
            if (hyperspace!=null) hyperspaces = new Pool<Transform>(2, () => {
                var instance = Create<Transform>(hyperspace);
                (instance.parent, instance.localPosition) = (transform, Vector3.zero);
                instance.gameObject.layer = gameObject.layer;
                instance.gameObject.SetActive(false);
                return instance; });
        }

        IEnumerator Start() {
            var (radius,results) = (100000, new Collider[32]);
            var mask = 1<<LayerMask.NameToLayer("NPC");
            blasters.ForEach(o => o.gameObject.SetActive(false));
            rockets.ForEach(o => o.gameObject.SetActive(false));
            SelectWeapon(); ChangeMode();
            while (true) {
                Physics.OverlapSphereNonAlloc(rigidbody.position,radius,results,mask);
                yield return null;
                foreach (var result in results)
                    if (result?.attachedRigidbody?.Get<ITrackable>() is ITrackable ship)
                        if (!targets.Contains(ship)) targets.Add(ship);
                yield return new WaitForSeconds(1);
            }
        }

        void FixedUpdate() => Energy += EnergyPotential*Time.fixedDeltaTime;
        void OnCollisionEnter(Collision c) => Damage(c.impulse.magnitude/4);

        int nextFire = -1; // on the way out
        public void Fire(AttackArgs e=null) {
            if (PreFire()) StartSemaphore(Firing);
            bool PreFire() => !IsDisabled && !IsDead && weapons.Count>0;
            IEnumerator Firing() {
                var blaster = weapons[(++nextFire%weapons.Count)];
                var args = new AttackArgs {
                    Sender = this, Target = Target, Displacement = Velocity,
                    Velocity = Target?.Velocity ?? Vector3.zero,
                    Position = Target?.Position ?? transform.forward };
                if (!(Target is null)) blaster.Target = Target;
                blaster?.Fire(e ?? args); FireEvent(e ?? args);
                yield return new WaitForSeconds(1f/(blaster.Rate*weapons.Count));
            }
        }

        public void Move(FlightArgs e=null) {
            if (IsDisabled || IsDead) return; // insert clever autopilot here
            if (e is null) e = new FlightArgs { Sender = this };
            Control = (Clamp(e.Roll), Clamp(e.Pitch), Clamp(e.Yaw));
            (Thrust, Lift, Strafe) = (Clamp(e.Thrust) + e.Turbo, Clamp(e.Lift), Clamp(e.Strafe));
            ForwardSpeed = Mathf.Max(0,transform.InverseTransformDirection(rigidbody.velocity).z);
            float Clamp(float o) => Mathf.Clamp(o,-1,1);

            switch (Mode) {
                case FlightMode.Manual: ManualFlight(); break;
                case FlightMode.Assisted: AssistedFlight(); break;
                case FlightMode.Navigation: NavigationFlight(); break;
                default: DefaultFlight(); break;
            } MoveEvent(e);

            void ManualFlight() {
                var strafe = Strafe; Strafe = Control.yaw; Control = (0, Control.pitch, Control.roll);
                (Throttle,CurrentPower,Shift) = (0,0,-1); ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                rigidbody.AddForce(CalculateThrust()*2);
                CalculateManeuverThrust(); CalculateLinearManuever();
            }

            void AssistedFlight() {
                (Throttle, CurrentPower) = ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                var aero = ComputeCoefficient();
                (rigidbody.velocity, rigidbody.rotation) = CalculateAerodynamics();
                rigidbody.AddForce(CalculateForce());
                rigidbody.AddTorque(CalculateTorque(aero).vect());
                CalculateManeuverThrust();
            }

            void NavigationFlight() {
                CaclulateAngles(); AutoLevel();
                (Throttle,CurrentPower) = ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                var aero = ComputeCoefficient();
                (rigidbody.velocity, rigidbody.rotation) = CalculateAerodynamics(aero);
                rigidbody.AddForce(CalculateForce());
                rigidbody.AddTorque(CalculateTorque(aero).vect());
                CalculateManeuverThrust();
                // HyperJump(Quaternion.LookRotation(Vector3.left),null);
            }

            void DefaultFlight() {
                CaclulateAngles(); AutoLevel();
                (Throttle,Shift) = ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                var aero = ComputeCoefficient();
                (rigidbody.velocity, rigidbody.rotation) = CalculateAerodynamics(aero);
                var liftForce = CalculateLift(0).vect();
                rigidbody.AddForce(liftForce+CalculateForce().vect());
                rigidbody.AddTorque(CalculateTorque(aero).vect());
                CalculateManeuverThrust();
            }

            void CalculateLinearManuever() {
                var power = linearEffect*maneuveringEnergy*Time.fixedDeltaTime;
                rigidbody.AddRelativeForce(Vector3.right*Strafe*power);
                rigidbody.AddRelativeForce(Vector3.up*Lift*power); }

            void CaclulateAngles() {
                var flat = new Vector3(transform.forward.x,0,transform.forward.z);
                if (flat.sqrMagnitude<=0) return; flat.Normalize();
                var localFlatForward = transform.InverseTransformDirection(flat);
                pitchAngle = Mathf.Atan2(localFlatForward.y, localFlatForward.z);
                var plumb = Vector3.Cross(Vector3.up, flat);
                var localFlatRight = transform.InverseTransformDirection(plumb);
                rollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
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
                var (velocity, force) = (rigidbody.velocity, Vector3.zero);
                if (0<=Thrust) force += transform.forward*CurrentPower;
                else force += Thrust*velocity.normalized*(velocity.magnitude/2)*brakesEffect;
                return (force.x, force.y, force.z);
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

        }

        Vector3 CalculateThrust() {
            var factor = rigidbody.velocity.normalized*rigidbody.velocity.magnitude/2;
            return (0<=Thrust)?transform.forward*ThrustPower:Thrust*factor*brakesEffect; }

        void OnHyperJump() => hypertrail.ForEach(o => o.Stop());

        public void Dodge(float horizontal, float vertical, float power=1000) {
            if (Energy>=EnergyJump/4) StartSemaphore(Dodging);
            IEnumerator Dodging() {
                Energy -= EnergyJump/4;
                rigidbody.AddForce(transform.up*vertical*power+transform.right*power, ForceMode.VelocityChange);
                yield return new WaitForSeconds(0.5f);
            }
        }

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
            if (VerifyJump()) StartSemaphore(Jumping);
            bool VerifyJump() => !(Destination is null) && Energy>=EnergyJump/2;
            IEnumerator Jumping() {
                IsDisabled = true;
                while (--Throttle>0) yield return new WaitForSeconds(0.1f);
                (Throttle, Shift) = (0,0);
                (rigidbody.drag, rigidbody.angularDrag) = (10,10);
                while (Mathf.Abs(Quaternion.Dot(transform.rotation,direction))<0.999f)
                    yield return Wait(new WaitForFixedUpdate(), () =>
                        rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, direction, Time.fixedDeltaTime));
                audio.PlayOneShot(hyperspaceClip);
                Throttle = 20; ControlThrottle();
                rigidbody.AddForce(transform.forward*1000, ForceMode.Impulse);
                hypertrail.ForEach(o => { o.gameObject.SetActive(true); o.Play(); });
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

        void OnHit(CombatArgs e) {
            if (IsDead) return;
            Health -= e.Damage;
            StartSemaphore(Damaging);
            IEnumerator Damaging() {
                if (!IsDisabled) audio.PlayOneShot(hitSounds.Pick(),1);
                if (e.Damage>200) SevereDamage();
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
                    new WaitForSeconds(0.1f), () => SevereDamage());
                StartCoroutine(Killing());
            }

            IEnumerator Killing() {
                Disable();
                while (0<parts.Count) SevereDamage();
                mechanics.ForEach(o => o.Disable());
                var instance = Create(explosion, transform.position, transform.rotation);
                instance.transform.parent = transform; // or null?
                yield return new WaitForSeconds(1);
                enabled = false;
                audio.Stop();
            }
        }

        void AutoLevel() {
            var (pitchTurn,pitchAuto,rollAuto,(roll,pitch,yaw)) = (0,0,0,Control);
            var turnBanking = Mathf.Sin(rollAngle);
            if (roll==0) Control = (-rollAngle*rollAuto, pitch, yaw);
            if (pitch==0) Control =
                (roll, -pitchAngle*pitchAuto-Mathf.Abs(Mathf.Pow(turnBanking,2)*pitchTurn), yaw);
        }

        (float,float) ControlThrottle() {
            if (IsDisabled) return (0,0);
            var throt = Shift*thrustEffect*Time.fixedDeltaTime;
            throt = Mathf.Clamp(Throttle+throt,0,1);
            return (throt,throt*EnginePower+ThrustPower);
        }

        (float,float) CalculateDrag() {
            if (IsDisabled) return (0, rigidbody.angularDrag);
            var (drag, angularDrag) = (0f,0f);
            drag += rigidbody.velocity.magnitude*dragEffect*0.5f;
            drag *= (Thrust<-0.1)?brakesEffect:1;
            angularDrag += Mathf.Max(300, rigidbody.velocity.magnitude)/TopSpeed;
            angularDrag = Mathf.Max(4, angularDrag * ((Thrust<-0.1)?brakesEffect:1));
            return (drag,angularDrag);
        }

        float ComputeCoefficient() => Mathf.Pow(Vector3.Dot(transform.forward,rigidbody.velocity.normalized),2);

        (Vector3,Quaternion) CalculateAerodynamics(float aeroFactor=1) {
            if (rigidbody.velocity.magnitude<=0) return (rigidbody.velocity, rigidbody.rotation);
            var velocity = Vector3.Lerp(
                rigidbody.velocity, transform.forward*ForwardSpeed,
                aeroFactor*ForwardSpeed*AeroEffect*Time.fixedDeltaTime);
            var rotation = Quaternion.identity;
            if (0.1f<rigidbody.velocity.sqrMagnitude) rotation = Quaternion.Slerp(
                rigidbody.rotation, Quaternion.LookRotation(rigidbody.velocity,transform.up),
                aeroFactor*AeroEffect*Time.fixedDeltaTime);
            return (velocity, rotation);
        }

        void CalculateSpin(float spin, float threshold=0.5f) {
            if (Mathf.Abs(spin)<=threshold) return;
            var spinEffect = 0;
            Throttle = 0;
            var direction = (0<spin)?rigidbody.velocity:-rigidbody.velocity;
            if (0.1f<direction.sqrMagnitude) rigidbody.rotation = Quaternion.Slerp(
                rigidbody.rotation, Quaternion.LookRotation(direction,Vector3.up),
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
