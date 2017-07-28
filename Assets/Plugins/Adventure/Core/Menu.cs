﻿/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-06-30 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ui=UnityEngine.UI;
using UnityEngine.Networking;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

namespace Adventure {
    public class Menu : Adventure.Object {
        [SerializeField] protected AudioClip click;
        [SerializeField] protected ui::Selectable selection;
        public string PlayerName {get;set;} = "Evan Erdos";
        public ShipProfile PlayerShip {get;set;}

        public void Play() => Click(() => LoadGame());
        public void Quit() => Click(() => Application.Quit());
        public void Load() => Click(() => Load(PlayerName, PlayerShip, PickSyst()));
        void Load(string name, ShipProfile ship, StarProfile star) => Load(name,ship,star,star.Spobs.Pick());
        void Load(string name, ShipProfile ship, StarProfile star, SpobProfile spob) =>
            Get<Loader>().Load(() => Manager.OnLoad(ship,star,spob), spob.Name);

        public void Continue() => Click(() => LoadGame(Manager.DefaultPilot, Manager.DefaultStar, Manager.DefaultSpob));
        public void LoadGame() => Click(() => LoadGame(Manager.Pilots.ToList().Pick()));
        public void LoadGame(PilotProfile pilot) => LoadGame(pilot, PickSyst());
        public void LoadGame(PilotProfile pilot, StarProfile star) => LoadGame(pilot, star, star.Spobs.Pick());
        public void LoadGame(PilotProfile pilot, StarProfile star, SpobProfile spob) =>
            Get<Loader>().Load(() => Manager.OnLoadGame(pilot,star,spob), spob.Name);

        public async void Click(Action f) { AudioSource.PlayClipAtPoint(click,PlayerCamera.Location,0.5f); await 0; f(); }
        StarProfile PickSyst() => Manager.Stars.Keys.ToList().Pick();

        void Start() {
            selection.Select();
            Create(PickSyst().prefab).transform.parent = transform;
            var cam = PlayerCamera.main.transform;
            cam.parent = null; cam.position = Vector3.zero; cam.rotation = Quaternion.identity;
        }
    }
}
