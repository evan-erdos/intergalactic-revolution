using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Adventure.Astronautics {
    public class SpaceManager : MonoBehaviour {
        string path, root = "adventure", dir = "star-systems";
        Deserializer deserializer = new Deserializer();
        Map<Type,Map<SpaceObject>> objects = new Map<Type,Map<SpaceObject>>();
        Map<Map<SpaceObject.Data>> data = new Map<Map<SpaceObject.Data>>();
        static Map<StarSystem.Data> starSystemData = new Map<StarSystem.Data>();
        public static Settings settings {get;set;}
        public static StarSystem CurrentSystem {get;set;}
        public static StarSystem Destination {get;set;}
        public static List<(string,double[])> NearbySystems => GetNearby(CurrentSystem);
        public static Map<StarSystem> StarSystems = new Map<StarSystem>();
        public static readonly Map<Type> tags = new Map<Type> {
            ["object"] = typeof(SpaceObject.Data),
            ["system"] = typeof(StarSystem.Data),
            ["settings"] = typeof(Adventure.Settings) };

        public static List<(string,double[])> GetNearby(StarSystem system) {
            var list = new List<(string,double[])>();
            foreach (var item in starSystemData[system.Name].systems)
                list.Add((item, starSystemData[item].position));
            return list;
        }


        void Awake() {
            path = $"{Application.streamingAssetsPath}/{root}";
            deserializer = YamlReader.GetDefaultDeserializer();
            // ParseDefaults($"{path}/adventure.yml");
            var objs = FindObjectsOfType(typeof(SpaceObject)) as SpaceObject[];
            foreach (var obj in objs) {
                obj.Init();
                var type = obj.GetType();
                if (!objects.ContainsKey(type))
                    objects[type] = new Map<SpaceObject>();
                objects[type][obj.name] = obj;
            }
        }

        IEnumerator Start() {
            var files = Directory.GetFiles($"{path}/{dir}/","*.yml");
            foreach (var file in files) { ParseFile(file); yield return null; }
            foreach (var pair in objects) foreach (var thing in pair.Value) {
                var type = YamlReader.FromType(pair.Key);
                // foreach (var bt in pair.Key.GetTypes(typeof(SpaceObject)))
                //     if (tags.Keys.Contains(YamlReader.FromType(bt))
                //     && defaults.TryGetValue(
                //                     tags[YamlReader.FromType(bt)],
                //                     out SpaceObject.Data bd))
                //         bd.Deserialize(thing.Value);
                // if (tags.Keys.Contains(type)
                // && defaults.TryGetValue(tags[type], out SpaceObject.Data yml))
                //     yml.Deserialize(thing.Value);
                if (!data.TryGetValue(type, out Map<SpaceObject.Data> map)) continue;
                if (!map.TryGetValue(thing.Key, out var yml)) {
                    print($"no entry under name {thing.Key}?"); continue; }
                yml.Deserialize(thing.Value);
                yield return null;
            }
        }

        T Deserialize<T>(EventReader reader) =>
            deserializer.Deserialize<T>(reader);

        void ParseFile(string file) {
            var reader = new EventReader(
                new YamlDotNet.Core.Parser(
                    new StreamReader(file)));
            reader.Expect<StreamStart>();
            if (!reader.Accept<DocumentStart>()) return;
            var document = Deserialize<Map<List<string>>>(reader);
            foreach (var type in document["type"]) {
                if (!reader.Accept<DocumentStart>())
                    throw new System.Exception("wrong type data in yaml file");
                switch (type) {
                    case "starsystem":
                        var systems = Deserialize<Map<StarSystem.Data>>(reader);
                        foreach (var pair in systems)
                            starSystemData[pair.Key] = pair.Value;
                        break;
                    case "date": break;
                    case "planet": break;
                    // case "message": ParseFile<Message>(reader); break;
                    // case "person": ParseFile<Person.Data>(reader); break;
                    // case "actor": ParseFile<Actor.Data>(reader); break;
                    // case "door": ParseFile<Door.Data>(reader); break;
                    // case "room": ParseFile<Room.Data>(reader); break;
                    // case "book": ParseFile<Book.Data>(reader); break;
                    // case "lamp": ParseFile<Lamp.Data>(reader); break;
                    // case "item": ParseFile<Item.Data>(reader); break;
                    // case "button": ParseFile<Button.Data>(reader); break;
                    // case "lever": ParseFile<Lever.Data>(reader); break;
                    // case "thing": ParseFile<Thing.Data>(reader); break;
                    default: throw new System.Exception(
                        "unknown type markers in yaml file");
                }
            }
        }

        void ParseFile<T>(EventReader reader) {
            foreach (var pair in Deserialize<Map<T>>(reader)) {
                var type = YamlReader.FromType(typeof(T));
                if (!data.ContainsKey(type))
                    data[type] = new Map<SpaceObject.Data>();
                data[type][pair.Key] = pair.Value as SpaceObject.Data;
            }
        }
    }
}
