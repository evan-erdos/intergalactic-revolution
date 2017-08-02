
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TextMesh=TMPro.TextMeshPro;
using Adventure;
using Adventure.Astronautics;
using Adventure.Astronautics.Spaceships;

public class TextHUD : Adventure.Object {
    enum ShipProp { Target, Speed, Mode, Weapon, Dest, Star, Spob, Cargo };
    TextMesh text; Spaceship ship;
    Map<ShipProp,string> map = new Map<ShipProp,string> {
        [ShipProp.Target] = "Freighter 214", [ShipProp.Speed] = "147.33 m/s",
        [ShipProp.Weapon] = "Diamond Spray", [ShipProp.Cargo] = "20 tons",
        [ShipProp.Star] = "Epsilon Eridani", [ShipProp.Spob] = "Spacedock VI",
        [ShipProp.Dest] = "Formalhaut", [ShipProp.Mode] = "Manual" };

    IEnumerator Start() {
        (text, ship) = (Get<TextMesh>(), GetParent<Spaceship>());
        while (enabled) { Log(); yield return new WaitForSeconds(0.1f); } }

    void Log() {
        var builder = new StringBuilder();
        builder.AppendLine(FindProperty(ShipProp.Target));
        builder.AppendLine(FindProperty(ShipProp.Dest));
        builder.AppendLine(FindProperty(ShipProp.Speed));
        builder.AppendLine(FindProperty(ShipProp.Mode));
        builder.AppendLine(FindProperty(ShipProp.Weapon));
        builder.AppendLine(FindProperty(ShipProp.Star));
        builder.AppendLine(FindProperty(ShipProp.Cargo));
        text.text = builder.ToString();

        string FindProperty(ShipProp property, string s="") {
            switch (property) {
                case ShipProp.Mode: s = $"{ship.Mode}"; break;
                case ShipProp.Speed: s = $"{ship.Speed} m/s"; break;
                case ShipProp.Cargo: s = $"{ship.CargoSpace} tons"; break;
                case ShipProp.Star: s = $"{ship.CurrentSystem?.Name}"; break;
                case ShipProp.Dest: s = $"{ship.Destination?.Name}"; break;
                case ShipProp.Target: s = $"{ship.Target?.Name}"; break;
                case ShipProp.Weapon: s = $"{ship.CurrentWeapon?.Name}"; break;
                default: return $"None";
            } return string.IsNullOrEmpty(s)?"None":s;
        }
    }
}
