#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red {
    using System;
    using System.Collections.Generic;
    using UniRx.Async;

    public static partial class RLinqExtension {
        /// <summary>
        ///     Default iteration in functional style.
        /// </summary>
        /// <param name="source">Some Enumerable</param>
        /// <param name="action">Functor</param>
        /// <typeparam name="T">Type of Enumerable</typeparam>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action) {
            foreach (var element in source) {
                action(element);
            }
        }

        /// <summary>
        ///     Sequential iteration for async API.
        /// </summary>
        /// <param name="source">Some Enumerable</param>
        /// <param name="action">Functor</param>
        /// <typeparam name="T">Type of Enumerable</typeparam>
        public static async UniTask ForEachAsync<T>(this IEnumerable<T> source, Func<T, UniTask> action) {
            foreach (var element in source) {
                await action(element);
            }
        }
    }
}
#endif