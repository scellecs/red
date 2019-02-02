namespace Red {
    using System;
    using JetBrains.Annotations;
    using UniRx;

    public interface ISystem<in T, out TR> : IObserver<T>, IObservable<TR> {
    }

    public abstract class RSystem<T, TR> : ISystem<T, TR> {
        private readonly Subject<TR> subject = new Subject<TR>();

        public virtual void OnCompleted() {
            this.subject.OnCompleted();
        }

        public virtual void OnError(Exception error) {
            this.subject.OnError(error);
        }

        public abstract void OnNext(T value);

        protected void Next(TR value) => this.subject.OnNext(value);

        protected void Error(Exception e) => this.subject.OnError(e);

        protected void Complete() => this.subject.OnCompleted();

        public IDisposable Subscribe(IObserver<TR> observer) {
            return this.subject.Subscribe(observer);
        }
    }
}