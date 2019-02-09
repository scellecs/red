#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;
    using UnityEngine;

    /// <summary>
    ///     Scheduler with manual publish all actions
    /// <para/>
    ///     All new actions inside <see cref="Publish" /> will add at the end of execution list
    /// <para/>
    ///     New actions will executed at current call <see cref="Publish" />
    /// </summary>
    public class RManualScheduler : IScheduler, ISchedulerQueueing {
        public DateTimeOffset Now => Scheduler.Now;
        protected readonly List<(DateTimeOffset time, Action action)> List
            = new List<(DateTimeOffset time, Action action)>();
        protected readonly List<(DateTimeOffset time, Action action)> RemoveList
            = new List<(DateTimeOffset time, Action action)>();

        protected readonly List<IHelper> Helpers     = new List<IHelper>();

        public IDisposable Schedule(Action action) {
            var temp = (DateTimeOffset.MinValue, action);
            this.List.Add(temp);
            return null;
        }

        public IDisposable Schedule(TimeSpan dueTime, Action action) {
            var time = Scheduler.Normalize(dueTime);
            var temp = (this.Now.Add(time), action);
            this.List.Add(temp);
            return null;
        }

        public virtual void Publish() {
            this.RemoveList.Clear();

            for (int i = 0; i < this.List.Count; i++) {
                var item = this.List[i];
                if (item.time <= this.Now) {
                    MainThreadDispatcher.UnsafeSend(item.action);
                    this.RemoveList.Add(item);
                }
            }

            this.RemoveList.ForEach(item => this.List.Remove(item));

            for (int i = 0; i < this.Helpers.Count; i++) {
                var helper = this.Helpers[i];
                helper.Publish();
            }
        }

        public void ScheduleQueueing<T>(ICancelable cancel, T state, Action<T> action) 
            => this.GetHelper<T>().Schedule(action, state);

        protected Helper<T> GetHelper<T>() {
            var temp = this.Helpers.FirstOrDefault(h => h is Helper<T>);
            if (temp == null) {
                temp = new Helper<T>();
                this.Helpers.Add(temp);
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
            this.RemoveList.Clear();

            var listCountLock = this.List.Count;
            for (int i = 0; i < listCountLock; i++) {
                var item = this.List[i];
                if (item.time <= this.Now) {
                    MainThreadDispatcher.UnsafeSend(item.action);
                    this.RemoveList.Add(item);
                }
            }

            this.RemoveList.ForEach(item => this.List.Remove(item));

            var helpersCountLock = this.Helpers.Count;
            for (int i = 0; i < helpersCountLock; i++) {
                var helper = this.Helpers[i];
                helper.Publish();
            }
        }
    }
}
#endif