namespace Red.Tests {
    using System;
    using System.Collections.Generic;
    using Editor.TestTools;
    using NSubstitute;
    using NUnit.Framework;
    using UniRx;

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
                new List<object>(100) {
                };
            }
            
            Assert.That(ExecuteReactiveOperation, new AllocatingCountGCMemoryConstraint(1));
        }

        [Test]
        public void SettingAVariableDoesNotAllocate() {
            void TestCase() {
                int a = 0;
                a = 1;
            }

            Assert.That(TestCase, new AllocatingCountGCMemoryConstraint(0));
        }
    }
}