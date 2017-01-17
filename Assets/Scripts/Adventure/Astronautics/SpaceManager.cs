using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using Adventure.Astronautics.Spaceships;

namespace Adventure.Astronautics {
    public class SpaceManager : MonoBehaviour {
        string path, root = "adventure", dir = "star-systems";
        NetworkManager network;
        Deserializer deserializer = new Deserializer();
        [SerializeField] protected StarProfile CurrentStarSystem;
        [SerializeField] StarProfile[] StarProfiles = new StarProfile[1];
        [SerializeField] PilotProfile[] PilotProfiles = new PilotProfile[1];
        [SerializeField] SpaceshipProfile[] ShipProfiles = new SpaceshipProfile[1];
        public static PilotProfile[] Pilots {get;protected set;}
        public static SpaceshipProfile[] Ships {get;protected set;}
        public static Map<StarProfile,StarProfile[]> StarSystems {get;} =
            new Map<StarProfile,StarProfile[]>();

        public static SpaceManager singleton {get;private set;}
        public static Settings settings {get;set;}
        public static readonly Map<Type> tags = new Map<Type> {
            ["object"] = typeof(SpaceObject),
            ["system"] = typeof(StarSystem),
            ["settings"] = typeof(Adventure.Settings) };

        public static bool IsOnline => false;
        public static void StopHost() => singleton.network.StopHost();
        public static void StartHost() => singleton.network.StartHost();
        public static void StartServer() => singleton.network.StartServer();
        public static void StartClient() => singleton.network.StartClient();
        public static void StartMatchMaker() => singleton.network.StartMatchMaker();
        // public static void CreateMatch() =>
        //     singleton.network.matchMaker.CreateMatch(
        //         singleton.network.matchName,
        //         singleton.network.matchSize, true, "",
        //         singleton.network.OnMatchCreate);

        // manager.matchMaker.JoinMatch(match.networkId, "", manager.OnMatchJoined);

        public static void Jump(StarProfile star, string spob) {
            print("you jumped!");
        }


        void Awake() {
            path = $"{Application.streamingAssetsPath}/{root}/{dir}";
            if (singleton is null) singleton = this;
            else { Destroy(gameObject); return; }
            network = GetComponent<NetworkManager>();
            DontDestroyOnLoad(gameObject);
            StarProfiles.ForEach(o => StarSystems[o] = o.NearbySystems);
            (Pilots,Ships) = (PilotProfiles,ShipProfiles);
            // var scene = SceneManager.GetSceneByName("Base");
            // foreach (var obj in scene.GetRootGameobjects())
            //     SceneManager.MoveGameObjectToScene(obj, "Menu");
        }

        void Start() => SceneManager.UnloadSceneAsync("Base");


        // public void CreateShip() => CmdCreateShip(shipPrefab);
        // [Command] public void CmdCreateShip(GameObject prefab) {
        //     var instance = Instantiate(prefab) as GameObject;
        //     NetworkServer.Spawn(instance);
        //     Ship = instance.Get<Spaceship>();
        //     Ship.Create();
        //     Ship.GetComponentsInChildren<ISpaceObject>().ForEach(o=>o.Create());
        //     control.Ship = Ship;
        //     Ship.KillEvent += (o,e) => OnKill();
        //     Ship.JumpEvent += (o,e) => OnJump();
        //     if (isLocalPlayer) PlayerCamera.Target = Ship.transform;
        // }

        public void CreateShip() => CreateShip();
        // [Command]
        public void CmdCreateShip(SpaceActor actor, SpaceshipProfile ship) {
            var instance = Instantiate(ship.prefab) as GameObject;
            NetworkServer.Spawn(instance);
            DontDestroyOnLoad(instance);
            actor.Ship = instance.Get<Spaceship>();
            actor.Ship.Create();
            actor.Ship.GetComponentsInChildren<ISpaceObject>().ForEach(o=>o.Create());
        }

        // void Start() => CreateShip();


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
        //         // foreach (var bt in pair.Key.GetTypes(typeof(SpaceObject)))
        //         //     if (tags.Keys.Contains(YamlReader.FromType(bt))
        //         //     && defaults.TryGetValue(
        //         //                     tags[YamlReader.FromType(bt)],
        //         //                     out SpaceObject bd))
        //         //         bd.Deserialize(thing.Value);
        //         // if (tags.Keys.Contains(type)
        //         // && defaults.TryGetValue(tags[type], out SpaceObject.Data yml))
        //         //     yml.Deserialize(thing.Value);
        //         // if (!data.TryGetValue(type,out Map<SpaceObject.Data> map))
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
        //             data[type] = new Map<SpaceObject.Data>();
        //         data[type][pair.Key] = pair.Value as SpaceObject.Data;
        //     }
        // }
    }
}
