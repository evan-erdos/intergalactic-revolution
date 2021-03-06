﻿/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Adventure;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class SpaceshipFactory : Adventure.Object {
    public List<GameObject> ships = new List<GameObject>();
    public List<Transform> locations = new List<Transform>();
    void Start() => CreateShip();
    public void CreateShip() {
        StartAsync(Creating);
        async Task Creating() {
            await 1; if (!this || !this.enabled) return;
            var instance = Create<Spaceship>(ships.Pick(),locations.Pick().position);
            instance.KillEvent += e => CreateShip();
            instance.gameObject.name = $"Viper {Random.Range(10,999)}";
            await 8;
        }
    }
}
