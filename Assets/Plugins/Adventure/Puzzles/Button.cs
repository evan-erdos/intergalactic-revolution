/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-12-02 */

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Adventure.Puzzles {
    public class Button : Piece, IUsable {
        public override void Use() => Pose();
        public virtual void Pose() => Solve();
        public override void Init() { base.Init();
            onSolve.AddListener((o,e) => StartAsync(() => OnSolve(o,e))); }
        async Task OnSolve(IThing o, StoryArgs e) {
            Log($"You press the {Name} and it clicks."); await 1; }

        new public class Data : Piece<bool,bool>.Data {
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Button;
                return instance;
            }
        }
    }
}
