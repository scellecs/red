namespace Red {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UniRx;

    public class ManualScheduler : IScheduler {
        public DateTimeOffset Now => Scheduler.Now;
        private List<(DateTimeOffset time, Action action, BooleanDisposable d)> list
            = new List<(DateTimeOffset time, Action action, BooleanDisposable d)>();

        private readonly Action<object> scheduleAction;

        public ManualScheduler() {
            this.scheduleAction = this.Schedule;
        }

        private void Schedule(object state) {
            var t = ((Action action, BooleanDisposable d)) state;
            if (!t.d.IsDisposed) {
                t.action();
            }
        }

        public IDisposable Schedule(Action action) {
            var temp = (DateTimeOffset.MinValue, action, new BooleanDisposable());
            this.list.Add(temp);
            return new CompositeDisposable(Disposable.Create(() => this.list.Remove(temp)), temp.Item3);
        }

        public IDisposable Schedule(TimeSpan dueTime, Action action) {
            var time = Scheduler.Normalize(dueTime);
            var temp = (this.Now.Add(time), action, new BooleanDisposable());
            this.list.Add(temp);
            return new CompositeDisposable(Disposable.Create(() => this.list.Remove(temp)), temp.Item3);
        }

        public void Publish() {
            this.list.Where(i => i.time <= this.Now)
                .ForEach(i => MainThreadDispatcher.Post(this.scheduleAction, (i.action, i.d)));
            this.list.RemoveAll(i => i.time <= this.Now);
        }
    }

    public class ManualSchedulerNonAlloc : IScheduler, ISchedulerQueueing {
        public DateTimeOffset Now => Scheduler.Now;
        private List<(DateTimeOffset time, Action action)> list
            = new List<(DateTimeOffset time, Action action)>();

        private void Schedule(object state) {
            var t = (Action) state;
            t();
        }

        public IDisposable Schedule(Action action) {
            var temp = (DateTimeOffset.MinValue, action);
            this.list.Add(temp);
            return null;
        }

        public IDisposable Schedule(TimeSpan dueTime, Action action) {
            var time = Scheduler.Normalize(dueTime);
            var temp = (this.Now.Add(time), action);
            this.list.Add(temp);
            return null;
        }

        private readonly List<(DateTimeOffset time, Action action)> tempList 
            = new List<(DateTimeOffset time, Action action)>();

        public void Publish() {
            this.tempList.Clear();

            for (int i = 0; i < this.list.Count; i++) {
                var item = this.list[i];
                if (item.time <= this.Now) {
                    MainThreadDispatcher.UnsafeSend(item.action);
                    this.tempList.Add(item);
                }
            }

            this.tempList.ForEach(item => this.list.Remove(item));
            
            this.helpers.ForEach(h => h.Publish());
            this.helpers.AddRange(this.tempHelpers);
            this.tempHelpers.Clear();
        }

        public void ScheduleQueueing<T>(ICancelable cancel, T state, Action<T> action) {
            this.GetHelper<T>().Schedule(action, state);
        }

        private readonly List<IHelper> helpers = new List<IHelper>();
        private readonly List<IHelper> tempHelpers = new List<IHelper>();
        private Helper<T> GetHelper<T>() {
            var temp = this.helpers.FirstOrDefault(h => h is T);
            if (temp == null) {
                temp = new Helper<T>(this);
                this.tempHelpers.Add(temp);
            }

            return (Helper<T>)temp;
        }

        private interface IHelper {
            void Publish();
        }
        
        private class Helper<T> : IHelper {
            private List<(DateTimeOffset time, Action<T> action, T state)> list
                = new List<(DateTimeOffset time, Action<T> action, T state)>();
            
            private readonly List<(DateTimeOffset time, Action<T> action, T state)> tempList 
                = new List<(DateTimeOffset time, Action<T> action, T state)>();

            private ManualSchedulerNonAlloc parent;
            public Helper(ManualSchedulerNonAlloc parent) {
                this.parent = parent;
            }

            public void Schedule(Action<T> action, T state) {
                this.list.Add((DateTimeOffset.MinValue, action, state));
            }

            public void Publish() {
                this.tempList.Clear();

                for (int i = 0; i < this.list.Count; i++) {
                    var item = this.list[i];
                    if (item.time <= this.parent.Now) {
                        MainThreadDispatcher.UnsafeSend(item.action, item.state);
                        this.tempList.Add(item);
                    }
                }

                this.tempList.ForEach(item => this.list.Remove(item));
            }
        }
    }
}