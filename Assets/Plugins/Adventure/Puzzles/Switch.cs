/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-12-02 */

using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Adventure.Puzzles {
    public class Switch : Piece<bool,bool>, IUsable {
        public override void Use() => Pose();
        public override bool Pose(PuzzleArgs<bool,bool> e=null) => Condition = !Condition;
        public override bool Solve(bool o) => Condition = o;
        public override void Init() { base.Init(); onSolve.Add(e => StartAsync(() => OnSolve(e))); }
        async Task OnSolve(StoryArgs e) { Log($"You press the {Name}."); await 1; }

        new public class Data : Piece<bool,bool>.Data {
            public override Object Deserialize(Object o) {
                var instance = base.Deserialize(o) as Switch; return instance; }
        }
    }
}
