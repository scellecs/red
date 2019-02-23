#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red.Pure {
    using System;
    using UniRx;

    public static partial class RSystemizeExtensions {
        /// <summary>
        ///     Complex operator, which allows process stream with custom <see cref="ISystem{T, TR}" />
        /// </summary>
        /// <param name="observable">Any Observable</param>
        /// <param name="system">System which receive <typeparamref name="T" /> and push forward <typeparamref name="TR" /></param>
        /// <typeparam name="T">Input type</typeparam>
        /// <typeparam name="TR">Output type</typeparam>
        /// <returns>New Operator Observable</returns>
        public static IObservable<TR> Systemize<T, TR>(this IObservable<T> observable, ISystem<T, TR> system) {
            return new RSystemizeObservable<T, TR>(observable, system);
        }


        /// <summary>
        ///     Complex operator, which allows process stream with custom <see cref="ISystem{T, TR}" />
        /// <para/>
        ///     Selector will be called before system
        /// </summary>
        /// <param name="observable">Any Observable</param>
        /// <param name="system">System which receive <typeparamref name="T2" /> and push forward <typeparamref name="TR" /></param>
        /// <param name="selector">Selector</param>
        /// <typeparam name="T1">Input type</typeparam>
        /// <typeparam name="T2">Selected type</typeparam>
        /// <typeparam name="TR">Output type</typeparam>
        /// <returns>New Operator Observable</returns>
        public static IObservable<TR> Systemize<T1, T2, TR>(this IObservable<T1> observable, ISystem<T2, TR> system,
            Func<T1, T2> selector) {
            return new RSystemizeObservable<T2, TR>(observable.Select(selector), system);
        }

        /// <summary>
        ///     Combined Operator Select with Do
        /// <para/>
        ///     Selector will be called first
        /// </summary>
        /// <param name="observable">Any Observable</param>
        /// <param name="observer">Any Observer</param>
        /// <param name="selector">Selector</param>
        /// <typeparam name="T">Input type</typeparam>
        /// <typeparam name="TR">Output type</typeparam>
        /// <returns></returns>
        public static IObservable<TR> Do<T, TR>(this IObservable<T> observable, IObserver<TR> observer,
            Func<T, TR> selector) {
            return observable.Select(selector).Do(observer);
        }
    }
}

#endif