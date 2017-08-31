/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-28 */

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Adventure;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class SpaceshipFactory : Adventure.Object {
    public List<ShipProfile> ships = new List<ShipProfile>();
    public List<Transform> locations = new List<Transform>();
    void Start() => CreateShip();
    public void CreateShip() {
        StartAsync(Creating);
        async Task Creating() {
            await 1; if (!this || !this.enabled) return;
            var instance = Create<Spaceship>(ships.Pick().prefab,locations.Pick().position);
            instance.KillEvent += e => CreateShip();
            instance.gameObject.name = $"Speeder {Random.Range(10,999)}";
            instance.gameObject.AddComponent<SpaceshipAI>();
            instance.gameObject.transform.SetLayer("NPC");
            await 8;
        }
    }
}
