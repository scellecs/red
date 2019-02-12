namespace Red {
    using System;
    using UniRx;

    public interface IIntegrationConfiguration {
        
    }

    public interface IIntegrationState {
        
    }

    public interface IIntegrationRoot<in TC, in TS> {
        void Integrate(TC configuration, TS state, IScheduler scheduler, IObservable<Unit> origin);
    }
}