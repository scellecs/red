#if (CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))) && UNITY_EDITOR
namespace Red.Editor.TestTools {
    using System;
    using JetBrains.Annotations;
    using NUnit.Framework;
    using NUnit.Framework.Constraints;
    using UnityEngine.Profiling;

    /// <summary>
    ///     <para>An NUnit test constraint class to test whether a given block of code makes GC allocations less than limit.</para>
    /// </summary>
    public class RAllocatingCountGCMemoryConstraint : Constraint {
        private readonly int maxLimit;

        public RAllocatingCountGCMemoryConstraint(int maxLimit) {
            this.maxLimit = maxLimit;
        }

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

            return new RAllocatingCountGCMemoryResult(this, original, recorder.sampleBlockCount, this.maxLimit);
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

        private class RAllocatingCountGCMemoryResult : ConstraintResult {
            private readonly int diff;
            private readonly int limit;

            public RAllocatingCountGCMemoryResult(IConstraint constraint, object actualValue, int diff, int maxLimit)
                : base(constraint, actualValue, diff <= maxLimit) {
                this.diff  = diff;
                this.limit = maxLimit;
            }

            public override void WriteMessageTo(MessageWriter writer) {
                if (this.diff == 0) {
                    writer.WriteMessageLine("The provided delegate did not make any GC allocations.");
                }
                else if (this.diff <= this.limit) {
                    writer.WriteMessageLine(
                        "The provided delegate make {0} GC allocations less or equal than limit {1}.",
                        this.diff, this.limit);
                }
                else {
                    writer.WriteMessageLine("The provided delegate made {0} GC allocation(s) great than limit {1}.",
                        this.diff, this.limit);
                }
            }
        }
    }

    public static class RConstraintExtensions {
        public static RAllocatingCountGCMemoryConstraint AllocatingCountGCMemory(this ConstraintExpression chain, int maxLimit) {
            var memoryConstraint = new RAllocatingCountGCMemoryConstraint(maxLimit);
            chain.Append(memoryConstraint);
            return memoryConstraint;
        }
    }
}
#endif