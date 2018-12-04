namespace Red.Example {
	using System;
	using UniRx;
	using UnityEngine;

	//Условно здесь идет какая-то полезная работа с ботМенеджером, которую мне лень писать :)
	public class BotFinderComponent : MonoBehaviour {
		
		private async void OnEnable() {
			//Вот так асинхронно мы можем резолвить зависимости у контейнера
			//Помогает не париться над очередностью вызова методов инициализации
			var botManager = await App.Common.ResolveAsync<CBotManager>();			
			
			//Можно конечно резолвить напрямую и если нет контракта в контейнере, то получать нуль
			botManager = App.Common.Resolve<CBotManager>();
			
			//Если что, то это обсервабл и можно повесить таймаут например :)
			botManager = await App.Common.ResolveAsync<CBotManager>().Timeout(TimeSpan.FromSeconds(10));


			botManager.SomeLogic.Execute();
			botManager.SomeLogic2.Execute();
			botManager.SomeLogic3.Execute();
			botManager.SomeLogic4.Execute(5);
		}
	}
}