﻿/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class SpaceshipFactory : Adventure.Object {
    public List<GameObject> ships = new List<GameObject>();
    public List<Transform> locations = new List<Transform>();
    void Start() => CreateShip();
    public void CreateShip() {
        StartSemaphore(Creating);
        IEnumerator Creating() {
            var instance = Create<Spaceship>(ships.Pick(),locations.Pick().position);
            instance.GetChildren<Adventure.Object>().ForEach(o => o.Create());
            instance.Create();
            instance.KillEvent += (o,e) => Create();
            instance.gameObject.name = $"Viper {Random.Range(10,999)}";
            yield return new WaitForSeconds(8);
        }
    }
}
