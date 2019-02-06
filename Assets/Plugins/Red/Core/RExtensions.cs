#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red {
    using System;
    using System.Collections.Generic;
    using UniRx;
    using UniRx.Operators;
    using JetBrains.Annotations;
    using UniRx.Async;
    using UnityEngine;

    public static partial class RContractExtension {
        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or null if it doesn't exists for current object
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="identifier">Unique identifier for contract</param>
        [CanBeNull]
        public static T TryGet<T>(this Component component, string identifier = "") where T : RContract<T>, new() {
            return RContract<T>.TryGet(component.gameObject, identifier);
        }

        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or null if it doesn't exists for current object
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="identifier">Unique identifier for contract</param>
        [CanBeNull]
        public static T TryGet<T>(this GameObject gameObject, string identifier = "") where T : RContract<T>, new() {
            return RContract<T>.TryGet(gameObject, identifier);
        }

        /// <summary>
        ///     Search for the instance of <see cref="RContract{T0}" />
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="contract">Instance of <see cref="RContract{T0}" /> or null</param>
        /// <param name="identifier">Unique identifier for contract</param>
        /// <returns>True if found, false if instance is null</returns>
        public static bool TryGet<T>(this GameObject gameObject, [CanBeNull] out T contract, string identifier = "")
            where T : RContract<T>, new() {
            contract = RContract<T>.TryGet(gameObject, identifier);
            return contract != null;
        }

        /// <summary>
        ///     Search for the instance of <see cref="RContract{T0}" />
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="contract">Instance of <see cref="RContract{T0}" /> or null</param>
        /// <param name="identifier">Unique identifier for contract</param>
        /// <returns>True if found, false if instance is null</returns>
        public static bool TryGet<T>(this Component component, [CanBeNull] out T contract, string identifier = "")
            where T : RContract<T>, new() {
            contract = RContract<T>.TryGet(component.gameObject, identifier);
            return contract != null;
        }

        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or create it, if it doesn't exists on current gameObject
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="identifier">Unique identifier for contract</param>
        public static T GetOrCreate<T>(this Component component, string identifier = "") where T : RContract<T>, new() {
            return RContract<T>.GetOrCreate(component.gameObject, identifier);
        }

        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or create it, if it doesn't exists on current gameObject
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="identifier">Unique identifier for contract</param>
        public static T GetOrCreate<T>(this GameObject gameObject, string identifier = "")
            where T : RContract<T>, new() {
            return RContract<T>.GetOrCreate(gameObject, identifier);
        }

        /// <summary>
        ///     Register contract in container
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        public static void RegisterIn<T>(this T contract, RContainer container) where T : RContract {
            container.Register(contract);
        }
    }

    public static partial class RLinqExtension {
        /// <summary>
        ///     Default iteration in functional style.
        /// </summary>
        /// <param name="source">Some Enumerable</param>
        /// <param name="action">Functor</param>
        /// <typeparam name="T">Type of Enumerable</typeparam>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var element in source) {
                action(element);
            }
        }

        /// <summary>
        ///     Sequential iteration for async API.
        /// </summary>
        /// <param name="source">Some Enumerable</param>
        /// <param name="action">Functor</param>
        /// <typeparam name="T">Type of Enumerable</typeparam>
        public static async UniTask ForEachAsync<T>(this IEnumerable<T> source, Func<T, UniTask> action) {
            foreach (var element in source) {
                await action(element);
            }
        }
    }

    public static partial class RExtensions {
        /// <summary>
        ///     Awaiter for TimeSpan.
        /// </summary>
        /// <param name="timeSpan">Any TimeSpan</param>
        /// <returns>Awaiter</returns>
        public static AsyncSubject<long> GetAwaiter(this TimeSpan timeSpan) {
            return Observable.Timer(timeSpan).GetAwaiter();
        }

        /// <summary>
        ///     Add contract to GameObject. Contract will be disposed when GameObject is destroyed.
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="contract"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddTo<T>(this T disposable, RContract contract) where T : IDisposable {
            if (contract?.Target is GameObject o) {
                disposable.AddTo(o);
                return disposable;
            }

            disposable.Dispose();
            return disposable;
        }

        /// <summary>
        ///     Complex operator, which allows process stream with custom <see cref="ISystem{T, TR}" />
        /// </summary>
        /// <param name="observable">Any Observable</param>
        /// <param name="system">System which receive <typeparamref name="T" /> and push forward <typeparamref name="TR" /></param>
        /// <typeparam name="T">Input type</typeparam>
        /// <typeparam name="TR">Output type</typeparam>
        /// <returns>New Operator Observable</returns>
        public static IObservable<TR> Systemize<T, TR>(this IObservable<T> observable, ISystem<T, TR> system) {
            return new SystemizeObservable<T, TR>(observable, system);
        }


        /// <summary>
        ///     Complex operator, which allows process stream with custom <see cref="ISystem{T, TR}" />
        /// <para/>
        ///     Selector will be called before system
        /// </summary>
        /// <param name="observable">Any Observable</param>
        /// <param name="system">System which receive <typeparamref name="T2" /> and push forward <typeparamref name="TR" /></param>
        /// <param name="selector">Selector</param>
        /// <typeparam name="T1">Input type</typeparam>
        /// <typeparam name="T2">Selected type</typeparam>
        /// <typeparam name="TR">Output type</typeparam>
        /// <returns>New Operator Observable</returns>
        public static IObservable<TR> Systemize<T1, T2, TR>(this IObservable<T1> observable, ISystem<T2, TR> system,
            Func<T1, T2> selector) {
            return new SystemizeObservable<T2, TR>(observable.Select(selector), system);
        }

        /// <summary>
        ///     Combined Operator Select with Do
        /// <para/>
        ///     Selector will be called first
        /// </summary>
        /// <param name="observable">Any Observable</param>
        /// <param name="observer">Any Observer</param>
        /// <param name="selector">Selector</param>
        /// <typeparam name="T">Input type</typeparam>
        /// <typeparam name="TR">Output type</typeparam>
        /// <returns></returns>
        public static IObservable<TR> Do<T, TR>(this IObservable<T> observable, IObserver<TR> observer,
            Func<T, TR> selector) {
            return observable.Select(selector).Do(observer);
        }


        private class SystemizeObservable<T, TR> : OperatorObservableBase<TR> {
            private readonly IObservable<T> source;
            private readonly ISystem<T, TR> system;

            public SystemizeObservable(IObservable<T> source, ISystem<T, TR> system)
                : base(source.IsRequiredSubscribeOnCurrentThread()) {
                this.source = source;
                this.system = system;
            }

            protected override IDisposable SubscribeCore(IObserver<TR> observer, IDisposable cancel) {
                var observerOperator = new Systemize(this, observer, cancel);
                return new CompositeDisposable(observerOperator.SubscribeSystem(),
                    this.source.Subscribe(observerOperator));
            }

            private class Systemize : OperatorObserverBase<T, TR> {
                private readonly SystemizeObservable<T, TR> parent;

                public Systemize(SystemizeObservable<T, TR> parent, IObserver<TR> observer, IDisposable cancel)
                    : base(observer, cancel) {
                    this.parent = parent;
                }

                public IDisposable SubscribeSystem() {
                    return this.parent.system.Subscribe(this.observer);
                }

                public override void OnNext(T value) {
                    try {
                        this.parent.system.OnNext(value);
                    }
                    catch (Exception ex) {
                        try {
                            this.observer.OnError(ex);
                        }
                        finally {
                            this.Dispose();
                        }
                    }
                }

                public override void OnError(Exception error) {
                    try {
                        this.parent.system.OnError(error);
                    }
                    finally {
                        this.Dispose();
                    }
                }

                public override void OnCompleted() {
                    try {
                        this.parent.system.OnCompleted();
                    }
                    finally {
                        this.Dispose();
                    }
                }
            }
        }
    }
}

#endif