namespace Red.Example {
	using UnityEngine;

	public class Bullet : MonoBehaviour {
		private void Update() {
			if (Physics.Raycast(this.transform.position, this.transform.forward, out var hit, 100f))
			{
				//Для скриптов извне мы можем попытаться получить контракт с помощью TryGet, а если его нет, то нуль
				var player = hit.collider.TryGet<CPlayer>();
				if (player != null) {
					player.HP.Value -= 10f;
				}
			}
		}
	}
	
}