#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red.Pure {
    using System;
    using UniRx;
    using UniRx.Operators;

    internal class RSystemizeObservable<T, TR> : OperatorObservableBase<TR> {
        private readonly IObservable<T> source;
        private readonly ISystem<T, TR> system;

        public RSystemizeObservable(IObservable<T> source, ISystem<T, TR> system)
            : base(source.IsRequiredSubscribeOnCurrentThread()) {
            this.source = source;
            this.system = system;
        }

        protected override IDisposable SubscribeCore(IObserver<TR> observer, IDisposable cancel) {
            var observerOperator = new RSystemize(this, observer, cancel);
            return new CompositeDisposable(observerOperator.SubscribeSystem(),
                this.source.Subscribe(observerOperator));
        }

        private class RSystemize : OperatorObserverBase<T, TR> {
            private readonly RSystemizeObservable<T, TR> parent;

            public RSystemize(RSystemizeObservable<T, TR> parent, IObserver<TR> observer, IDisposable cancel)
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
#endif