#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red {
    using System;
    using JetBrains.Annotations;
    using UniRx;
    using UnityEngine;

    public static partial class RContractExtensions {
        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or null if it doesn't exists for current object
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="identifier">Unique identifier for contract</param>
        [CanBeNull]
        public static T TryGet<T>(this Component component, string identifier = "") where T : RContract<T>, new() {
            return RContract<T>.TryGet(component.gameObject, identifier);
        }

        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or null if it doesn't exists for current object
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="identifier">Unique identifier for contract</param>
        [CanBeNull]
        public static T TryGet<T>(this GameObject gameObject, string identifier = "") where T : RContract<T>, new() {
            return RContract<T>.TryGet(gameObject, identifier);
        }

        /// <summary>
        ///     Search for the instance of <see cref="RContract{T0}" />
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="contract">Instance of <see cref="RContract{T0}" /> or null</param>
        /// <param name="identifier">Unique identifier for contract</param>
        /// <returns>True if found, false if instance is null</returns>
        public static bool TryGet<T>(this GameObject gameObject, [CanBeNull] out T contract, string identifier = "")
            where T : RContract<T>, new() {
            contract = RContract<T>.TryGet(gameObject, identifier);
            return contract != null;
        }

        /// <summary>
        ///     Search for the instance of <see cref="RContract{T0}" />
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="contract">Instance of <see cref="RContract{T0}" /> or null</param>
        /// <param name="identifier">Unique identifier for contract</param>
        /// <returns>True if found, false if instance is null</returns>
        public static bool TryGet<T>(this Component component, [CanBeNull] out T contract, string identifier = "")
            where T : RContract<T>, new() {
            contract = RContract<T>.TryGet(component.gameObject, identifier);
            return contract != null;
        }

        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or create it, if it doesn't exists on current gameObject
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="component">The component from which the gameObject is taken</param>
        /// <param name="identifier">Unique identifier for contract</param>
        public static T GetOrCreate<T>(this Component component, string identifier = "") where T : RContract<T>, new() {
            return RContract<T>.GetOrCreate(component.gameObject, identifier);
        }

        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or create it, if it doesn't exists on current gameObject
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <param name="gameObject">The gameObject that acts as an anchor</param>
        /// <param name="identifier">Unique identifier for contract</param>
        public static T GetOrCreate<T>(this GameObject gameObject, string identifier = "")
            where T : RContract<T>, new() {
            return RContract<T>.GetOrCreate(gameObject, identifier);
        }

        /// <summary>
        ///     Register contract in container
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        public static void RegisterIn<T>(this T contract, RContainer container) where T : RContract {
            container.Register(contract);
        }
        
        
        /// <summary>
        ///     Add contract to GameObject. Contract will be disposed when GameObject is destroyed.
        /// </summary>
        /// <param name="disposable"></param>
        /// <param name="contract"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T AddTo<T>(this T disposable, RContract contract) where T : IDisposable {
            if (contract?.Target is GameObject o) {
                disposable.AddTo(o);
                return disposable;
            }

            disposable.Dispose();
            return disposable;
        }
    }
}
#endif