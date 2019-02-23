#if (CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))) && UNITY_EDITOR
namespace Red.Editor {
    public static class RPaths {
        public const string RedFolder = "Assets/Plugins/Red/";
    }
}
#endif