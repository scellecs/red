namespace Red.Editor.TestTools {
    using System;
    using JetBrains.Annotations;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using UnityEngine.Profiling;

    /// <summary>
    ///   <para>An NUnit test constraint class to test whether a given block of code makes any GC allocations.</para>
    /// </summary>
    public class AllocatingGCMemoryConstraint : Constraint {
        private int maxLimit;

        public AllocatingGCMemoryConstraint(int maxLimit) => this.maxLimit = maxLimit;

        private ConstraintResult ApplyTo(Action action, object original) {
            var recorder = Recorder.Get("GC.Alloc");
            recorder.enabled = false;
            recorder.FilterToCurrentThread();
            recorder.enabled = true;
            try {
                action();
            }
            finally {
                recorder.enabled = false;
                recorder.CollectFromAllThreads();
            }

            return new AllocatingGCMemoryResult(this, original, recorder.sampleBlockCount);
        }

        public override ConstraintResult ApplyTo([NotNull] object obj) {
            if (obj == null) {
                throw new ArgumentNullException();
            }

            if (!(obj is TestDelegate d)) {
                throw new ArgumentException(
                    $"The actual value must be a TestDelegate but was {(object) obj.GetType()}");
            }

            return this.ApplyTo(() => d(), obj);
        }


        public override ConstraintResult ApplyTo<TActual>([NotNull] ActualValueDelegate<TActual> del) {
            if (del == null) {
                throw new ArgumentNullException();
            }

            TActual actual;
            return this.ApplyTo(() => actual = del(), del);
        }

        public override string Description => "allocates GC memory";

        private class AllocatingGCMemoryResult : ConstraintResult {
            private readonly int diff;

            public AllocatingGCMemoryResult(IConstraint constraint, object actualValue, int diff)
                : base(constraint, actualValue, diff > 0) {
                this.diff = diff;
            }

            public override void WriteMessageTo(MessageWriter writer) {
                if (this.diff == 0)
                    writer.WriteMessageLine("The provided delegate did not make any GC allocations.");
                else
                    writer.WriteMessageLine("The provided delegate made {0} GC allocation(s).", (object) this.diff);
            }
        }
    }
}