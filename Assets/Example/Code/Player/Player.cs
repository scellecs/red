using UniRx;

namespace Red.Example {
	using UnityEngine;

	public class Player : MonoBehaviour {
		private readonly CompositeDisposable dispose = new CompositeDisposable();
		private CPlayer contract;

		private void OnEnable() {
			this.contract = this.GetOrCreate<CPlayer>();

			this.contract.JumpCommand
				.Subscribe(_ => Debug.LogWarning("Well, let's say I jumped"))
				.AddTo(this.dispose);
			
			this.contract.RegisterIn(App.Player);
		}

		private void OnDisable() {
			this.dispose.Clear();			
		}
	}
}