namespace Red.Example {
	using System;
	using UniRx;
	using UnityEngine;

	//Conventionally, there is some kind of useful work with the bot manager, which I am too lazy to write :)
	public class BotFinderComponent : MonoBehaviour {
		
		private async void OnEnable() {
			//Here so asynchronously we can resolve dependences at the container
			//Helps not focus on the order of initialization methods
			var botManager = await App.Common.ResolveAsync<CBotManager>();			
			
			//You can of course resolve directly and if there is no contract in the container, then you get null
			botManager = App.Common.Resolve<CBotManager>();
			
			//If anything, then this is observable and you can add a timeout for example
			botManager = await App.Common.ResolveAsync<CBotManager>().Timeout(TimeSpan.FromSeconds(10));


			botManager.SomeLogic.Execute();
			botManager.SomeLogic2.Execute();
			botManager.SomeLogic3.Execute();
			botManager.SomeLogic4.Execute(5);
		}
	}
}