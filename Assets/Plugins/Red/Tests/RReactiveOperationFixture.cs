#if (CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))) && UNITY_EDITOR
namespace Red.Tests {
    using System;
    using System.Collections;
    using Editor.TestTools;
    using NUnit.Framework;
    using UniRx;
    using UnityEngine;
    using UnityEngine.TestTools;

    [TestFixture]
    public class RReactiveOperationFixture {
        private ReactiveOperation<float, float> reactiveOperation;

        [SetUp]
        public void Setup() {
            this.reactiveOperation = new ReactiveOperation<float, float>();
        }

        [TearDown]
        public void TearDown() {
            this.reactiveOperation.Dispose();
        }


        [Test(
            Author      = "Oleg Morozov",
            Description = "Execute and receive OnNext and OnComplete",
            TestOf      = typeof(ReactiveOperation<,>))]
        [TestCase(0, 10.5f)]
        [TestCase(-100, -100)]
        [TestCase(100, 100)]
        [Category("Unit")]
        public void ReactiveOperation_Execute_GotOnNextAndOnComplete(float post, float expected) {
            this.reactiveOperation.Subscribe(ctx => {
                Assert.AreEqual(post, ctx.Parameter);

                ctx.OnNext(expected);
                ctx.OnCompleted();
            });

            var current     = -99f;
            var isCompleted = false;

            this.reactiveOperation
                .Execute(post)
                .Subscribe(value => current = value, () => isCompleted = true);

            Assert.AreEqual(expected, current);
            Assert.IsTrue(isCompleted);
        }

        [UnityTest]
        [Category("Unit")]
        public IEnumerator ReactiveOperation_Execute_UnscribeAfterExecute() {
            this.reactiveOperation.Subscribe(async ctx => {
                for (float i = 0; i < 10; i++) {
                    ctx.OnNext(i);
                    await Observable.NextFrame();
                }

                ctx.OnCompleted();
            });

            var current     = -99f;
            var isCompleted = false;

            var disposable = this.reactiveOperation
                .Execute(0f)
                .Subscribe(value => current = value, () => isCompleted = true);

            disposable.Dispose();

            //Fix synchronization context
            for (int i = 0; i < 10; i++) {
                yield return null;
            }

            Assert.AreEqual(0f, current);
        }


        [UnityTest]
        [Category("Unit")]
        public IEnumerator ReactiveOperation_Execute_BreakAwaitersInSubscriber() {
            Exception error = null;
            this.reactiveOperation.Subscribe(async ctx => {
                try {
                    await TimeSpan.FromSeconds(10).GetAwaiter(ctx.Cancellation.Token);

                    ctx.OnNext(0f);
                    ctx.OnCompleted();
                }
                catch (Exception e) {
                    error = e;
                }
            });

            var disposable = this.reactiveOperation
                .Execute(0f)
                .Subscribe();

            disposable.Dispose();

            //Fix synchronization context
            for (int i = 0; i < 10; i++) {
                yield return null;
            }

            Assert.NotNull(error);
            Assert.That(() => error is OperationCanceledException);
        }
    }

    [TestFixture]
    public class RReactiveOperationAllocationFixture {
        private ReactiveOperation<float, float> reactiveOperation;

        [SetUp]
        public void Setup() {
            this.reactiveOperation = new ReactiveOperation<float, float>();
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
        public void ReactiveOperation_Execute_23Alloc() {
            this.reactiveOperation.Subscribe(ctx => {
                ctx.OnNext(0f);
                ctx.OnCompleted();
            });

            void ExecuteReactiveOperation() {
                this.reactiveOperation.Execute(0f);
            }

            Assert.That(ExecuteReactiveOperation, new RAllocatingCountGCMemoryConstraint(23));
        }

        [Test(
            Author      = "Oleg Morozov",
            Description = "Subscribe produced 18 allocations",
            TestOf      = typeof(ReactiveOperation<,>))]
        [Category("Allocations")]
        public void ReactiveOperation_Subscribe_18Alloc() {
            Action<IOperationContext<float, float>> action = ctx => {
                ctx.OnNext(0f);
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
        public void ReactiveOperation_Dispose_2Alloc() {
            this.reactiveOperation.Subscribe(ctx => {
                ctx.OnNext(0f);
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