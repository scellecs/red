#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace UniRx {
    using System;
    using UnityEngine;

    [Serializable]
    public class GameObjectReactiveProperty : ReactiveProperty<GameObject> {
    }

    [Serializable]
    public class TransformReactiveProperty : ReactiveProperty<Transform> {
    }

    [Serializable]
    public class RigidbodyReactiveProperty : ReactiveProperty<Rigidbody> {
    }
}
#endif