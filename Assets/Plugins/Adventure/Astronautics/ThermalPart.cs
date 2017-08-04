/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-07-30 */

using UnityEngine;
using System.Collections;

namespace Adventure.Astronautics.Spaceships {
    public class ThermalPart : Adventure.Object {
        new protected Renderer renderer;
        MaterialPropertyBlock properties;
        public float Heat {get;protected set;} = 0;
        public float HeatGain {get;protected set;} = 100;
        public float HeatLoss {get;protected set;} = 70;
        public float MaxHeat => 2000;
        public void Reset() => (renderer.enabled, Heat) = (true, 0);
        protected virtual void Awake() { GetParent<ISpaceship>().MoveEvent += OnMove;
            (renderer, properties) = (Get<Renderer>(), new MaterialPropertyBlock()); }

        void OnMove(FlightArgs e) {
            Heat += ((0.3<e.Thrust)?HeatGain:-HeatLoss)*Time.fixedDeltaTime;
            var ratio = Mathf.Clamp(Heat,0,MaxHeat)/MaxHeat;
            var color = new Color(ratio,ratio,ratio,1);
            renderer.GetPropertyBlock(properties);
            properties.SetColor("_EmissionColor", color);
            renderer.SetPropertyBlock(properties);
        }
    }
}
