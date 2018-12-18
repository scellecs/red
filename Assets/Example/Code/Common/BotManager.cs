namespace Red.Example {
    using UniRx;
    using UnityEngine;

    /// <summary>
    /// Here are examples of what is used in contracts mainly.
    /// You can put any observables, but we often get by with a combination of ready-made reactive objects.
    /// </summary>
    public class CBotManager : RContract<CBotManager> {
        public ReactiveCommand SomeLogic { get; } = new ReactiveCommand();
        public ReactiveCommand SomeLogic2 { get; } = new ReactiveCommand();
        public ReactiveCommand SomeLogic3 { get; } = new ReactiveCommand();
        public ReactiveCommand<int> SomeLogic4 { get; } = new ReactiveCommand<int>();
        
        public ReactiveProperty<int> SomeProperty { get; } = new ReactiveProperty<int>();
        public ReactiveCollection<int> SomeCollection { get; } = new ReactiveCollection<int>();
        
        //This is an analogue of Func <int, int>, but reactive, that is, you can wait for the return value.
        public ReactiveOperation<int, int> SomeOperation { get; } = new ReactiveOperation<int, int>();
    }

    public class BotManager : MonoBehaviour {
        private CompositeDisposable disposable = new CompositeDisposable();
        private CBotManager         contract;
        
        private void OnEnable() {
            this.contract = this.GetOrCreate<CBotManager>();

            this.contract.SomeLogic.Subscribe().AddTo(this.disposable);
            
            //Registration in a static container
            App.Common.Register(this.contract);

            //There is an alternative
            //this.contract.RegisterIn(App.Common);
        }

        private void OnDisable() {
            this.disposable.Clear();
        }
    }
}