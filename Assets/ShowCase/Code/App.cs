namespace Red.Example {
	using UI;
	
	/// <summary>
	/// Variation of ServiceLocator, it can be made non-static, and in general have any number of containers
    /// But for this you need to build an architecture where the container\containers will go down from the top.
	/// </summary>
	public static class App {
		public static RContainer Common = new RContainer();
		public static RContainerUi UI = new RContainerUi();
		public static RContainer Player = new RContainer();
	}
}