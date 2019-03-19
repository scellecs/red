#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))
namespace Red.Pure.Integration {
    using JetBrains.Annotations;

    public interface IIntegrationRoot<in TC, in TS>
        where TC : IIntegrationConfiguration
        where TS : IIntegrationState {
        void Integrate([NotNull] TC configuration, [NotNull] TS state, [NotNull] IObservableScheduler scheduler);
    }
}
#endif