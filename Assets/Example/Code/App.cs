using Red.Example.UI;

namespace Red.Example {
	
	/// <summary>
	/// Вариация ServiceLocator, его можно сделать нестатичным, да и вообще иметь сколько угодно контейнеров
	/// Но под это нужно строить архитектуру, где контейнер\контейнеры будут спускаться сверху вниз
	/// </summary>
	public static class App {
		public static RContainer Common = new RContainer();
		public static RContainerUI UI = new RContainerUI();
		public static RContainer Player = new RContainer();
	}
}