#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red.Pure.Integration {
    public interface IIntegrationState {
    }
    
    public interface IIntegrationState<in T> : IIntegrationState
        where T : IIntegrationBridge {
        void Inject(T bridge);
    }
}
#endif