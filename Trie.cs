using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TryToTrie
{
    public interface ITrie<V>
    {
        V Get(string key);
    }

    public class Trie
    {
        public static ITrie<int> Build(IReadOnlyDictionary<string, int> d)
        {
            var noise = Guid.NewGuid().ToString()[..8];
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                new AssemblyName($"Trie_{noise}_Assembly"),
                AssemblyBuilderAccess.Run);
            assemblyBuilder.SetCustomAttribute(Generated);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule($"Trie_{noise}_Module");
            moduleBuilder.SetCustomAttribute(Generated);
            var typeAttrs =
                TypeAttributes.Public |
                TypeAttributes.Class |
                TypeAttributes.AutoClass |
                TypeAttributes.AnsiClass |
                TypeAttributes.BeforeFieldInit |
                TypeAttributes.AutoLayout;
            var typeBuilder = moduleBuilder.DefineType($"Trie_{noise}", typeAttrs, typeof(object));
            typeBuilder.SetCustomAttribute(Generated);
            typeBuilder.AddInterfaceImplementation(typeof(ITrie<int>));
            var ctorAttrs =
                MethodAttributes.Public |
                MethodAttributes.HideBySig |
                MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName;
            typeBuilder.DefineDefaultConstructor(ctorAttrs);
            var methodAttrs =
                MethodAttributes.Public |
                MethodAttributes.Final |
                MethodAttributes.HideBySig |
                MethodAttributes.NewSlot |
                MethodAttributes.Virtual;
            var methodBuilder = typeBuilder.DefineMethod(
                nameof(ITrie<int>.Get),
                methodAttrs,
                CallingConventions.Standard,
                typeof(int),
                new Type[] { typeof(string) });
            methodBuilder.SetCustomAttribute(Generated);
            var il = methodBuilder.GetILGenerator();
            IfElseChain(
                il,
                d.OrderBy(kv => kv.Key.Length)
                    .Select(kv => Clause(_ => EqualsStringLiteral(il, kv.Key), _ => ReturnInt(il, kv.Value))),
                ThrowKeyNotFoundException);
            var newType = typeBuilder.CreateType() ?? throw new Exception("no type created");
            return Activator.CreateInstance(newType) as ITrie<int> ?? throw new Exception("no instance created");
        }

        private static (Action<ILGenerator> cond, Action<ILGenerator> ifTrue) Clause(
            Action<ILGenerator> cond, Action<ILGenerator> ifTrue) => (cond, ifTrue);

        // both branches are expected to be terminal (return or throw)
        private static void IfElse(
            ILGenerator il,
            Action<ILGenerator> cond,
            Action<ILGenerator> ifTrue,
            Action<ILGenerator> ifFalse)
        {
            var elseLabel = il.DefineLabel();
            cond(il);
            il.Emit(OpCodes.Brfalse, elseLabel);
            ifTrue(il);
            il.MarkLabel(elseLabel);
            ifFalse(il);
        }

        private static void IfElseChain(
            ILGenerator il,
            IEnumerable<(Action<ILGenerator> cond, Action<ILGenerator> ifTrue)> clauses,
            Action<ILGenerator> ifFalse)
        {
            foreach (var (cond, ifTrue) in clauses)
            {
                var elseLabel = il.DefineLabel();
                cond(il);
                il.Emit(OpCodes.Brfalse, elseLabel);
                ifTrue(il);
                il.MarkLabel(elseLabel);
            }

            ifFalse(il);
        }

        private static void EqualsStringLiteral(ILGenerator il, string key)
        {
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldstr, key);
            var stringEqualsMethod = typeof(string).GetMethod(
                "Equals",
                BindingFlags.Public | BindingFlags.Static,
                new Type[] { typeof(string), typeof(string) })
                ?? throw new Exception("no string.Equals method");
            il.Emit(stringEqualsMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, stringEqualsMethod);
        }

        private static void ReturnInt(ILGenerator il, int value)
        {
            il.Emit(OpCodes.Ldc_I4, value);
            il.Emit(OpCodes.Ret);
        }

        private static void ThrowKeyNotFoundException(ILGenerator il)
        {
            il.Emit(OpCodes.Ldarg_1);
            var exceptionCtor = typeof(KeyNotFoundException).GetConstructor(new Type[] { typeof(string) });
            il.Emit(OpCodes.Newobj, exceptionCtor ?? throw new Exception("no ThrowKeyNotFoundException(string) constructor"));
            il.Emit(OpCodes.Throw);
        }

        private static CustomAttributeBuilder Generated =>
            new(typeof(CompilerGeneratedAttribute).GetConstructors().Single(), Array.Empty<object>());
    }
}
