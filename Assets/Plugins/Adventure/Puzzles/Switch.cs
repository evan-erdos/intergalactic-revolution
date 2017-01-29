/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-12-02 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Puzzles {
    public class Switch : Piece<bool>, IUsable {
        public override void Use() => Pose();
        public override bool Pose() => Condition = !Condition;
        public override bool Solve(bool condition) => Condition = condition;

        protected override void OnSolve() {
            StartSemaphore(Solving);
            IEnumerator Solving() {
                Log($"You Press the {Name}, and it clicks into place.");
                yield return new WaitForSeconds(1);
            }
        }

        new public class Data : Piece<bool>.Data {
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Switch;
                return instance;
            }
        }
    }
}
