namespace Red.Example {
    using UniRx;
    using UnityEngine;

    public class HealthComponent : MonoBehaviour {
        private CompositeDisposable disposable = new CompositeDisposable();
        private CPlayer contract;
        
        private void OnEnable() {
            //Получение контракта с этого объета, если он существует уже; или создание нового инстанса
            //Несколько компонентов могут попытаться получить контракт и в итоге будет ссылаться на один и тот же
            //Для примера DummyComponent получит тот же контракт
            //Исключение это добавление стрингового индетификатора this.GetOrCreate<CPlayer>("someString")
            //Это позволяет привязать к ГО несколько контрактов одинакового типа
            //В большинстве случаев это не нужно
            this.contract = this.GetOrCreate<CPlayer>();

            this.contract.HP.Subscribe(hp => Debug.Log(hp)).AddTo(this.disposable);
        }

        private void OnDisable() {
            this.disposable.Clear();
        }
    }
}