using UniRx;

namespace Red.Example {
	using UnityEngine;

	public class Player : MonoBehaviour {
		private readonly CompositeDisposable dispose = new CompositeDisposable();
		private CPlayer contract;

		private void OnEnable() {
			this.contract = this.GetOrCreate<CPlayer>();
			App.Player.Register(contract);

			Bind();
		}

		private void Bind() {
			this.contract.JumpCommand
				.Subscribe(_ => Debug.LogWarning("Well, let's say I jumped"))
				.AddTo(this.dispose);
		}

		private void OnDisable() {
			this.dispose.Clear();			
		}
	}
}