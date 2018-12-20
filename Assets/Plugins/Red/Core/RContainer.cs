#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using UniRx;

    /// <inheritdoc />
    /// <summary>
    /// Container for resolving global contract references.
    /// </summary>
    public sealed class RContainer : IDisposable {
        private readonly Dictionary<Type, RContract> contracts = new Dictionary<Type, RContract>();
        private readonly Subject<RContract> newContract = new Subject<RContract>();

        /// <summary>
        /// Registrate contract in single instance.
        /// If contract exist throw <see cref="InvalidOperationException"/>
        /// </summary>
        /// <param name="contract">Instance contract for registration</param>
        /// <returns>Return <see cref="IDisposable"/> for unscribing</returns>
        public IDisposable Register(RContract contract) {
            if (contract == null) throw new ArgumentNullException(nameof(contract));
            
            return this.RegisterLocal(contract, contract.GetType());
        }

        private IDisposable RegisterLocal(RContract contract, Type type) {
            if (type == null) throw new ArgumentNullException(nameof(type));
            
            if (!this.contracts.ContainsKey(type) && !this.contracts.ContainsValue(contract)) {
                this.contracts.Add(type, contract);
                this.newContract.OnNext(contract);
                return Disposable.Create(() => this.Unregister(type));
            }

            throw new InvalidOperationException($"Current type {type} of contract is exists in container");
        }

        private void Unregister(Type type) {
            if (this.contracts.ContainsKey(type)) {
                this.contracts.Remove(type);
            }
        }

        /// <summary>
        /// Resolve contract <typeparamref name="T"/> synchronously.
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        /// <returns>Instance of <typeparamref name="T"/> or null</returns>
        public T Resolve<T>() where T : RContract {
            if (this.contracts.TryGetValue(typeof(T), out var contract)) {
                return (T) contract;
            }

            return null;
        }

        /// <summary>
        /// Resolve contact <typeparamref name="T"/> asynchronously. 
        /// Waiting for contract if contract doesn't exist.
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        public IObservable<T> ResolveAsync<T>() where T : RContract {
            return Observable.Create<T>(o => {
                if (this.contracts.TryGetValue(typeof(T), out var contract)) {
                    o.OnNext((T) contract);
                    o.OnCompleted();
                    return Disposable.Empty;
                }

                return this.newContract
                    .Where(c => c is T)
                    .Subscribe(c => {
                        o.OnNext((T) c);
                        o.OnCompleted();
                    });
            });
        }

        /// <summary>
        /// Stream contracts with <typeparamref name="T"/>.
        /// <para/>Doesn't work with async\await cause never calling OnComplete().
        /// </summary>
        /// <typeparam name="T">Type of contract</typeparam>
        public IObservable<T> ResolveStream<T>() where T : RContract {
            return Observable.Create<T>(o => {
                if (this.contracts.TryGetValue(typeof(T), out var contract)) {
                    o.OnNext((T) contract);
                }

                return this.newContract
                    .Where(c => c is T)
                    .Subscribe(c => o.OnNext((T) c));
            });
        }

        /// <summary>
        /// Clear contracts list without locking container
        /// </summary>
        public void Dispose() {
            this.contracts.Clear();
        }
    }
}
#endif