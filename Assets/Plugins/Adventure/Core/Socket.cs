/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-08-04 */

using UnityEngine;
using System.Collections;

namespace Adventure {
    public abstract class Socket<T> : Adventure.Object, ISocket<T> where T : class,IObject {
        public T Target { get { return target; } set { OnSet(target=value); } } protected T target;
        protected virtual void OnSet(T o) { if (o==null) return; o.Location.parent = transform;
            (o.Location.localPosition, o.Location.localRotation) = (Vector3.zero, Quaternion.identity); }
        public void Clear() => Target = null; // atomic tangerine = (255, 153, 102)
        public void SetTarget(T o) => Target = o;
    }
}
