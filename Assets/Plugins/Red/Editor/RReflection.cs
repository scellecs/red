#if CSHARP_7_OR_LATER || (UNITY_2018_3_OR_NEWER && (NET_STANDARD_2_0 || NET_4_6))  && UNITY_EDITOR
namespace Red.Editor {
    using System;
    using System.Linq;
    using JetBrains.Annotations;

    internal static class RReflectionExtensions {
        internal static Type[] FindCurrentGenericTypeImplementation(this Type child, Type parent) {
            parent = ResolveGenericTypeDefinition(parent);

            var currentChild = child.IsGenericType
                ? child.GetGenericTypeDefinition()
                : child;

            while (currentChild != typeof(object)) {
                var args = GetGenericArgumentsFromInterfaces(parent, child);

                if (args != null) {
                    return args;
                }


                currentChild = currentChild.BaseType != null
                               && currentChild.BaseType.IsGenericType
                    ? currentChild.BaseType.GetGenericTypeDefinition()
                    : currentChild.BaseType;

                if (currentChild == null)
                    return null;
            }

            return null;
        }

        internal static bool InheritsOrImplements(this Type child, Type parent) {
            parent = ResolveGenericTypeDefinition(parent);

            var currentChild = child.IsGenericType
                ? child.GetGenericTypeDefinition()
                : child;

            while (currentChild != typeof(object)) {
                if (parent == currentChild || HasAnyInterfaces(parent, currentChild))
                    return true;

                currentChild = currentChild.BaseType != null
                               && currentChild.BaseType.IsGenericType
                    ? currentChild.BaseType.GetGenericTypeDefinition()
                    : currentChild.BaseType;

                if (currentChild == null)
                    return false;
            }

            return false;
        }

        private static bool HasAnyInterfaces(Type parent, Type child) {
            return child.GetInterfaces()
                .Any(childInterface => {
                    var currentInterface = childInterface.IsGenericType
                        ? childInterface.GetGenericTypeDefinition()
                        : childInterface;
                    return currentInterface == parent;
                });
        }

        [CanBeNull]
        private static Type[] GetGenericArgumentsFromInterfaces(Type parent, Type child) {
            var interf = child.GetInterfaces()
                .FirstOrDefault(childInterface =>
                    childInterface.IsGenericType && childInterface.GetGenericTypeDefinition() == parent);
            return interf != null ? interf.GenericTypeArguments : null;
        }

        private static Type ResolveGenericTypeDefinition(Type parent) {
            var shouldUseGenericType = !(parent.IsGenericType && parent.GetGenericTypeDefinition() != parent);

            if (parent.IsGenericType && shouldUseGenericType)
                parent = parent.GetGenericTypeDefinition();
            return parent;
        }
    }
}
#endif