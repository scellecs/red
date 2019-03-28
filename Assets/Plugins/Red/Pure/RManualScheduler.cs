#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red.Pure {
    using System;
    using System.Collections.Generic;
    using UniRx;
    using UnityEngine;

    public interface IManualObservableScheduler : IObservableScheduler {
        bool CanDispatch();
        void Dispatch();
    }
    
    //TODO implement ISchedulerPeriodic ISchedulerLongRunning
    public interface IObservableScheduler :
        IObservable<Unit>,
        IScheduler,
        ISchedulerQueueing,
        IDisposable {
    }

    /// <summary>
    ///     Scheduler with manual publish all actions
    /// <para/>
    ///     All new actions inside <see cref="Dispatch" /> will add at the end of execution list
    /// <para/>
    ///     New actions will executed at current call <see cref="Dispatch" />
    /// </summary>
#if ZENJECT
    [Zenject.ZenjectAllowDuringValidation]
#endif
    public class RManualScheduler : IManualObservableScheduler {
        public DateTimeOffset Now => Scheduler.Now;
        protected readonly List<(DateTimeOffset time, Action action)> list
            = new List<(DateTimeOffset time, Action action)>();
        protected readonly List<(DateTimeOffset time, Action action)> removeList
            = new List<(DateTimeOffset time, Action action)>();

        protected readonly List<IHelper> helpers = new List<IHelper>();
        protected readonly Subject<Unit> subject = new Subject<Unit>();
        protected bool isDisposed;

        public virtual IDisposable Schedule(Action action) {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Scheduler is disposed");
            }
            var temp = (DateTimeOffset.MinValue, action);
            this.list.Add(temp);
            return null;
        }

        public virtual IDisposable Schedule(TimeSpan dueTime, Action action) {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Scheduler is disposed");
            }
            
            var time = Scheduler.Normalize(dueTime);
            var temp = (this.Now.Add(time), action);
            this.list.Add(temp);
            return null;
        }

        public virtual bool CanDispatch() => !this.isDisposed;

        public virtual void Dispatch() {
            if (this.CanDispatch() == false) {
                return;
            }

            try {
                this.subject.OnNext(Unit.Default);
            }
            catch (Exception e) {
                Debug.LogException(e);
                this.subject.OnError(e);
            }

            this.removeList.Clear();

            for (int i = 0; i < this.list.Count; i++) {
                var item = this.list[i];
                if (item.time <= this.Now) {
                    MainThreadDispatcher.UnsafeSend(item.action);
                    this.removeList.Add(item);
                }
            }

            for (var i = 0; i < this.removeList.Count; i++) {
                var item = this.removeList[i];
                this.list.Remove(item);
            }

            for (int i = 0; i < this.helpers.Count; i++) {
                var helper = this.helpers[i];
                helper.Publish();
            }
        }

        public virtual void ScheduleQueueing<T>(ICancelable cancel, T state, Action<T> action) {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Scheduler is disposed");
            }
            
            this.GetHelper<T>().Schedule(cancel, action, state);
        }


        public virtual IDisposable Subscribe(IObserver<Unit> observer) {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Scheduler is disposed");
            }
            
            return this.subject.Subscribe(observer);
        }

        public virtual void Dispose() {
            this.list.Clear();
            this.removeList.Clear();
            this.helpers.Clear();
            this.subject.Dispose();

            this.isDisposed = true;
        }
        
        protected Helper<T> GetHelper<T>() {
            IHelper temp = null;
            for (var index = 0; index < this.helpers.Count; index++) {
                var h = this.helpers[index];
                if (h is Helper<T>) {
                    temp = h;
                    break;
                }
            }

            if (temp == null) {
                temp = new Helper<T>();
                this.helpers.Add(temp);
            }

            return (Helper<T>) temp;
        }

        protected interface IHelper {
            void Publish();
        }

        protected class Helper<T> : IHelper {
            private readonly List<(ICancelable cancel, Action<T> action, T state)> list
                = new List<(ICancelable cancel, Action<T> action, T state)>();

            public void Schedule(ICancelable cancel, Action<T> action, T state) 
                => this.list.Add((cancel, action, state));

            public void Publish() {
                for (int i = 0; i < this.list.Count; i++) {
                    var (cancel, action, state) = this.list[i];
                    if (cancel.IsDisposed) {
                        continue;
                    }
                    MainThreadDispatcher.UnsafeSend(action, state);
                }

                this.list.Clear();
            }
        }
    }

    /// <summary>
    ///     Scheduler with manual publish all actions
    /// <para/>
    ///     All new actions inside <see cref="Publish" /> will add at the end of execution list
    /// <para/>
    ///     New actions will executed at next call <see cref="Publish" />
    /// </summary>
#if ZENJECT
    [Zenject.ZenjectAllowDuringValidation]
#endif
    public class RManualSchedulerLocked : RManualScheduler {
        public override void Dispatch() {
            if (this.CanDispatch() == false) {
                return;
            }
            
            try {
                this.subject.OnNext(Unit.Default);
            }
            catch (Exception e) {
                Debug.LogException(e);
                this.subject.OnError(e);
            }

            this.removeList.Clear();

            var listCountLock = this.list.Count;
            for (int i = 0; i < listCountLock; i++) {
                var item = this.list[i];
                if (item.time <= this.Now) {
                    MainThreadDispatcher.UnsafeSend(item.action);
                    this.removeList.Add(item);
                }
            }

            for (var i = 0; i < this.removeList.Count; i++) {
                var item = this.removeList[i];
                this.list.Remove(item);
            }

            var helpersCountLock = this.helpers.Count;
            for (int i = 0; i < helpersCountLock; i++) {
                var helper = this.helpers[i];
                helper.Publish();
            }
        }
    }
}
#endif