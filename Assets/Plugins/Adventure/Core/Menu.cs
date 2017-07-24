/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-06-30 */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ui=UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

namespace Adventure {
    public class Menu : Adventure.Object {
        [SerializeField] protected PilotProfile pilot;
        [SerializeField] protected StarProfile star;
        [SerializeField] protected string spob;
        [SerializeField] protected AudioClip click;
        [SerializeField] protected ui::Selectable selection;

        public string PlayerName {get;set;} = "Evan Erdos";
        public ShipProfile PlayerShip {get;set;}

        public void Play() => Click(() => LoadGame());
        public void Load() => Click(() => Load(PlayerName, PlayerShip, PickSyst()));
        public void Quit() => Click(() => Application.Quit());
        public void Continue() => Click(() => LoadGame(pilot, star, spob));
        public void LoadGame() => Click(() => LoadGame(Manager.Pilots.ToList().Pick()));
        public void LoadGame(PilotProfile pilot) => LoadGame(pilot, PickSyst());
        public void LoadGame(PilotProfile pilot, StarProfile star) => LoadGame(pilot, star, star.Subsystems.Pick());
        public void LoadGame(PilotProfile pilot, StarProfile star, string spob) => Get<Loader>().Load(() => Manager.OnLoadGame(pilot,star,spob), spob);
        void Load(string name, ShipProfile ship, StarProfile star) => Load(name,ship,star,star.Subsystems.Pick());
        void Load(string name, ShipProfile ship, StarProfile star, string spob) => Get<Loader>().Load(() => Manager.OnLoad(name,ship,star,spob), spob);
        public async void Click(Action then) { AudioSource.PlayClipAtPoint(click,PlayerCamera.Location,0.5f); await 0; then(); }
        public StarProfile PickSyst() => Manager.StarSystems.Keys.ToList().Pick();
        void Awake() => selection.Select();
        void Start() {
            Create(PickSyst().prefab).transform.parent = transform;
            CameraFade.StartAlphaFade(Color.black,true,1,0);
            PlayerCamera.main.transform.parent = null;
            PlayerCamera.main.transform.position = Vector3.zero;
            PlayerCamera.main.transform.rotation = Quaternion.identity;
        }
    }
}
