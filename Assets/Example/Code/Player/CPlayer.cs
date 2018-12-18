namespace Red.Example {
    using UniRx;

    /// <summary>
    /// Example Player Contract
    /// </summary>
    public class CPlayer : RContract<CPlayer> {
        [Input] public ReactiveCommand JumpCommand = new ReactiveCommand();
        [Output] public ReactiveProperty<PlayerState> State = new ReactiveProperty<PlayerState>(PlayerState.Idle);
        
        [Output] public ReactiveProperty<float> HP { get; } = new ReactiveProperty<float>();
        [Output] public IReadOnlyReactiveProperty<bool> IsDead { get; private set; }

        /// <summary>
        /// Method for determining the relationship between observables
        /// </summary>
        protected override void Initialize() {
            this.IsDead = this.HP.Select(hp => hp <= 0f).ToReactiveProperty();
        }
    }
}

