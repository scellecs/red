namespace Red.Example {
    using UniRx;
    using UnityEngine;

    /// <summary>
    /// Тут примеры что используется в контрактах в основном
    /// Можно помещать любые обсерваблы, но мы часто обходимся комбинацией готовых реактивных объектов
    /// </summary>
    public class CBotManager : RContract<CBotManager> {
        public ReactiveCommand SomeLogic { get; } = new ReactiveCommand();
        public ReactiveCommand SomeLogic2 { get; } = new ReactiveCommand();
        public ReactiveCommand SomeLogic3 { get; } = new ReactiveCommand();
        public ReactiveCommand<int> SomeLogic4 { get; } = new ReactiveCommand<int>();
        
        public ReactiveProperty<int> SomeProperty { get; } = new ReactiveProperty<int>();
        public ReactiveCollection<int> SomeCollection { get; } = new ReactiveCollection<int>();
        
        //Это типа аналог Func<int, int> только реактивный, то есть ты можешь дожидаться возвращаемого значения
        //Самописный, потому уточняю
        public ReactiveOperation<int, int> SomeOperation { get; } = new ReactiveOperation<int, int>();
    }

    public class BotManager : MonoBehaviour {
        private CompositeDisposable disposable = new CompositeDisposable();
        private CBotManager         contract;
        
        private void OnEnable() {
            this.contract = this.GetOrCreate<CBotManager>();

            this.contract.SomeLogic.Subscribe().AddTo(this.disposable);
            
            //Регистрация в статическом контейнере ссылок
            this.contract.RegisterIn(App.Common);
            
            //Есть альтернативная запись
            //App.Common.Register(this.contract);
        }

        private void OnDisable() {
            this.disposable.Clear();
        }
    }
}