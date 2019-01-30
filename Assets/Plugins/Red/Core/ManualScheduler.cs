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
            this.list.Where(i => i.time <= this.Now).ForEach(i => MainThreadDispatcher.Post(this.scheduleAction, (i.action, i.d)));
            this.list.RemoveAll(i => i.time <= this.Now);
        }
    }
}