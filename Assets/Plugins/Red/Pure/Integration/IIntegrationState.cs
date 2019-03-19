#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red.Pure.Integration {
    using JetBrains.Annotations;

    public interface IIntegrationState {
    }

    public interface IIntegrationState<T> : IIntegrationState
        where T : IIntegrationBridge {
#if ZENJECT
        [Inject]
#endif
        T Bridge { [NotNull] get; [NotNull] set; }
    }
}
#endif