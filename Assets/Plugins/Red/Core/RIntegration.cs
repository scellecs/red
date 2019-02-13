namespace Red {
    public interface IIntegrationConfiguration {
    }

    public interface IIntegrationState {
    }
    
    public interface IPreIntegrationRoot<in TC, in TS> : IIntegrationRoot<TC, TS>
        where TC : IIntegrationConfiguration
        where TS : IIntegrationState {
    }

    public interface IIntegrationRoot<in TC, in TS>
        where TC : IIntegrationConfiguration
        where TS : IIntegrationState {
        void Integrate(TC configuration, TS state, IObservableScheduler scheduler);
    }

    public interface IPostIntegrationRoot<in TC, in TS> : IIntegrationRoot<TC, TS>
        where TC : IIntegrationConfiguration
        where TS : IIntegrationState {
    }
}