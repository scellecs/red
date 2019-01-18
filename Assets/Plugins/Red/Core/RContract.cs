#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using UniRx;

    /// <inheritdoc />
    /// <summary>
    ///     Base non-generic type for contracts
    /// </summary>
    public abstract class RContract : IDisposable {
#if UNITY_EDITOR
        internal static ReactiveDictionary<object, ReactiveCollection<RContract>> AllContracts 
            = new ReactiveDictionary<object, ReactiveCollection<RContract>>();
#endif

        /// <summary>
        ///     Reference for object with which the contract is associated
        /// </summary>
        public object Target { get; protected set; }

        /// <summary>
        ///     Unique identifier for getting different instances from single target
        /// </summary>
        public string Identifier { get; protected set; }

        protected readonly CompositeDisposable Disposables = new CompositeDisposable();

        protected virtual void PreInitialize() {
#if UNITY_EDITOR
            if (this.Target != null) {
                if (AllContracts.TryGetValue(this.Target, out var list)) {
                    list.Add(this);
                }
                else {
                    AllContracts.Add(this.Target, new ReactiveCollection<RContract> {this});
                }
            }
#endif
        }

        /// <summary>
        ///     Default place for getting sub-contracts or create complex <see cref="IObservable{T}" />
        /// </summary>
        protected virtual void Initialize() {
        }

        public virtual void Dispose() {
            this.Disposables.Clear();
        }

        [AttributeUsage(AttributeTargets.All)]
        protected class DescriptionAttribute : Attribute {
            protected DescriptionAttribute() : this("") {
            }

            protected DescriptionAttribute(string description) {
                this.Description = description;
            }

            public string Description { get; }
        }

        protected sealed class InputAttribute : DescriptionAttribute {
            public InputAttribute() {
            }

            public InputAttribute(string description) : base(description) {
            }
        }

        protected sealed class OutputAttribute : DescriptionAttribute {
            public OutputAttribute() {
            }

            public OutputAttribute(string description) : base(description) {
            }
        }

        protected sealed class InternalAttribute : DescriptionAttribute {
            public InternalAttribute() {
            }

            public InternalAttribute(string description) : base(description) {
            }
        }
    }

    /// <summary>
    ///     Mediator for <see cref="UniRx.ReactiveCommand" />, <see cref="UniRx.ReactiveProperty{T}" />,
    ///     <see cref="UniRx.ReactiveOperation{T,TR}" />
    /// </summary>
    /// <typeparam name="T0">Type of inherited contract</typeparam>
    public abstract class RContract<T0> : RContract where T0 : RContract<T0>, new() {
        private static readonly List<T0> Contracts = new List<T0>();

        protected RContract() {
        }

        /// <summary>
        ///     Special constructor for creating non-gameObject contracts. Also for static types.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="identifier">Unique identifier for contract</param>
        public RContract([CanBeNull] object target, string identifier = "") {
            this.Target = target;
            this.Identifier = identifier;

            this.PreInitialize();
            this.Initialize();
        }
      
        protected override void PreInitialize() {
            base.PreInitialize();
            RContract<T0>.Contracts.Add((T0) this);
  
            Disposable
                .Create(() => RContract<T0>.Contracts.Remove((T0) this))
                .AddTo(this.Disposables);
        }

        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or create it, if it doesn't exists for current object
        ///     <para />
        ///     For null target use cctor instead
        /// </summary>
        /// <typeparam name="T0">Type of contract</typeparam>
        /// <param name="obj">Usual type or null</param>
        /// <param name="identifier">Unique identifier for contract</param>
        public static T0 GetOrCreate(object obj, string identifier = "") {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }

            var contract = RContract<T0>.TryGet(obj, identifier);
            if (contract == null) {
                contract = new T0 {
                    Target     = obj,
                    Identifier = identifier
                };
                contract.PreInitialize();
                contract.Initialize();
            }

            return contract;
        }

        /// <summary>
        ///     Return instance <see cref="RContract{T0}" /> or null if it doesn't exists for current object
        /// </summary>
        /// <typeparam name="T0">Type of contract</typeparam>
        /// <param name="obj">Usual type or null</param>
        /// <param name="identifier">Unique identifier for contract</param>
        [CanBeNull]
        public static T0 TryGet(object obj, string identifier = "") {
            if (obj == null) {
                throw new ArgumentNullException(nameof(obj));
            }

            return RContract<T0>.Contracts.FirstOrDefault(c => c.Target == obj && c.Identifier == identifier);
        }

        /// <summary>
        ///     Returns instance <see cref="RContract{T0}" /> or create it, if it doesn't exists for current object
        ///     <para />
        ///     For null target use cctor instead
        /// </summary>
        /// <param name="identifier">Unique identifier for contract</param>
        /// <typeparam name="T1">Type of contract</typeparam>
        /// <returns>Instance of contract</returns>
        protected T1 GetOrCreate<T1>(string identifier = "") where T1 : RContract<T1>, new() {
            var local = RContract<T1>.GetOrCreate(this.Target, identifier);
            return local;
        }
    }
}
#endif