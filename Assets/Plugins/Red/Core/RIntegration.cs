#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red {
    public interface IIntegrationConfiguration {
    }

    public interface IIntegrationState {
    }

    public interface IIntegrationBridge {
        void Inject<T> (T originState) where T : IIntegrationState;
    }

    public interface IIntegrationState<in T> : IIntegrationState
        where T : IIntegrationBridge {
        void Inject(T input);
    }

    public interface IIntegrationRoot<in TC, in TS>
        where TC : IIntegrationConfiguration
        where TS : IIntegrationState {
        void Integrate(TC configuration, TS state, IObservableScheduler scheduler);
    }
}
#endif