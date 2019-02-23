using System;
using System.Linq;
using Red;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class Test : MonoBehaviour {
    public ReactiveProperty<int> origin = new ReactiveProperty<int>();

    private void Update() {
        this.origin.Value = Random.Range(0, 100);
    }
}


//
//public static class App {
//    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
//    public static async void Main() {
//    }
//}