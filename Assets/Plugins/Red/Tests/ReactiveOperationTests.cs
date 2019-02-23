#if (CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))) && UNITY_EDITOR && NSUBSTITUTE
namespace Red.Tests {
    using System;
    using Editor.TestTools;
    using NUnit.Framework;
    using UniRx;

    public class ReactiveOperationTests {
        private ReactiveOperation<Unit, Unit> reactiveOperation;

        [SetUp]
        public void Setup() {
            this.reactiveOperation = new ReactiveOperation<Unit, Unit>();
        }

        [TearDown]
        public void TearDown() {
            this.reactiveOperation.Dispose();
        }

        [Test(
            Author      = "Oleg Morozov",
            Description = "Execute produced 23 allocations",
            TestOf      = typeof(ReactiveOperation<,>))]
        [Category("Allocations")]
        public void _0_ReactiveOperation_Execute_23Alloc() {
            this.reactiveOperation.Subscribe(ctx => {
                ctx.OnNext(Unit.Default);
                ctx.OnCompleted();
            });

            void ExecuteReactiveOperation() {
                this.reactiveOperation.Execute(Unit.Default);
            }

            Assert.That(ExecuteReactiveOperation, new RAllocatingCountGCMemoryConstraint(23));
        }

        [Test(
            Author      = "Oleg Morozov",
            Description = "Subscribe produced 18 allocations",
            TestOf      = typeof(ReactiveOperation<,>))]
        [Category("Allocations")]
        public void _1_ReactiveOperation_Subscribe_18Alloc() {
            Action<IOperationContext<Unit, Unit>> action = ctx => {
                ctx.OnNext(Unit.Default);
                ctx.OnCompleted();
            };

            void ExecuteReactiveOperation() {
                this.reactiveOperation.Subscribe(action);
            }

            Assert.That(ExecuteReactiveOperation, new RAllocatingCountGCMemoryConstraint(18));
        }

        [Test(
            Author      = "Oleg Morozov",
            Description = "Dispose produced 2 allocations",
            TestOf      = typeof(ReactiveOperation<,>))]
        [Category("Allocations")]
        public void _2_ReactiveOperation_Dispose_2Alloc() {
            this.reactiveOperation.Subscribe(ctx => {
                ctx.OnNext(Unit.Default);
                ctx.OnCompleted();
            });

            void ExecuteReactiveOperation() {
                this.reactiveOperation.Dispose();
            }

            Assert.That(ExecuteReactiveOperation, new RAllocatingCountGCMemoryConstraint(2));
        }
    }
}
#endif