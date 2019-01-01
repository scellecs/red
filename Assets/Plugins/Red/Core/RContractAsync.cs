#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red {
    using System;
    using UniRx;
    using UniRx.Async;

    public abstract class RContractAsync<T> : RContract<T>, IObservable<T> where T : RContractAsync<T>, new() {
        private readonly AsyncSubject<T> initialized = new AsyncSubject<T>();

        protected sealed override async void Initialize() {
            await this.InitializeAsync();
            this.initialized.OnNext((T) this);
            this.initialized.OnCompleted();
        }

        protected abstract UniTask InitializeAsync();

        public IDisposable Subscribe(IObserver<T> observer) {
            return this.initialized.Subscribe(observer);
        }

        public override void Dispose() {
            base.Dispose();
            this.initialized.Dispose();
        }
    }
}
#endif