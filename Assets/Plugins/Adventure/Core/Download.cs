/* Ben Scott * @evan-erdos * bescott@andrew.cmu.edu * 2016-12-23 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using YamlDotNet.Serialization;

/// Download : coroutine
/// waits for a download and then raises the callback
public class Download : CustomYieldInstruction, IDisposable {
    bool disposed;
    Action<WWW> then;
    protected WWW www;
    protected Action<WWW> error = o => Debug.Log($"error: {o.error}\nurl: {o.url}");
    public float Progress => www?.progress ?? 0;
    public bool IsDone => www?.isDone ?? true;
    public override bool keepWaiting { get {
        if (!www.isDone) return true;
        if (string.IsNullOrEmpty(www.error)) then(www);
        else error?.Invoke(www); return false; } }

    ~Download() { Dispose(false); }
    public Download(string path, Action<WWW> then, Action<WWW> error=null)
        : this(new WWW(EscapeURL(path)), then, error) { }
    public Download(WWW www, Action<WWW> then, Action<WWW> error=null) {
        (this.www, this.then, this.error) = (www, then, error); }

    public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
    protected virtual void Dispose(bool dispose) { if (!(disposed || !dispose)) {
        www.Dispose(); www = null; then = null; error = null; disposed = true; } }
    static protected string EscapeURL(string o) => System.Uri.EscapeUriString(
        (new Regex(@"(file|https?)://").IsMatch(o)?"":"file://")+o);
}

/// DownloadAsync : coroutine
/// waits for a download and then raises the callback
// public class DownloadAsync : CustomYieldInstruction, IDisposable {
//     bool disposed;
//     Action<WWW> then;
//     protected WWW www;
//     protected AsyncOperation op;
//     protected Action<WWW> error = o => Debug.Log($"error: {o.error}\nurl: {o.url}");
//     public float Progress => www?.progress ?? 0;
//     public override bool keepWaiting { get {
//         if (!www.isDone) return true;
//         if (www.error.IsNullOrEmpty()) then(www);
//         else error?.Invoke(www); return false; } }

//     ~Download() { Dispose(false); }

//     public Download(string path, Action<WWW> then, Action<WWW> error=null)
//         : this(new WWW(EscapeURL(path)), then, error) { }

//     public Download(WWW www, Action<WWW> then, Action<WWW> error=null) {
//         this.www = www; this.then = then; this.error = error; }

//     public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
//     protected virtual void Dispose(bool dispose) { if (!(disposed || !dispose)) {
//         www.Dispose(); www = null; then = null; error = null; disposed = true; } }
//     static protected string EscapeURL(string o) => System.Uri.EscapeUriString(
//         (new Regex(@"(file|https?)://").IsMatch(o)?"":"file://")+o);
// }

/// Download<T> : coroutine
/// waits for a download, and then invokes the callback with an object of type T
public class Download<T> : Download {
    static Deserializer reader = CreateReader();
    public Download(string path, Action<T> then, Action<WWW> error=null)
        : base(www: new WWW(EscapeURL(path)), error: error, then: o => then(Parse(o))) { }
    public Download(WWW www, Action<T> then, Action<WWW> error=null)
        : base(www: www, error: error, then: o => then(Parse(o))) { }
    static protected T Parse(WWW o) => reader.Deserialize<T>(new StringReader(o.text));
    public static Deserializer CreateReader() {
        var deserializer = new Deserializer(ignoreUnmatched: true);
        var tags = new Dictionary<string,Type>();
        var converters = new List<IYamlTypeConverter> { new RegexYamlConverter(), new Vector2YamlConverter() };
        converters.ForEach(o => deserializer.RegisterTypeConverter(o));
        foreach (var tag in tags) deserializer.RegisterTagMapping($"tag:yaml.org,2002:{tag.Key}", tag.Value);
        return deserializer;
    }
}
