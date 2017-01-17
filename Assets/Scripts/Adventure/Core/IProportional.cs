/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2017-01-13 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure {

    /// IProportional
    /// anything which can be created from a single ratio, [0...1]
    public interface IProportional { float Ratio {get;set;} }
}
