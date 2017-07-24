/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2015-07-07 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Adventure.Inventories;

namespace Adventure.Movement {
    public class Hand : MonoBehaviour {
        public bool ikActive;
        public Transform objHand;
        public AvatarIKGoal handGoal;
        public IWieldable heldItem;
        public bool IKEnabled {get;set;}
        public void SwitchItem(IWieldable o) => heldItem = o;
        void Start() => handGoal = AvatarIKGoal.LeftHand;
    }
}
