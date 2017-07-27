/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-11-13 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// Semaphore : YieldInstruction
/// encapsulates a blocking pattern, only starting a new call if the prior has finished
public class Semaphore : YieldInstruction {
    Func<IEnumerator,Coroutine> action;
    Dictionary<string,Func<IEnumerator>> map = new Dictionary<string,Func<IEnumerator>>();
    public bool AreAnyYielding => map.Count>0;
    public Semaphore(Func<IEnumerator,Coroutine> action) { this.action = action; }
    public bool IsYielding(string name) => map.ContainsKey(name);
    public void Clear() => map.Clear();
    public void Call(Func<IEnumerator> func) => Call(func.Method.Name, func);
    public void Call(string s, Func<IEnumerator> f) { if (!map.ContainsKey(s)) action(Wait(s,f)); }
    IEnumerator Wait(string s, Func<IEnumerator> f) { map[s]=f;yield return action(f());map.Remove(s);} }
