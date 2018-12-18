namespace Red.Example {
    using UniRx;
    using UnityEngine;

    public class HealthComponent : MonoBehaviour {
        private CompositeDisposable disposable = new CompositeDisposable();
        private CPlayer contract;
        
        private void OnEnable() {
            //Getting a contract from this object, if it already exists; or creating a new instance
            //Several components may try to get a contract and will eventually refer to the same
            //For example, DummyComponent will get the same contract.
            //The exception is the addition of the string identifier this.GetOrCreate<CPlayer>("someString")
            //This allows you to bind several contracts of the same type to the GO.
            //In most cases it is not necessary.
            this.contract = this.GetOrCreate<CPlayer>();

            this.contract.HP.Subscribe(hp => Debug.Log(hp)).AddTo(this.disposable);
        }

        private void OnDisable() {
            this.disposable.Clear();
        }
    }
}