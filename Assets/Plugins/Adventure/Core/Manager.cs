using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

namespace Adventure {
    public class Manager : Adventure.Object {
        [SerializeField] protected AdventurePrefabs prefabs = new AdventurePrefabs();
        [SerializeField] protected AdventureProfiles profiles = new AdventureProfiles();
        [SerializeField] StarProfile[] StarProfiles = new StarProfile[1];
        [SerializeField] SpobProfile[] SpobProfiles = new SpobProfile[1];
        [SerializeField] PilotProfile[] PilotProfiles = new PilotProfile[1];
        [SerializeField] ShipProfile[] ShipProfiles = new ShipProfile[1];

        [Serializable] protected class AdventurePrefabs {
            [SerializeField] public GameObject menu;
            [SerializeField] public GameObject user;
            [SerializeField] public GameObject cam; }

        [Serializable] protected class AdventureProfiles {
            [SerializeField] public PilotProfile pilot;
            [SerializeField] public SpobProfile spob;
            [SerializeField] public StarProfile star; }

        public static readonly string root = "adventure", dir = "star-systems";
        public static string path {get;protected set;}
        public static Manager singleton {get;private set;}
        public static Settings settings {get;set;}
        public static NetworkManager network {get;protected set;}
        public static Player user {get;protected set;}
        public static Spaceship ship {get;protected set;}
        public static Menu menu {get;protected set;}
        public static PilotProfile DefaultPilot {get;protected set;}
        public static StarProfile DefaultStar {get;protected set;}
        public static SpobProfile DefaultSpob {get;protected set;}
        public static new PlayerCamera camera {get;protected set;}
        public static PilotProfile[] Pilots {get;protected set;}
        public static ShipProfile[] Ships {get;protected set;}
        public static List<NetworkStartPosition> StartPositions {get;protected set;} = new List<NetworkStartPosition>();
        public static Map<StarProfile,StarProfile[]> Stars {get;} = new Map<StarProfile,StarProfile[]>();
        public static readonly Map<Type> tags = new Map<Type> {
            ["object"] = typeof(Adventure.Object), ["star"] = typeof(StarSystem),
            ["settings"] = typeof(Adventure.Settings) };

        public static bool IsOnline => false;
        public static void StopHost() => network.StopHost();
        public static void StartHost() => network.StartHost();
        public static void StartServer() => network.StartServer();
        public static void StartClient() => network.StartClient();
        public static void StartMatchMaker() => network.StartMatchMaker();
        // public static void CreateMatch() => network.matchMaker.CreateMatch(
        //     network.matchName, network.matchSize, true, "", network.OnMatchCreate);
        // manager.matchMaker.JoinMatch(match.networkId, "", manager.OnMatchJoined);

        // public static void OnLoad() {
        //     var star = Create(starSystem.prefab);
        //     // var user = Create<SpacePlayer>(pilot.prefab);
        //     var pilot = PickUser();
        //     var user = Create<SpacePlayer>(pilot.prefab);
        //     var ship = Create<Spaceship>(pilot.ship.prefab);
        //     var scene = SceneManager.GetSceneByName(spob);
        //     DontDestroyOnLoad(ship.gameObject);
        //     // ship.Create();
        //     (star.name, user.name, user.Ship) = (starSystem.name, pilot.name, ship);
        //     SceneManager.MoveGameObjectToScene(star.gameObject,scene);
        //     SceneManager.MoveGameObjectToScene(user.gameObject,scene);
        //     SceneManager.SetActiveScene(SceneManager.GetSceneByName(spob));
        //     user.Reset();
        //     DontDestroyOnLoad(user.gameObject);
        //     PlayerCamera.atmosphere = starSystem.atmosphere;
        //     PlayerCamera.Target = ship.transform;
        //     var list = new List<NetworkStartPosition>();
        //     list.Add(FindObjectsOfType<NetworkStartPosition>());
        //     var spawn = list.Pick();
        //     ship.transform.position = spawn.transform.position;
        //     ship.transform.rotation = spawn.transform.rotation;
        //     user.SetShip(ship);
        //     Destroy(menu.gameObject);
        //     SceneManager.UnloadSceneAsync("Menu");
        // }

        // if (spawnPositions.Any()) {
        //     if (spawnMethod==PlayerSpawnMethod.Random)
        //         return spawnPositions[Random.Range(0, spawnPositions.Count)];
        //     if (spawnMethod==PlayerSpawnMethod.RoundRobin) {
        //         if (s_StartPositionIndex >= spawnPositions.Count) s_StartPositionIndex = 0;
        //         return spawnPositions[s_StartPositionIndex++];
        //     }
        // }

        public static void Jump(SpobProfile spob) {
            if (spob is null) spob = DefaultSpob;
            var scene = SceneManager.GetSceneByPath(spob.Name);
            var star = Create(spob.Star.prefab);
            if (!scene.IsValid()) scene = SceneManager.GetSceneByPath(DefaultSpob.Name);
            Fade.StartAlphaFade(new Color(1,1,1,0.7f),false,1,0);
            SceneManager.LoadSceneAsync(scene.name);
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            SceneManager.MoveGameObjectToScene(star.gameObject,scene);
            SceneManager.SetActiveScene(SceneManager.GetSceneByPath(spob.Name));
            // var list = new List<NetworkStartPosition>();
            // list.Add(FindObjectsOfType<NetworkStartPosition>());
            // var spawn = list.Pick();
            // ship.transform.position = spawn.transform.position;
            // ship.transform.rotation = spawn.transform.rotation;
            // user.SetShip(ship);
            PlayerCamera.atmosphere = spob.Star.atmosphere;
            // PlayerCamera.Target = ship.transform;
            NetworkServer.SpawnObjects();
            network.ServerChangeScene(scene.name);
        }


        // public static void LoadScene(string name) {
        //     Fade.StartAlphaFade(new Color(1,1,1,0.7f),false,1,0);
        //     var scene = SceneManager.GetSceneByPath(name);
        //     if (!scene.isValid) scene = SceneManager.GetSceneByPath(DefaultSpob.Name);
        //     SceneManager.LoadSceneAsync(scene);
        //     NetworkServer.SpawnObjects();
        // }

        void Awake() {
            path = $"{Application.streamingAssetsPath}/{root}/{dir}";
            if (singleton is null) singleton = this;
            else { Destroy(gameObject); return; }
            network = Get<NetworkManager>();
            DontDestroyOnLoad(gameObject);
            StarProfiles.ForEach(o => Stars[o] = o.NearbySystems);
            (Pilots, Ships) = (PilotProfiles, ShipProfiles);
            (DefaultPilot, DefaultSpob, DefaultStar) = (profiles.pilot, profiles.spob, profiles.star);
        }

        void Start() { camera = Create<PlayerCamera>(prefabs.cam); LoadMenu(); }

        public static void PrintInput() {
            foreach (KeyCode k in Enum.GetValues(typeof(KeyCode)))
                if (Input.GetKeyDown(k)) print(k); }

        public static void LoadMenu() {
            menu = Create<Menu>(singleton.prefabs.menu);
            Fade.StartAlphaFade(Color.black,true,1,0); }

        // public void CreateShip() => CmdCreateShip(shipPrefab);
        // [Command] public void CmdCreateShip(GameObject prefab) {
        //     var instance = Instantiate(prefab) as GameObject;
        //     NetworkServer.Spawn(instance);
        //     Ship = instance.Get<Spaceship>();
        //     Ship.Create();
        //     Ship.GetComponentsInChildren<IAdventure.Object>().ForEach(o=>o.Create());
        //     control.Ship = Ship;
        //     Ship.KillEvent += (o,e) => OnKill();
        //     Ship.JumpEvent += (o,e) => OnJump();
        //     if (isLocalPlayer) PlayerCamera.Target = Ship.transform;
        // }

        public void CreateShip() => CreateShip();

        public void CmdCreateShip(SpaceActor actor, ShipProfile ship) { // [Command]
            var instance = Instantiate(ship.prefab) as GameObject;
            NetworkServer.Spawn(instance);
            DontDestroyOnLoad(instance);
            actor.Ship = instance.Get<Spaceship>();
            actor.Ship.Init(); actor.Ship.Create();
            actor.Ship.GetChildren<Adventure.Object>().ForEach(o => { o.Init(); o.Create(); });
        }


        public static void OnLoad(ShipProfile shipData, StarProfile star, SpobProfile spob) {
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene()); StartHost();
            var sun = Create(star.prefab);
            var pilot = Pilots.ToList().Pick();
            var user = Create<SpacePlayer>(pilot.prefab);
            var ship = Create<Spaceship>(pilot.ship.prefab);
            var scene = SceneManager.GetSceneByName(spob.Name);
            DontDestroyOnLoad(ship.gameObject);
            (sun.name, user.name, user.Ship) = (star.name, pilot.name, ship);
            SceneManager.SetActiveScene(scene);
            SceneManager.MoveGameObjectToScene(sun.gameObject,scene);
            SceneManager.MoveGameObjectToScene(user.gameObject,scene);
            user.Reset();
            DontDestroyOnLoad(user.gameObject);
            PlayerCamera.atmosphere = star.atmosphere;
            PlayerCamera.Target = ship.transform;
            StartPositions = FindObjectsOfType<NetworkStartPosition>().ToList();
            var spawn = StartPositions.Pick();
            ship.JumpEvent += e => Manager.Jump(ship.Destination);
            ship.transform.position = spawn.transform.position;
            ship.transform.rotation = spawn.transform.rotation;
            user.SetShip(ship);
            Destroy(menu.gameObject);
        }


        public static void OnLoadGame(PilotProfile pilot, StarProfile star, SpobProfile spob) {
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene()); StartHost();
            var sun = Create(star.prefab);
            var user = Create<SpacePlayer>(pilot.prefab);
            var ship = Create<Spaceship>(pilot.ship.prefab);
            var scene = SceneManager.GetSceneByPath(spob.Name);
            DontDestroyOnLoad(user.gameObject);
            DontDestroyOnLoad(ship.gameObject);
            ship.Spobs = spob.Star.Spobs;
            (sun.name, user.name, user.Ship) = (star.name, pilot.name, ship);
            SceneManager.SetActiveScene(scene);
            // network.ServerChangeScene(SceneManager.GetActiveScene().name);
            SceneManager.MoveGameObjectToScene(sun.gameObject,scene);
            PlayerCamera.atmosphere = star.atmosphere;
            PlayerCamera.Target = ship.transform;
            Manager.ship = ship;
            StartPositions = FindObjectsOfType<NetworkStartPosition>().ToList();
            var spawn = StartPositions.Pick();
            ship.transform.position = spawn.transform.position;
            ship.transform.rotation = spawn.transform.rotation;
            user.SetShip(ship);
            Destroy(menu.gameObject);
        }

        public async static void LoadNetGame() {
            // var scene = SceneManager.GetSceneByPath(DefaultSpob.Name);
            var task = SceneManager.LoadSceneAsync(DefaultSpob.Name,LoadSceneMode.Additive);
            await task; await 0.1;
            LoadNetGame(DefaultPilot, DefaultStar, DefaultSpob);
        }

        public static void LoadNetGame(PilotProfile pilot, StarProfile star, SpobProfile spob) {
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene()); StartHost();
            var sun = Create(star.prefab);
            var user = Create<SpacePlayer>(pilot.prefab);
            var ship = Create<Spaceship>(pilot.ship.prefab);
            var scene = SceneManager.GetSceneByPath(spob.Name);
            DontDestroyOnLoad(user.gameObject);
            DontDestroyOnLoad(ship.gameObject);
            ship.Spobs = spob.Star.Spobs;
            (sun.name, user.name, user.Ship) = (star.name, pilot.name, ship);
            SceneManager.SetActiveScene(scene);
            network.ServerChangeScene(SceneManager.GetActiveScene().name);
            NetworkServer.Spawn(user.gameObject);
            NetworkServer.Spawn(ship.gameObject);
            NetworkServer.SpawnObjects();
            SceneManager.MoveGameObjectToScene(sun.gameObject,scene);
            PlayerCamera.atmosphere = star.atmosphere;
            PlayerCamera.Target = ship.transform;
            Manager.ship = ship;
            StartPositions = FindObjectsOfType<NetworkStartPosition>().ToList();
            var spawn = StartPositions.Pick();
            ship.transform.position = spawn.transform.position;
            ship.transform.rotation = spawn.transform.rotation;
            user.SetShip(ship);
            Destroy(menu.gameObject);
        }


        // public virtual void OnServerAddPlayer(NetworkConnection c, short id) {
        //     var location = StartPositions.Pick();
        //     var player = Create(prefabs.user, location.position, location.rotation);
        //     network.AddPlayerForConnection(c, player, id);
        // }



        // T Deserialize<T>(EventReader o) => deserializer.Deserialize<T>(o);
        // void AddStars(StarProfile system) {
        //     foreach (var star in GetStars(system)) {
        //         var list = Stars[star] ?? new List<StarProfile>();
        //         if (!list.Contains(star))
        //             Stars[star].AddRange(star.NearbySystems);
        //     }
        // }

        // List<StarProfile> GetStars(StarProfile system) {
        //     var list = new List<StarProfile>();
        //     foreach (var star in system.NearbySystems)
        //         foreach (var nearby in GetStars(star))
        //             if (!list.Contains(nearby) && nearby!=star) list.Add(nearby);
        //     return list;
        // }

        // IEnumerator Start() {
        //     var files = Directory.GetFiles($"{path}/{dir}/","*.yml");
        //     foreach (var file in files) { ParseFile(file); yield return null; }
        //     foreach (var pair in objects) foreach (var thing in pair.Value) {
        //         // var type = YamlReader.FromType(pair.Key);
        //         // foreach (var bt in pair.Key.GetTypes(typeof(Adventure.Object)))
        //         //     if (tags.Keys.Contains(YamlReader.FromType(bt))
        //         //     && defaults.TryGetValue(
        //         //                     tags[YamlReader.FromType(bt)],
        //         //                     out Adventure.Object bd))
        //         //         bd.Deserialize(thing.Value);
        //         // if (tags.Keys.Contains(type)
        //         // && defaults.TryGetValue(tags[type], out Adventure.Object.Data yml))
        //         //     yml.Deserialize(thing.Value);
        //         // if (!data.TryGetValue(type,out Map<Adventure.Object.Data> map))
        //         //     continue;
        //         // if (!map.TryGetValue(thing.Key, out var yml)) {
        //             // print($"no entry under name {thing.Key}?"); continue; }
        //         // yml.Deserialize(thing.Value);
        //         yield return null;
        //     }
        // }

        // void ParseFile(string file) {
        //     var reader = new EventReader(
        //         new YamlDotNet.Core.Parser(
        //             new StreamReader(file)));
        //     reader.Expect<StreamStart>();
        //     if (!reader.Accept<DocumentStart>()) return;
        //     var document = Deserialize<Map<List<string>>>(reader);
        //     foreach (var type in document["type"]) {
        //         if (!reader.Accept<DocumentStart>())
        //             throw new System.Exception("wrong type data in yaml file");
        //         switch (type) {
        //             case "starsystem":
        //                 var systems = Deserialize<Map<StarSystem>>(reader);
        //                 foreach (var pair in systems)
        //                     starSystemData[pair.Key] = pair.Value;
        //                 break;
        //             case "date": break;
        //             case "planet": break;
        //             // case "message": ParseFile<Message>(reader); break;
        //             // case "person": ParseFile<Person.Data>(reader); break;
        //             // case "actor": ParseFile<Actor.Data>(reader); break;
        //             // case "door": ParseFile<Door.Data>(reader); break;
        //             // case "room": ParseFile<Room.Data>(reader); break;
        //             // case "book": ParseFile<Book.Data>(reader); break;
        //             // case "lamp": ParseFile<Lamp.Data>(reader); break;
        //             // case "item": ParseFile<Item.Data>(reader); break;
        //             // case "button": ParseFile<Button.Data>(reader); break;
        //             // case "lever": ParseFile<Lever.Data>(reader); break;
        //             // case "thing": ParseFile<Thing.Data>(reader); break;
        //             default: throw new System.Exception(
        //                 "unknown type markers in yaml file");
        //         }
        //     }
        // }

        // void ParseFile<T>(EventReader reader) {
        //     foreach (var pair in Deserialize<Map<T>>(reader)) {
        //         var type = YamlReader.FromType(typeof(T));
        //         if (!data.ContainsKey(type))
        //             data[type] = new Map<Adventure.Object.Data>();
        //         data[type][pair.Key] = pair.Value as Adventure.Object.Data;
        //     }
        // }
    }
}
