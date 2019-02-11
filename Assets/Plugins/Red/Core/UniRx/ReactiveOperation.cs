#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace UniRx {
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public interface IReactiveOperation<T, TR> : IObservable<IOperationContext<T, TR>> {
        IObservable<TR> Execute(T parameter);
    }

    public interface IOperationContext<out T, in TR> : IObserver<TR> {
        T Parameter { get; }
    }

    /// <summary>
    ///     Utility class for <see cref="ReactiveOperation{T,TR}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TR"></typeparam>
    public class OperationContext<T, TR> : IOperationContext<T, TR>, IObservable<TR> {
        public T Parameter { get; }
        public CancellationDisposable Cancellation { get; } = new CancellationDisposable();

        private readonly Subject<TR>      subject;
        private readonly Queue<TR>        queue;
        private readonly Queue<Exception> queueExceptions;

        private bool isComplete;
        private int  countObservers;

        public OperationContext(T parameter) {
            this.Parameter = parameter;

            this.subject         = new Subject<TR>();
            this.queue           = new Queue<TR>();
            this.queueExceptions = new Queue<Exception>();

            this.isComplete     = false;
            this.countObservers = 0;
        }

        public IDisposable Subscribe(IObserver<TR> observer) {
            if (this.countObservers == 0) {
                foreach (var value in this.queue) {
                    observer.OnNext(value);
                }

                foreach (var exception in this.queueExceptions) {
                    observer.OnError(exception);
                }

                if (this.isComplete) {
                    observer.OnCompleted();
                }

                this.queue.Clear();
                this.queueExceptions.Clear();
            }

            Interlocked.Increment(ref this.countObservers);
            var decrement = Disposable.Create(() => { Interlocked.Decrement(ref this.countObservers); });

            var subscription = this.subject.Subscribe(observer);

            return new CompositeDisposable(subscription, decrement, this.Cancellation);
        }

        public void OnCompleted() {
            this.isComplete = true;
            this.subject.OnCompleted();
        }

        public void OnError(Exception error) {
            if (this.countObservers == 0) {
                this.queueExceptions.Enqueue(error);
            }

            this.subject.OnError(error);
        }

        public void OnNext(TR value) {
            if (this.countObservers == 0) {
                this.queue.Enqueue(value);
            }

            this.subject.OnNext(value);
        }
    }

    /// <summary>
    ///     Complex entity which returns Observable Operation at the moment execution
    /// </summary>
    /// <typeparam name="T">Input Type</typeparam>
    /// <typeparam name="TR">Return Type</typeparam>
    public class ReactiveOperation<T, TR> : IReactiveOperation<T, TR>, IDisposable {
        private readonly Subject<OperationContext<T, TR>> trigger = new Subject<OperationContext<T, TR>>();

        public IObservable<TR> Execute(T parameter) {
            var operation = new OperationContext<T, TR>(parameter);
            this.trigger.OnNext(operation);
            return operation;
        }

        public IDisposable Subscribe(IObserver<IOperationContext<T, TR>> observer) {
            return this.trigger.Subscribe(observer);
        }

        public void Dispose() {
            this.trigger?.Dispose();
        }
    }
}

#endif