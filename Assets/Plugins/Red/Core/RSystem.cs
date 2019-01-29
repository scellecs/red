namespace Red {
    using System;
    using JetBrains.Annotations;
    using UniRx;

    public interface ISystem<in T,out TR> : IObserver<T>, IObservable<TR> {
        
    }

    public abstract class RSystem<T, TR> : ISystem<T, TR> {
        protected Subject<TR> Subject = new Subject<TR>();
        
        public virtual void OnCompleted() {
            this.Subject.OnCompleted();
        }

        public virtual void OnError(Exception error) {
            this.Subject.OnError(error);
        }

        public abstract void OnNext(T value);

        public IDisposable Subscribe(IObserver<TR> observer) {
            return this.Subject.Subscribe(observer);
        }
    }
}