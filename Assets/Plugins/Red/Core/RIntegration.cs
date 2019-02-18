namespace Red {
    public interface IIntegrationConfiguration {
    }

    public interface IIntegrationState {
    }
    
    public interface IIntegrationRoot<in TC, in TS>
        where TC : IIntegrationConfiguration
        where TS : IIntegrationState {
        void Integrate(TC configuration, TS state, IObservableScheduler scheduler);
    }
}