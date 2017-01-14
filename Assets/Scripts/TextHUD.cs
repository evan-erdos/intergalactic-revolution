
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class TextHUD : SpaceObject {
    enum ShipProperty { Mode, Speed, Cargo, System, Destination, Target, Weapon };
    [SerializeField] ShipProperty property = ShipProperty.Mode;
    TextMesh text;
    Spaceship spaceship;
    void Start() => (text,spaceship) = (Get<TextMesh>(),GetParent<Spaceship>());
    void Update() => text.text = $"{GetShipProperty(property)}";
    string GetShipProperty(ShipProperty property) {
        var s = "";
        switch (property) {
            case ShipProperty.Mode: s = $"{spaceship.Mode}"; break;
            case ShipProperty.Speed: s = $"{spaceship.ForwardSpeed} m/s"; break;
            case ShipProperty.Cargo: s = $"{spaceship.CargoSpace} tons"; break;
            case ShipProperty.System: s = $"{SpaceManager.CurrentSystem}"; break;
            case ShipProperty.Destination: s = $"{SpaceManager.Destination}"; break;
            case ShipProperty.Target: s = $"{spaceship.Target?.Name}"; break;
            case ShipProperty.Weapon: s = $"{spaceship.Weapon.Name}"; break;
            default: return $"None";
        } return string.IsNullOrEmpty(s)?"None":s;
    }
}
