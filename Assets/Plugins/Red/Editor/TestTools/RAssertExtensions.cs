#if (CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))) && UNITY_EDITOR && NSUBSTITUTE
namespace Red.Editor.TestTools {
    using System.Linq;
    using NSubstitute;
    using NUnit.Framework;

    public static class RAssertExtensions {
        public static void AssertTotalReceivedCalls<T>(this T instance, int expected, int testingCalls = 0) where T : class {
            var calls        = instance.ReceivedCalls().ToList();
            var current      = calls.Count;
            var callsStrings = string.Join("\n    ", calls.Select(call => call.GetMethodInfo()));
            Assert.AreEqual(current, expected + testingCalls, 
                $"You should call {expected} times {typeof(T)}, but you call it {current - testingCalls} times with testing calls {testingCalls} \n    {callsStrings}");
        }
    }
}
#endif