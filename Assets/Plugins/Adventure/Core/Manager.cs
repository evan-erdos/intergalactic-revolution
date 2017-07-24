using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Adventure.Astronautics.Spaceships;

namespace Adventure.Astronautics {
    public class Manager : Adventure.Object {
        [SerializeField] protected AdventurePrefabs prefabs = new AdventurePrefabs();
        [SerializeField] StarProfile[] StarProfiles = new StarProfile[1];
        [SerializeField] PilotProfile[] PilotProfiles = new PilotProfile[1];
        [SerializeField] ShipProfile[] ShipProfiles = new ShipProfile[1];
        [Serializable] protected class AdventurePrefabs {
            [SerializeField] public GameObject menu;
            [SerializeField] public GameObject camera;
            [SerializeField] public GameObject player; }

        public static readonly string root = "adventure", dir = "star-systems";
        public static string path {get;protected set;}
        public static Manager singleton {get;private set;}
        public static Settings settings {get;set;}
        public static NetworkManager network {get;protected set;}
        public static Player user {get;protected set;}
        public static Spaceship ship {get;protected set;}
        public static Menu menu {get;protected set;}
        public static new PlayerCamera camera {get;protected set;}
        public static PilotProfile[] Pilots {get;protected set;}
        public static ShipProfile[] Ships {get;protected set;}
        public static Map<StarProfile,StarProfile[]> StarSystems {get;} = new Map<StarProfile,StarProfile[]>();
        public static readonly Map<Type> tags = new Map<Type> {
            ["object"] = typeof(Adventure.Object),
            ["system"] = typeof(StarSystem),
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


        public static void Jump(StarProfile starSystem, string spob) {
            var scene = SceneManager.GetSceneByName(spob);
            var star = Create(starSystem.prefab);
            LoadScene(spob);
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            SceneManager.MoveGameObjectToScene(star.gameObject,scene);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(spob));
            // var list = new List<NetworkStartPosition>();
            // list.Add(FindObjectsOfType<NetworkStartPosition>());
            // var spawn = list.Pick();
            // ship.transform.position = spawn.transform.position;
            // ship.transform.rotation = spawn.transform.rotation;
            // user.SetShip(ship);
            PlayerCamera.atmosphere = starSystem.atmosphere;
            // PlayerCamera.Target = ship.transform;
        }


        public static void LoadScene(string scene) =>
            CameraFade.StartAlphaFade(Color.black,false,1,0,() => SceneManager.LoadSceneAsync(scene));

        void Awake() {
            path = $"{Application.streamingAssetsPath}/{root}/{dir}";
            if (singleton is null) singleton = this;
            else { Destroy(gameObject); return; }
            network = Get<NetworkManager>();
            DontDestroyOnLoad(gameObject);
            StarProfiles.ForEach(o => StarSystems[o] = o.NearbySystems);
            (Pilots, Ships) = (PilotProfiles, ShipProfiles);
        }

        void Start() { camera = Create<PlayerCamera>(prefabs.camera); LoadMenu(); }

        public static void LoadMenu() => menu = Create<Menu>(singleton.prefabs.menu); // SceneManager.LoadSceneAsync("Menu");

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
        // [Command]
        public void CmdCreateShip(SpaceActor actor, ShipProfile ship) {
            var instance = Instantiate(ship.prefab) as GameObject;
            NetworkServer.Spawn(instance);
            DontDestroyOnLoad(instance);
            actor.Ship = instance.Get<Spaceship>();
            actor.Ship.Init(); actor.Ship.Create();
            actor.Ship.GetChildren<Adventure.Object>().ForEach(o=>o.Init());
            actor.Ship.GetChildren<Adventure.Object>().ForEach(o=>o.Create());
        }


        public static void OnLoad(string name, ShipProfile shipData, StarProfile profile, string spob) {
            StartHost();
            var star = Create(profile.prefab);
            var pilot = Pilots.ToList().Pick();
            var user = Create<SpacePlayer>(pilot.prefab);
            var ship = Create<Spaceship>(pilot.ship.prefab);
            var scene = SceneManager.GetSceneByName(spob);
            DontDestroyOnLoad(ship.gameObject);
            (star.name, user.name, user.Ship) = (profile.name, pilot.name, ship);
            SceneManager.SetActiveScene(scene);
            SceneManager.MoveGameObjectToScene(star.gameObject,scene);
            SceneManager.MoveGameObjectToScene(user.gameObject,scene);
            user.Reset();
            DontDestroyOnLoad(user.gameObject);
            PlayerCamera.atmosphere = profile.atmosphere;
            PlayerCamera.Target = ship.transform;
            var list = new List<NetworkStartPosition>();
            list.Add(FindObjectsOfType<NetworkStartPosition>());
            var spawn = list.Pick();
            ship.JumpEvent += (o,e) => Manager.Jump(ship.Destination, ship.Destination.Subsystems.Pick());
            ship.transform.position = spawn.transform.position;
            ship.transform.rotation = spawn.transform.rotation;
            user.SetShip(ship);
            Destroy(menu.gameObject);
        }


        public static void OnLoadGame(PilotProfile pilot, StarProfile profile, string spob) {
            StartHost();
            // var star = Create<StarSystem>(profile.prefab);
            var star = Create(profile.prefab);
            var user = Create<SpacePlayer>(pilot.prefab);
            var ship = Create<Spaceship>(pilot.ship.prefab);
            var scene = SceneManager.GetSceneByName(spob);
            DontDestroyOnLoad(user.gameObject);
            DontDestroyOnLoad(ship.gameObject);
            ship.Stars = profile.NearbySystems;
            (star.name, user.name, user.Ship) = (profile.name, pilot.name, ship);
            SceneManager.SetActiveScene(scene);
            SceneManager.MoveGameObjectToScene(star.gameObject,scene);
            PlayerCamera.atmosphere = profile.atmosphere;
            PlayerCamera.Target = ship.transform;
            Manager.ship = ship;
            var list = FindObjectsOfType<NetworkStartPosition>().ToList();
            var spawn = list.Pick();
            ship.transform.position = spawn.transform.position;
            ship.transform.rotation = spawn.transform.rotation;
            user.SetShip(ship);
            Destroy(menu.gameObject);
        }


        // T Deserialize<T>(EventReader o) => deserializer.Deserialize<T>(o);
        // void AddStars(StarProfile system) {
        //     foreach (var star in GetStars(system)) {
        //         var list = StarSystems[star] ?? new List<StarProfile>();
        //         if (!list.Contains(star))
        //             StarSystems[star].AddRange(star.NearbySystems);
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
