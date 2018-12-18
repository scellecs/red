namespace Red.Example {
	using UnityEngine;

	public class Bullet : MonoBehaviour {
		private void Update() {
			if (Physics.Raycast(this.transform.position, this.transform.forward, out var hit, 100f)) {
				//For scripts from the outside, we can try to get a contract using TryGet, and if not, then null
				var player = hit.collider.TryGet<CPlayer>();
				if (player != null) {
					player.HP.Value -= 10f;
				}
			}
		}
	}
	
}