#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red.Pure {
    using System;
    using UniRx;

    public interface ISystem<in T, out TR> : IObserver<T>, IObservable<TR> {
    }

    public abstract class RSystem<T, TR> : ISystem<T, TR> {
        private readonly Subject<TR> subject = new Subject<TR>();

        public virtual void OnCompleted() => this.subject.OnCompleted();

        public virtual void OnError(Exception error) => this.subject.OnError(error);

        public abstract void OnNext(T value);

        protected void Next(TR value) => this.subject.OnNext(value);

        protected void Error(Exception e) => this.subject.OnError(e);

        protected void Complete() => this.subject.OnCompleted();

        public IDisposable Subscribe(IObserver<TR> observer) => this.subject.Subscribe(observer);
    }
}
#endif