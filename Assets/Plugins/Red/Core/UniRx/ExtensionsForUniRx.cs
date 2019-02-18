#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace UniRx {
    using System;

    public static class ExtensionsForUniRx {
        public static IDisposable Subscribe<T>(this IObservable<T> observable, IReactiveProperty<T> reactiveProperty)
            => observable.Subscribe(value => reactiveProperty.Value = value);

        public static IObservable<T> WhereNotNull<T>(this IObservable<T> observable) where T : class
            => observable.Where(v => v != null);
    }
}
#endif