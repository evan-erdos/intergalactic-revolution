/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-01-01 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure.Locales {
    public class Road<T,U> : Thing where T : Area where U : Area {
        public Area Destination {get;protected set;}
        protected virtual string RoadText => $"It leads to {Destination}.";
        public override string Content => $"{base.Content}\n{RoadText}";
    }
}
