/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-12-02 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Puzzles {
    public class Button : Piece, IUsable {
        public override void Use() => Pose();
        public virtual void Pose() => Solve();

        protected override void OnSolve() {
            StartSemaphore(Solving);
            IEnumerator Solving() {
                Log($"You Press the {Name}, and it makes a clicking noise.");
                yield return new WaitForSeconds(1);
            }
        }

        new public class Data : Piece<bool>.Data {
            public override BaseObject Deserialize(BaseObject o) {
                var instance = base.Deserialize(o) as Button;
                return instance;
            }
        }
    }
}
