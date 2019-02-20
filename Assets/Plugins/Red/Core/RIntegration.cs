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