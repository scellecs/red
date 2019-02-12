namespace Red.Tests {
    using System;
    using System.Collections;
    using NUnit.Framework;
    using UnityEngine.TestTools;
    using NSubstitute;
    using UniRx;
    using UnityEngine.TestTools.Constraints;
    using UnityIs = UnityEngine.TestTools.Constraints.Is;

    public class ReactiveOperationTests {
        [SetUp]
        public void Setup() {
        }

        [TearDown]
        public void TearDown() {
        }

        [Test]
        public void ReactiveOperation_Execute_NonAlloc() {
            var reactiveOperation = new ReactiveOperation<Unit, Unit>();
            var currentOperation = reactiveOperation.Execute(Unit.Default);

            void ExecuteReactiveOperation() {
                //currentOperation.;
            }
            
            Assert.That(ExecuteReactiveOperation, UnityIs.Not.AllocatingGCMemory());
        }

        [Test]
        public void SettingAVariableDoesNotAllocate() {
            void TestCase() {
                int a = 0;
                a = 1;
            }

            Assert.That(TestCase, UnityIs.Not.AllocatingGCMemory());
        }
    }
}