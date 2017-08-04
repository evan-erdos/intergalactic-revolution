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
        float Shift, Spin, rollAngle, pitchAngle, energy = 8000, SpinEnergy = 200;
        float energyJumpFactor = 9, energyLoss=50, energyGain=20, maneuveringEnergy=100;
        float rollEffect=1, pitchEffect=1, yawEffect=0.2f, dragEffect=0.001f;
        float brakesEffect=3, thrustEffect=0.5f, linearEnergy=800, linearEnergyMax=3000;
        Animator animator; new AudioSource audio; new Rigidbody rigidbody;
        ParticleSystem particles;
        AudioClip modeClip, changeClip, selectClip, hyperspaceClip, alarmClip;
        GameObject explosionPrefab, hyperspacePrefab, thrusterPrefab, motesOfLightPrefab;
        Pool<Transform> hyperspaces = new Pool<Transform>();
        List<AudioClip> hitSounds = new List<AudioClip>();
        List<AudioClip> shieldSounds = new List<AudioClip>();
        Set<Renderer> degradable = new Set<Renderer>();
        Set<Renderer> destroyable = new Set<Renderer>();
        Stack<IDamageable> parts = new Stack<IDamageable>();
        List<IWeapon> weapons = new List<IWeapon>();
        List<IShield> shields = new List<IShield>();
        List<ITrackable> targets = new List<ITrackable>();
        List<IShipPart> mechanics = new List<IShipPart>();
        List<IThruster> thrusters = new List<IThruster>();
        List<ParticleSystem> hypertrail = new List<ParticleSystem>();
        Map<Type,List<IWeapon>> blasters = new Map<Type,List<IWeapon>>();
        List<FlightMode> modes = new List<FlightMode> { FlightMode.Manual };
        [SerializeField] protected ShipProfile profile;
        [SerializeField] protected ObjectEvent onKill = new ObjectEvent();
        [SerializeField] protected FlightEvent onMove = new FlightEvent();
        [SerializeField] protected TravelEvent onJump = new TravelEvent();
        [SerializeField] protected CombatEvent onHit = new CombatEvent();
        [SerializeField] protected CombatEvent onFire = new CombatEvent();
        public event AdventureAction<ObjectArgs> KillEvent;
        public event AdventureAction<FlightArgs> MoveEvent;
        public event AdventureAction<TravelArgs> JumpEvent;
        public event AdventureAction<CombatArgs> HitEvent, FireEvent;
        public FlightMode Mode {get;protected set;} = FlightMode.Manual;
        public bool IsAlive {get;protected set;} = true;
        public bool IsEnabled {get;protected set;} = true;
        public int CargoSpace {get;protected set;} = 20; // tons
        public float Mass {get;protected set;} = 40; // tons
        public float Throttle {get;protected set;} = 0; // [0...1]
        public float EnginePower {get;protected set;} = 800; // kN
        public float CurrentPower {get;protected set;} = 800; // kN
        public float AeroEffect {get;protected set;} = 2; // drag coeff
        public float TopSpeed {get;protected set;} = 1500; // m/s
        public float Shield {get;protected set;} = 1000; // kN
        public float MaxShield {get;protected set;} = 1000; // kN
        public float Health {get;protected set;} = 8000; // kN
        public float MaxHealth {get;protected set;} = 8000; // kN
        public float MaxEnergy {get;protected set;} = 8000; // kN/L
        public float EnergyThrust {get;protected set;} = 6000; // kN
        public float ForwardSpeed {get;protected set;} = 0; // m/s
        public float Rate => CurrentWeapon?.Rate ?? 0; // hZ
        public float Speed => rigidbody.velocity.magnitude; // m/s
        public float EnergyJump => energyJumpFactor*Mass;
        public float EnergyPotential => (Thrust>0)?-energyLoss:energyGain;
        public float ThrustPower => (Energy>1 && Thrust>0.1f)?EnergyThrust:0;
        public float Energy { get { return Mathf.Clamp(energy,0,MaxEnergy); } set { energy=value; } }
        public List<Vector3> Pivots {get;protected set;}
        public float Thrust {get;protected set;}
        public float Lift {get;protected set;}
        public float Strafe {get;protected set;}
        public (float roll, float pitch, float yaw) Control {get;protected set;}
        public Vector3 Velocity => rigidbody.velocity;
        public ITrackable Target {get;protected set;}
        public IWeapon CurrentWeapon {get;protected set;}
        public StarSystem CurrentSystem {get;protected set;}
        public SpobProfile Destination {get;protected set;}
        public SpobProfile[] Spobs {get;set;} = new SpobProfile[1];
        public void Hit(float damage=0) => HitEvent(new CombatArgs { Sender=this, Damage=damage });
        public void Hit(CombatArgs e=null) => HitEvent(e ?? new CombatArgs { Sender=this, Target=this });
        public void Kill(ObjectArgs e=null) => KillEvent(e ?? new ObjectArgs { Sender=this, Target=this });
        public void Jump(TravelArgs e=null) => JumpEvent(e ?? new TravelArgs { Sender=this, Destination=Destination });

        public void Disable() => (IsEnabled, rigidbody.drag, rigidbody.angularDrag) = (false,0,0);
        public void Reset() => (IsEnabled, IsAlive, rigidbody.mass, Health, Energy) = (true,true,Mass,MaxHealth,MaxEnergy);
        public void Alarm() => audio.Play();

        public void Create(ShipProfile o) {
            (name, Mass, MaxHealth, EnginePower) = (o.Name, o.Mass, o.Health, o.EnginePower);
            (rollEffect, pitchEffect, yawEffect) = (o.RollEffect, o.PitchEffect, o.YawEffect);
            (brakesEffect, thrustEffect, AeroEffect) = (o.BrakesEffect, o.ThrottleEffect, o.AeroEffect);
            (energyLoss, energyGain, MaxEnergy) = (o.EnergyLoss, o.EnergyGain, o.EnergyCapacity);
            (dragEffect, EnergyThrust, Pivots) = (o.DragEffect, o.EnergyThrust, o.Pivots);
            (TopSpeed, maneuveringEnergy, linearEnergy) = (o.TopSpeed, o.ManeuveringEnergy, o.linearEnergy);
            (hitSounds, shieldSounds, modeClip, changeClip) = (o.hitSounds, shieldSounds, o.modeClip, o.changeClip);
            (selectClip, hyperspaceClip, alarmClip) = (o.selectClip, o.hyperspaceClip, o.alarmClip);
            (explosionPrefab, hyperspacePrefab, thrusterPrefab, motesOfLightPrefab) = (o.explosion, o.hyperspace, o.thruster, o.motesOfLight);
            particles = Create<ParticleSystem>(motesOfLightPrefab);
            foreach (var i in GetChildren<ThrusterSocket>()) {
                var t = Create<IThruster>(thrusterPrefab); t.SetShip(this); thrusters.Add(t); i.SetTarget(t); }
        }

        int nextSystem = -1; // gross
        public void SelectSystem() { StartAsync(Hyperspacing);
            async Task Hyperspacing() {
                if (0>=Spobs.Length) return;
                Destination = Spobs[++nextSystem%Spobs.Length];
                audio.PlayOneShot(selectClip); await 0.15;
            }
        }

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

        int nextCamera = -1;
        public void ToggleView() { if (IsEnabled) StartSemaphore(Toggling);
            IEnumerator Toggling() {
                PlayerCamera.Pivot = Pivots[++nextCamera%Pivots.Count];
                yield return new WaitForSeconds(0.4f);
            }
        }

        int nextWeapon = -1;
        public void SelectWeapon() { if (IsEnabled) StartSemaphore(Selecting);
            IEnumerator Selecting() {
                weapons.ForEach(o => o.Location.gameObject.SetActive(false));
                weapons.Clear();
                switch ((nextWeapon=(1+nextWeapon)%blasters.Keys.Count)) {
                    case 0: weapons.Add(blasters[typeof(IWeapon)].Cast<IWeapon>()); break;
                    default: weapons.Add(blasters[typeof(IWeapon)].Cast<IWeapon>()); break; }
                nextFire = 0;
                weapons.ForEach(o => o.Location.gameObject.SetActive(true));
                audio.PlayOneShot(changeClip);
                if (0<weapons.Count) CurrentWeapon = weapons.First();
                yield return new WaitForSeconds(0.5f);
            }
        }


        int nextMode = 0; // on the way out
        public void ChangeMode() { if (IsEnabled) StartSemaphore(ChangingMode);
            IEnumerator ChangingMode() {
                audio.PlayOneShot(modeClip);
                Mode = modes[++nextMode%modes.Count];
                switch (Mode) {
                    case FlightMode.Manual: ChangeDrag(0,0); break;
                    case FlightMode.Assisted: ChangeDrag(AeroEffect); break;
                    case FlightMode.Navigation: ChangeDrag(AeroEffect,0); break;
                } yield return new WaitForSeconds(0.2f);
            }
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
                        maxSpeed: max, deltaTime: time+=Time.fixedDeltaTime/4f);
                }
            }

            IEnumerator ChangingDrag() {
                var (time, speed, smooth, max) = (0f,0f,0.125f,100f);
                while (dragEffect!=dragCoefficient) {
                    yield return new WaitForFixedUpdate();
                    dragEffect = Mathf.SmoothDamp(
                        current: dragEffect, target: dragCoefficient,
                        currentVelocity: ref speed, smoothTime: smooth,
                        maxSpeed: max, deltaTime: time+=Time.fixedDeltaTime/4f);
                }
            }
        }

        void Awake() {
            (animator, rigidbody, audio) = (Get<Animator>(), Get<Rigidbody>(), GetOrAdd<AudioSource>());
            (audio.clip, audio.loop, audio.playOnAwake) = (alarmClip,true,false);
            (mechanics, shields) = (GetChildren<IShipPart>(), GetChildren<IShield>());
            parts = new Stack<IDamageable>(GetChildren<IDamageable>().Where(o => !(o is IShield)));
            blasters[typeof(IWeapon)] = GetChildren<IWeapon>().Where(o => !(o is ISpaceship)).ToList();
            foreach (var a in blasters) foreach (var b in a.Value) b.Location.gameObject.SetActive(false);
            foreach (var r in GetChildren<Renderer>())
                if (r.CompareTag("Destroyed")) destroyable.Add(r); else degradable.Add(r);
            KillEvent += e => onKill?.Call(e); onKill.Add(e => OnKill());
            JumpEvent += e => onJump?.Call(e); onJump.Add(e => OnHyperJump());
            HitEvent += e => onHit?.Call(e); onHit.Add(e => OnHit(e));
            MoveEvent += e => onMove?.Call(e); // onMove.Add(e => OnMove());
            FireEvent += e => onFire?.Call(e); onFire.Add(e => OnFire(e));
            Create(profile);
            var query =
                from particles in GetChildren<ParticleSystem>()
                where particles.name=="Hypertrail" select particles;
            hypertrail.Add(query);
            hypertrail.ForEach(o => o.gameObject.SetActive(false));
            Reset();
            if (hyperspacePrefab!=null) hyperspaces = new Pool<Transform>(2, () => {
                var instance = Create<Transform>(hyperspacePrefab);
                (instance.parent, instance.localPosition) = (transform, Vector3.zero);
                instance.gameObject.layer = gameObject.layer;
                instance.gameObject.SetActive(false); return instance; });
        }

        IEnumerator Start() {
            var (radius,results) = (100000, new Collider[32]);
            var mask = 1<<LayerMask.NameToLayer("NPC");
            yield return new WaitForSeconds(0.1f);
            SelectWeapon(); ChangeMode(); ToggleView();
            while (true) {
                Physics.OverlapSphereNonAlloc(rigidbody.position,radius,results,mask);
                yield return null;
                foreach (var result in results)
                    if (result?.attachedRigidbody?.Get<ITrackable>() is ITrackable ship)
                        if (!targets.Contains(ship)) targets.Add(ship);
                yield return new WaitForSeconds(1);
            }
        }

        void FixedUpdate() {
            if (Shield<MaxShield) Shield += EnergyPotential*Time.fixedDeltaTime;
            else Energy += EnergyPotential*Time.fixedDeltaTime; }

        void OnCollisionEnter(Collision c) => Hit(new CombatArgs { Sender=this,
            Target=c.contacts.First().thisCollider?.Get<IShield>(), Damage=c.impulse.magnitude/4f });

        int nextFire = -1; // on the way out
        public void Fire(CombatArgs e=null) {
            if (PreFire()) StartSemaphore(Firing);
            bool PreFire() => IsAlive && IsEnabled && weapons.Count>0;
            IEnumerator Firing() {
                CurrentWeapon = weapons[(++nextFire%weapons.Count)];
                FireEvent(e ?? new CombatArgs {
                    Sender=this, Target=Target, Position=Target?.Position ?? transform.forward,
                    Displacement=Velocity, Velocity=Target?.Velocity ?? Vector3.zero });
                yield return new WaitForSeconds(1f/(CurrentWeapon.Rate*weapons.Count));
            }
        }

        void OnFire(CombatArgs e) => CurrentWeapon?.Fire(e);

        public void Move(FlightArgs e=null) {
            if (!IsAlive || !IsEnabled) return; // insert clever autopilot here
            if (e is null) e = new FlightArgs { Sender=this };
            Control = (Clamp(e.Roll), Clamp(e.Pitch), Clamp(e.Yaw)); Spin = Clamp(e.Spin);
            (Thrust, Lift, Strafe) = (Clamp(e.Thrust), Clamp(e.Lift), Clamp(e.Strafe));
            // if (e.Spin>0) ChangeDrag(AeroEffect); else ChangeDrag(0,0);
            ForwardSpeed = Mathf.Max(0,transform.InverseTransformDirection(rigidbody.velocity).z);
            var aero = Mathf.Pow(Vector3.Dot(transform.forward,rigidbody.velocity.normalized),2);

            switch (Mode) {
                case FlightMode.Manual: ManualFlight(); break;
                case FlightMode.Assisted: AssistedFlight(); break;
                case FlightMode.Navigation: NavigationFlight(); break;
                default: DefaultFlight(); break;
            } MoveEvent(e);

            float Clamp(float o) => Mathf.Clamp(o,-1,1);

            void ManualFlight() {
                (Throttle,CurrentPower,Shift) = (0,0,-1); ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                rigidbody.AddForce(CalculateThrust());
                CalculateAlign(Spin);
                CalculateManeuverThrust(); CalculateLinearManuever();
            }

            void AssistedFlight() {
                (Throttle, CurrentPower) = ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
                (rigidbody.velocity, rigidbody.rotation) = CalculateAerodynamics();
                rigidbody.AddForce(CalculateForce());
                rigidbody.AddTorque(CalculateTorque(aero).vect());
                CalculateManeuverThrust();
            }

            void NavigationFlight() {
                CaclulateAngles(); AutoLevel();
                (Throttle,CurrentPower) = ControlThrottle();
                (rigidbody.drag, rigidbody.angularDrag) = CalculateDrag();
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
                (rigidbody.velocity, rigidbody.rotation) = CalculateAerodynamics(aero);
                var liftForce = CalculateLift(0);
                rigidbody.AddForce(liftForce+CalculateForce().vect());
                rigidbody.AddTorque(CalculateTorque(aero).vect());
                CalculateManeuverThrust();
            }
        }


        void CaclulateAngles() {
            var flat = new Vector3(transform.forward.x,0,transform.forward.z);
            if (flat.sqrMagnitude<=0) return; flat.Normalize();
            var localFlatForward = transform.InverseTransformDirection(flat);
            pitchAngle = Mathf.Atan2(localFlatForward.y, localFlatForward.z);
            var plumb = Vector3.Cross(Vector3.up, flat);
            var localFlatRight = transform.InverseTransformDirection(plumb);
            rollAngle = Mathf.Atan2(localFlatRight.y, localFlatRight.x);
        }

        Vector3 CalculateLift(float aeroFactor=1) {
            var (Lift, zeroLiftSpeed) = (0,0); // (0.002f,300)
            var forces = CurrentPower*transform.forward;
            var liftDirection = Vector3.Cross(rigidbody.velocity,transform.right);
            var zeroLift = Lift*Mathf.InverseLerp(zeroLiftSpeed,0,ForwardSpeed);
            var liftPower = ForwardSpeed*ForwardSpeed*zeroLift*aeroFactor;
            return forces + liftPower*liftDirection.normalized;
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

        Vector3 CalculateThrust() {
            var factor = rigidbody.velocity.normalized*rigidbody.velocity.magnitude;
            var overdrive = Mathf.Clamp(Speed-TopSpeed,0,TopSpeed);
            var limit = ThrustPower*(TopSpeed/(TopSpeed+overdrive*overdrive));
            var force = limit + ThrustPower*(1-Vector3.Dot(Velocity.normalized,transform.forward));
            return (0<=Thrust)?transform.forward*force:Thrust*factor*brakesEffect; }

        void CalculateLinearManuever() {
            var power = linearEnergy*maneuveringEnergy*Time.fixedDeltaTime;
            power += Mathf.Min(linearEnergyMax, rigidbody.velocity.magnitude)/TopSpeed;
            // var force = (Vector3.right*Strafe+Vector3.up*Lift)*power;
            // if (linearEnergyMax<Vector3.Dot(rigidbody.velocity, force)) rigidbody.AddRelativeForce(force);
            rigidbody.AddRelativeForce(Vector3.right*Strafe*power+Vector3.up*Lift*power);
        }


        void AutoLevel() {
            var (pitchTurn,pitchAuto,rollAuto,(roll,pitch,yaw)) = (0,0,0,Control);
            var turnBanking = Mathf.Sin(rollAngle);
            if (roll==0) Control = (-rollAngle*rollAuto, pitch, yaw);
            if (pitch==0) Control = (roll, -pitchAngle*pitchAuto-Mathf.Abs(Mathf.Pow(turnBanking,2)*pitchTurn), yaw);
        }

        (float,float) ControlThrottle() {
            if (!IsEnabled) return (0,0);
            var throt = Shift*thrustEffect*Time.fixedDeltaTime;
            throt = Mathf.Clamp(Throttle+throt,0,1);
            return (throt,throt*EnginePower+ThrustPower);
        }

        (float,float) CalculateDrag() {
            if (!IsEnabled) return (0, rigidbody.angularDrag);
            var drag = rigidbody.velocity.sqrMagnitude*dragEffect*((Thrust<-0.1)?brakesEffect:1);
            var angularDrag = Mathf.Max(10, Mathf.Max(300, rigidbody.velocity.magnitude)/TopSpeed);
            return (drag,angularDrag);
        }

        (Vector3,Quaternion) CalculateAerodynamics(float aeroFactor=1) {
            if (rigidbody.velocity.magnitude<=0) return (rigidbody.velocity, rigidbody.rotation);
            var velocity = Vector3.Lerp(
                rigidbody.velocity, transform.forward*ForwardSpeed,
                aeroFactor*ForwardSpeed*AeroEffect*Time.fixedDeltaTime);
            var rotation = Quaternion.identity;
            if (1<rigidbody.velocity.sqrMagnitude) rotation = Quaternion.Slerp(
                rigidbody.rotation, Quaternion.LookRotation(rigidbody.velocity,transform.up),
                aeroFactor*AeroEffect*Time.fixedDeltaTime);
            return (velocity, rotation);
        }

        void CalculateAlign(float align=0, float threshold=0.01f) =>
            rigidbody.AddForce(Speed*SpinEnergy*align*(transform.forward-Velocity.normalized));

        void CalculateSpin(float spin, float threshold=0.5f) {
            if (Mathf.Abs(spin)<=threshold) return;
            var spinEffect = 0; Throttle = 0;
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
                IsEnabled = false;
                while (--Throttle>0) yield return new WaitForSeconds(0.1f);
                (Throttle, Shift) = (0,0);
                (rigidbody.drag, rigidbody.angularDrag) = (10,10);
                while (Mathf.Abs(Quaternion.Dot(transform.rotation,direction))<0.999f) {
                    yield return new WaitForFixedUpdate();
                    rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, direction, Time.fixedDeltaTime);
                }
                audio.PlayOneShot(hyperspaceClip);
                Throttle = 20; ControlThrottle();
                rigidbody.AddForce(transform.forward*1000, ForceMode.Impulse);
                hypertrail.ForEach(o => { o.gameObject.SetActive(true); o.Play(); });
                yield return new WaitForSeconds(5);
                hyperspaces.Create(transform.position,Quaternion.identity);
                transform.position += transform.forward*100;
                rigidbody.AddForce(transform.forward*100000, ForceMode.Impulse);
                IsEnabled = true;
                Jump();
            }
        }


        void OnHit(CombatArgs e) {
            if (!IsAlive) return;
            if (e.Target is IShield s) { s.Hit(e.Damage); audio.PlayOneShot(shieldSounds.Pick(),0.8f); return; }
            // if (Shield>e.Damage) { Shield -= e.Damage; return; }
            Health -= e.Damage;
            StartSemaphore(Damaging);
            IEnumerator Damaging() {
                if (IsEnabled) audio.PlayOneShot(hitSounds.Pick(),1);
                if (e.Damage>500) SevereDamage(e.Damage);
                if (Health<0) Kill();
                yield return new WaitForSeconds(0.1f);
            }
        }

        void SevereDamage(float damage=100000) {
            if (0>=parts.Count) return;
            var part = parts.Pop();
            part.Hit(damage);
            if (part.IsAlive) { parts.Push(part); return; }
            if (part is IWeapon weapon) weapon.Disable();
            if (part is Adventure.Object o) o.transform.parent = null;
        }

        void DestroyPart() {
            if (0>=parts.Count) return;
            var part = parts.Pop();
            part.Hit(part.Health*2);
            if (part is IWeapon weapon) weapon.Disable();
            if (part is Adventure.Object o) o.transform.parent = null;
        }


        void OnKill() {
            if (!IsAlive) return; IsAlive = false;
            if (-400<Health) StartSemaphore(HaltAndCatchFire);
            else StartSemaphore(Detonating);

            IEnumerator HaltAndCatchFire() {
                Disable(); Alarm();
                while (0<parts.Count) { yield return null; DestroyPart(); }
                StartCoroutine(Killing());
            }

            IEnumerator Detonating() {
                destroyable.ForEach(o => { o.enabled = true; o.transform.parent = null; });
                degradable.ForEach(o => o.enabled = false);
                yield return StartCoroutine(Killing());
            }

            IEnumerator Killing() {
                Disable();
                while (0<parts.Count) DestroyPart();
                mechanics.ForEach(o => o.Disable());
                var instance = Create(explosionPrefab, transform.position, transform.rotation);
                instance.transform.parent = transform;
                yield return new WaitForSeconds(1);
                enabled = false; audio.Stop();
            }
        }
    }
}
