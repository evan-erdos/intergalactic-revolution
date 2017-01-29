using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

using Adventure.Locales;
using Adventure.Inventories;
using Adventure.Puzzles;

namespace Adventure {
    public class Story : MonoBehaviour {
        string path, root = "adventure", dir = "cloister";
        Deserializer deserializer = new Deserializer();
        [SerializeField] public Player player;
        Map<Type,Thing.Data> defaults = new Map<Type,Thing.Data>();
        Map<Map<Thing.Data>> data = new Map<Map<Thing.Data>>();
        Map<Type,Map<Thing>> things = new Map<Type,Map<Thing>>();
        public Settings settings {get;set;}
        public Map<Verb> commands {get;set;}

        public static Map<Room> Rooms = new Map<Room>();
        public static readonly Map<Type> tags = new Map<Type> {
            ["object"] = typeof(Adventure.BaseObject.Data),
            ["thing"] = typeof(Adventure.Thing.Data),
            ["actor"] = typeof(Adventure.Actor.Data),
            ["person"] = typeof(Adventure.Person.Data),
            ["player"] = typeof(Adventure.Player.Data),
            ["area"] = typeof(Adventure.Locales.Area.Data),
            ["room"] = typeof(Adventure.Locales.Room.Data),
            ["path"] = typeof(Adventure.Locales.Path.Data),
            ["door"] = typeof(Adventure.Locales.Door.Data),
            ["message"] = typeof(Adventure.Message),
            ["encounter"] = typeof(Adventure.Encounter.Data),
            ["item"] = typeof(Adventure.Inventories.Item.Data),
            ["lamp"] = typeof(Adventure.Inventories.Lamp.Data),
            ["book"] = typeof(Adventure.Inventories.Book.Data),
            ["bag"] = typeof(Adventure.Inventories.Bag.Data),
            ["key"] = typeof(Adventure.Inventories.Key.Data),
            ["weapon"] = typeof(Adventure.Inventories.Weapon.Data),
            ["button"] = typeof(Adventure.Puzzles.Button.Data),
            ["lever"] = typeof(Adventure.Puzzles.Lever.Data),
            ["settings"] = typeof(Adventure.Settings)};


        void Awake() {
            path = $"{Application.streamingAssetsPath}/{root}";
            deserializer = YamlReader.GetDefaultDeserializer();
            ParseDefaults($"{path}/adventure.yml");
            var everything = FindObjectsOfType(typeof(Thing)) as Thing[];
            foreach (var thing in everything) {
                var type = thing.GetType();
                if (thing.GetType()==typeof(Room))
                    Rooms[thing.name] = thing as Room;
                if (!things.ContainsKey(type))
                    things[type] = new Map<Thing>();
                things[type][thing.name] = thing;
            }
        }

        IEnumerator Start() {
            var files = Directory.GetFiles($"{path}/{dir}","*.yml");
            foreach (var file in files)
                yield return new Wait(() => ParseFile(file));
            foreach (var pair in things) foreach (var thing in pair.Value) {
                var type = YamlReader.FromType(pair.Key);
                if (type=="treebutton") type = "button";
                foreach (var bt in pair.Key.GetTypes(typeof(Thing)))
                    if (tags.Keys.Contains(YamlReader.FromType(bt))
                    && defaults.TryGetValue(
                                    tags[YamlReader.FromType(bt)],
                                    out Thing.Data bd))
                        bd.Deserialize(thing.Value);
                if (tags.Keys.Contains(type)
                && defaults.TryGetValue(tags[type], out Thing.Data yml))
                    yml.Deserialize(thing.Value);
                if (!data.TryGetValue(type, out Map<Thing.Data> map)) continue;
                if (!map.TryGetValue(thing.Key, out yml)) {
                    print($"no entry under name {thing.Key}?"); continue; }
                yml.Deserialize(thing.Value);
                yield return null;
            }
        }

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
                    case "message": ParseFile<Message>(reader); break;
                    case "person": ParseFile<Person.Data>(reader); break;
                    case "actor": ParseFile<Actor.Data>(reader); break;
                    case "door": ParseFile<Door.Data>(reader); break;
                    case "room": ParseFile<Room.Data>(reader); break;
                    case "book": ParseFile<Book.Data>(reader); break;
                    case "lamp": ParseFile<Lamp.Data>(reader); break;
                    case "item": ParseFile<Item.Data>(reader); break;
                    case "button": ParseFile<Button.Data>(reader); break;
                    case "lever": ParseFile<Lever.Data>(reader); break;
                    case "thing": ParseFile<Thing.Data>(reader); break;
                    default: throw new System.Exception(
                        "unknown type markers in yaml file");
                }
            }
        }

        void ParseFile<T>(EventReader reader) {
            foreach (var pair in Deserialize<Map<T>>(reader)) {
                var type = YamlReader.FromType(typeof(T));
                if (!data.ContainsKey(type))
                    data[type] = new Map<Thing.Data>();
                data[type][pair.Key] = pair.Value as Thing.Data;
            }
        }

        T Deserialize<T>(EventReader reader) =>
            deserializer.Deserialize<T>(reader);

        void ParseDefaults(string file) {
            var reader = new EventReader(
                new YamlDotNet.Core.Parser(
                    new StreamReader(file)));
            reader.Expect<StreamStart>();
            if (!reader.Accept<DocumentStart>()) return;
            var document = Deserialize<Map<List<string>>>(reader);
            foreach (var type in document["type"]) {
                if (!reader.Accept<DocumentStart>())
                    throw new System.Exception(
                        $"no type data or wrong type data in file {file}");
                switch (type) {
                    case "settings":
                        settings = Deserialize<Settings>(reader);
                        break;
                    case "messages":
                        Terminal.Messages = Deserialize<Map<Message>>(reader);
                        break;
                    case "defaults":
                        foreach (var pair in Deserialize<Map<Thing.Data>>(reader))
                            defaults[tags[pair.Key]] = pair.Value;
                        break;
                    case "commands":
                        Terminal.Verbs = Deserialize<Map<Verb>>(reader);
                        break;
                    default: throw new System.Exception(
                        $"unrecognized type signifier in file {file}");
                }
            }
        }
    }
}
