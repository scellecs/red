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

        CancellationDisposable Cancellation { get; }
    }

    /// <summary>
    ///     Utility class for <see cref="ReactiveOperation{T,TR}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TR"></typeparam>
    public class OperationContext<T, TR> : IOperationContext<T, TR>, IObservable<TR> {
        public T                      Parameter    { get; }
        public CancellationDisposable Cancellation { get; }

        private readonly Subject<TR>      subject;

        private TR               singleValue;
        private Queue<TR>        queueValues;
        private Exception        singleException;
        private Queue<Exception> queueExceptions;

        private bool haveSingleValue;
        private bool isComplete;
        private int  countObservers;

        public OperationContext(T parameter) {
            this.Parameter    = parameter;
            this.Cancellation = new CancellationDisposable();

            this.subject         = new Subject<TR>();

            this.haveSingleValue = false;
            this.isComplete       = false;
            this.countObservers   = 0;
        }

        public IDisposable Subscribe(IObserver<TR> observer) {
            if (this.countObservers == 0) {
                if (queueValues != null) {
                    foreach (var value in this.queueValues) {
                        observer.OnNext(value);
                    }
                } else if (this.haveSingleValue) {
                    observer.OnNext(this.singleValue);                    
                }

                if (this.queueExceptions != null) {
                    foreach (var exception in this.queueExceptions) {
                        observer.OnError(exception);
                    }
                } else if (this.singleException != null) {
                    observer.OnError(this.singleException);                    
                }

                if (this.isComplete) {
                    observer.OnCompleted();
                }

                this.queueValues?.Clear();
                this.queueExceptions?.Clear();
                this.haveSingleValue = false;
                this.singleException = null;
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
                if (this.queueExceptions != null) {
                    this.queueExceptions.Enqueue(error);
                }
                else {
                    if (this.singleException == null) {
                        this.singleException = error;
                    } else {
                        this.queueExceptions = new Queue<Exception>();
                        this.queueExceptions.Enqueue(singleException);
                        this.queueExceptions.Enqueue(error);
                        this.singleException = null;
                    }
                }
            }

            this.subject.OnError(error);
        }

        public void OnNext(TR value) {
            if (this.countObservers == 0) {
                if (this.queueValues != null) {
                    this.queueValues.Enqueue(value);
                }
                else {
                    if (this.haveSingleValue == false) {
                        this.singleValue = value;
                        this.haveSingleValue = true;
                    }
                    else {
                        this.queueValues = new Queue<TR>();
                        this.queueValues.Enqueue(singleValue);
                        this.queueValues.Enqueue(value);
                        this.haveSingleValue = false;
                    }
                }
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


        /// <summary>
        ///     Push parameter and return IObservable{TR}
        ///     <para/>14 allocations on call
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public IObservable<TR> Execute(T parameter) {
            var operation = new OperationContext<T, TR>(parameter);
            this.trigger.OnNext(operation);
            return operation;
        }

        /// <summary>
        ///     <para/>18 allocations on call
        /// </summary>
        /// <param name="observer"></param>
        /// <returns></returns>
        public IDisposable Subscribe(IObserver<IOperationContext<T, TR>> observer) {
            return this.trigger.Subscribe(observer);
        }

        /// <summary>
        ///     <para/>2 allocations on call
        /// </summary>
        public void Dispose() {
            this.trigger?.Dispose();
        }
    }
}

#endif