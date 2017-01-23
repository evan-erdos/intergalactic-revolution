
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ui=UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Adventure;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class SpaceMenu : SpaceObject {
    bool once;
    [SerializeField] protected PilotProfile pilot;
    [SerializeField] protected StarProfile star;
    [SerializeField] protected string spob;
    [SerializeField] protected AudioClip click;
    [SerializeField] protected ui::Selectable selection;

    public string PlayerName {get;set;} = "Evan Erdos";
    public SpaceshipProfile PlayerShip {get;set;}

    public void Play() => Click(() => LoadGame());
    public void Load() => Click(() => Load(PlayerName,PlayerShip,PickSyst()));
    public void Quit() => Click(() => Application.Quit());
    public void Continue() => Click(() => LoadGame(pilot, star, spob));
    public void LoadGame() => Click(() => LoadGame(PickUser()));
    public void LoadGame(PilotProfile pilot) => LoadGame(pilot,PickSyst());
    public void LoadGame(PilotProfile pilot, StarProfile star) =>
        LoadGame(pilot, star, star.Subsystems.Pick());
    public void LoadGame(PilotProfile pilot, StarProfile star, string spob) =>
        Get<SceneLoader>().Load(() => OnLoadGame(pilot,star,spob), spob);

    void Load(string name, SpaceshipProfile ship, StarProfile star) =>
        Load(name,ship,star,star.Subsystems.Pick());
    void Load(string name, SpaceshipProfile ship, StarProfile star, string spob) =>
        Get<SceneLoader>().Load(() => OnLoad(name, ship, star, spob), spob);
    void OnLoad(
                    string name,
                    SpaceshipProfile shipProfile,
                    StarProfile starSystem,
                    string spob) {
        if (once) return; once = true;
        SpaceManager.StartHost();
        var star = Create(starSystem.prefab);
        // var user = Create<SpacePlayer>(pilot.prefab);
        var pilot = PickUser();
        var user = Create<SpacePlayer>(pilot.prefab);
        var ship = Create<Spaceship>(pilot.ship.prefab);
        var scene = SceneManager.GetSceneByName(spob);
        DontDestroyOnLoad(ship.gameObject);
        // ship.Create();
        (star.name, user.name, user.Ship) = (starSystem.name, pilot.name, ship);
        SceneManager.MoveGameObjectToScene(star.gameObject,scene);
        SceneManager.MoveGameObjectToScene(user.gameObject,scene);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(spob));
        user.Reset();
        DontDestroyOnLoad(user.gameObject);
        PlayerCamera.atmosphere = starSystem.atmosphere;
        PlayerCamera.Target = ship.transform;
        var list = new List<NetworkStartPosition>();
        list.Add(FindObjectsOfType<NetworkStartPosition>());
        var spawn = list.Pick();
        ship.JumpEvent += (o,e) => SpaceManager.Jump(
            ship.Destination, ship.Destination.Subsystems.Pick());
        ship.transform.position = spawn.transform.position;
        ship.transform.rotation = spawn.transform.rotation;
        user.SetShip(ship);
        SceneManager.UnloadSceneAsync("Menu");
    }

    void OnLoadGame(PilotProfile pilot, StarProfile starSystem, string spob) {
        if (once) return; once = true;
        SpaceManager.StartHost();
        // var star = Create<StarSystem>(starSystem.prefab);
        var star = Create(starSystem.prefab);
        var user = Create<SpacePlayer>(pilot.prefab);
        var ship = Create<Spaceship>(pilot.ship.prefab);
        var scene = SceneManager.GetSceneByName(spob);
        DontDestroyOnLoad(user.gameObject);
        DontDestroyOnLoad(ship.gameObject);
        // ship.Create();
        ship.Stars = starSystem.NearbySystems;
        (star.name, user.name, user.Ship) = (starSystem.name, pilot.name, ship);
        SceneManager.MoveGameObjectToScene(star.gameObject,scene);
        PlayerCamera.atmosphere = starSystem.atmosphere;
        PlayerCamera.Target = ship.transform;
        var list = new List<NetworkStartPosition>();
        list.Add(FindObjectsOfType<NetworkStartPosition>());
        var spawn = list.Pick();
        ship.transform.position = spawn.transform.position;
        ship.transform.rotation = spawn.transform.rotation;
        user.SetShip(ship);
        SceneManager.UnloadSceneAsync("Menu");
    }

    public void Click(Action then) {
        StartSemaphore(Clicking);
        IEnumerator Clicking() {
            AudioSource.PlayClipAtPoint(click,PlayerCamera.Location,0.5f);
            yield return null; then();
        }
    }

    PilotProfile PickUser() => SpaceManager.Pilots.ToList().Pick();
    StarProfile PickSyst() => SpaceManager.StarSystems.Keys.ToList().Pick();
    void Awake() => selection.Select();
    void Start() {
        Create(PickSyst().prefab);
        CameraFade.StartAlphaFade(Color.black,true,1,0);
        PlayerCamera.main.transform.parent = null;
        PlayerCamera.main.transform.position = Vector3.zero;
        PlayerCamera.main.transform.rotation = Quaternion.identity;
    }
}
