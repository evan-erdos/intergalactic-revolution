
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
    void Awake() => (text,spaceship) = (Get<TextMesh>(),GetParent<Spaceship>());
    void Update() => text.text = $"{GetShipProperty(property)}";
    string GetShipProperty(ShipProperty property) {
        switch (property) {
            case ShipProperty.Mode: return $"{spaceship.Mode}";
            case ShipProperty.Speed: return $"{spaceship.ForwardSpeed} m/s";
            case ShipProperty.Cargo: return $"{spaceship.CargoSpace} tons";
            case ShipProperty.System: return $"{SpaceManager.CurrentSystem}";
            case ShipProperty.Destination: return $"{SpaceManager.Destination}";
            case ShipProperty.Target: return $"{spaceship.Target}";
            // case ShipProperty.Weapon: return $"{spaceship.Weapon}";
            default: return $"None";
        }
    }
}
