#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using UnityEngine;

    //TODO implement ISchedulerPeriodic ISchedulerLongRunning
    public interface IObservableScheduler : 
        IObservable<Unit>,
        IScheduler,
        ISchedulerQueueing,
        IDisposable { }

    /// <summary>
    ///     Scheduler with manual publish all actions
    /// <para/>
    ///     All new actions inside <see cref="Publish" /> will add at the end of execution list
    /// <para/>
    ///     New actions will executed at current call <see cref="Publish" />
    /// </summary>
    public class RManualScheduler : IObservableScheduler {
        public DateTimeOffset Now => Scheduler.Now;
        protected readonly List<(DateTimeOffset time, Action action)> list
            = new List<(DateTimeOffset time, Action action)>();
        protected readonly List<(DateTimeOffset time, Action action)> removeList
            = new List<(DateTimeOffset time, Action action)>();

        protected readonly List<IHelper> helpers = new List<IHelper>();
        protected readonly Subject<Unit> subject = new Subject<Unit>();
        protected bool isDisposed;

        public IDisposable Schedule(Action action) {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Scheduler is disposed");
            }
            var temp = (DateTimeOffset.MinValue, action);
            this.list.Add(temp);
            return null;
        }

        public IDisposable Schedule(TimeSpan dueTime, Action action) {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Scheduler is disposed");
            }
            
            var time = Scheduler.Normalize(dueTime);
            var temp = (this.Now.Add(time), action);
            this.list.Add(temp);
            return null;
        }

        public virtual void Publish() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Scheduler is disposed");
            }
            
            this.subject.OnNext(Unit.Default);

            this.removeList.Clear();

            for (int i = 0; i < this.list.Count; i++) {
                var item = this.list[i];
                if (item.time <= this.Now) {
                    MainThreadDispatcher.UnsafeSend(item.action);
                    this.removeList.Add(item);
                }
            }

            this.removeList.ForEach(item => this.list.Remove(item));

            for (int i = 0; i < this.helpers.Count; i++) {
                var helper = this.helpers[i];
                helper.Publish();
            }
        }

        public void ScheduleQueueing<T>(ICancelable cancel, T state, Action<T> action)
            => this.GetHelper<T>().Schedule(action, state);

        protected Helper<T> GetHelper<T>() {
            var temp = this.helpers.FirstOrDefault(h => h is Helper<T>);
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
            private readonly List<(Action<T> action, T state)> list
                = new List<(Action<T> action, T state)>();

            public void Schedule(Action<T> action, T state) => this.list.Add((action, state));

            public void Publish() {
                for (int i = 0; i < this.list.Count; i++) {
                    var (action, state) = this.list[i];
                    MainThreadDispatcher.UnsafeSend(action, state);
                }

                this.list.Clear();
            }
        }

        public IDisposable Subscribe(IObserver<Unit> observer)
            => this.subject.Subscribe(observer);

        public void Dispose() {
            this.list.Clear();
            this.removeList.Clear();
            this.helpers.Clear();
            this.subject.Dispose();

            this.isDisposed = true;
        }
    }

    /// <summary>
    ///     Scheduler with manual publish all actions
    /// <para/>
    ///     All new actions inside <see cref="Publish" /> will add at the end of execution list
    /// <para/>
    ///     New actions will executed at next call <see cref="Publish" />
    /// </summary>
    public class RManualSchedulerLocked : RManualScheduler {
        public override void Publish() {
            if (this.isDisposed) {
                throw new ObjectDisposedException("Scheduler is disposed");
            }
            
            this.subject.OnNext(Unit.Default);

            this.removeList.Clear();

            var listCountLock = this.list.Count;
            for (int i = 0; i < listCountLock; i++) {
                var item = this.list[i];
                if (item.time <= this.Now) {
                    MainThreadDispatcher.UnsafeSend(item.action);
                    this.removeList.Add(item);
                }
            }

            this.removeList.ForEach(item => this.list.Remove(item));

            var helpersCountLock = this.helpers.Count;
            for (int i = 0; i < helpersCountLock; i++) {
                var helper = this.helpers[i];
                helper.Publish();
            }
        }
    }
}
#endif