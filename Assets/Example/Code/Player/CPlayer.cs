namespace Red.Example {
    using UniRx;

    /// <summary>
    /// Простенький контракт игрока
    /// </summary>
    public class CPlayer : RContract<CPlayer> {
        [Input] public ReactiveCommand JumpCommand = new ReactiveCommand();
        [Output] public ReactiveProperty<PlayerState> State = new ReactiveProperty<PlayerState>(PlayerState.Idle);
        
        public ReactiveProperty<float> HP { get; } = new ReactiveProperty<float>();
        public IReadOnlyReactiveProperty<bool> IsDead { get; private set; }

        /// <summary>
        /// Метод для установление взаимосвязей между пропертями
        /// </summary>
        protected override void Initialize() {
            this.IsDead = this.HP.Select(hp => hp <= 0f).ToReactiveProperty();
        }
    }
}

