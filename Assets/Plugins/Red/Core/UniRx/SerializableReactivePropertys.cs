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