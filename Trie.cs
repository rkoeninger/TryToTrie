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
            var root = Nodify(d);
            var remainingKey = il.DeclareLocal(typeof(string));
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Stloc, remainingKey);
            BuildNode(il, remainingKey, root);
            var newType = typeBuilder.CreateType() ?? throw new Exception("no type created");
            return Activator.CreateInstance(newType) as ITrie<int> ?? throw new Exception("no instance created");
        }

        private static void BuildNode(ILGenerator il, LocalBuilder remainingKey, Node node)
        {
            if (node.HasValue)
            {
                IfElse(
                    il,
                    _ => IsEmptyString(il, remainingKey),
                    _ => ReturnInt(il, node.Value),
                    _ => BuildChildren(il, remainingKey, node.Children));
            }
            else
            {
                BuildChildren(il, remainingKey, node.Children);
            }
        }

        private static void BuildChildren(ILGenerator il, LocalBuilder remainingKey, List<Node> children)
        {
            IfElseChain(
                il,
                children.OrderBy(n => n.Prefix.Length)
                    .Select(n =>
                        Clause(
                            _ => StartsWithStringLiteral(il, remainingKey, n.Prefix),
                            _ =>
                            {
                                RemovePrefix(il, remainingKey, n.Prefix);
                                BuildNode(il, remainingKey, n);
                            })),
                ThrowKeyNotFoundException);
        }

        private static (Action<ILGenerator>, Action<ILGenerator>) Clause(
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
            IEnumerable<(Action<ILGenerator>, Action<ILGenerator>)> clauses,
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

        private static void IsEmptyString(ILGenerator il, LocalBuilder remainingKey)
        {
            var lengthMethod = typeof(string).GetProperty("Length")?.GetGetMethod()
                ?? throw new Exception("no string.Length property");
            il.Emit(OpCodes.Ldloc, remainingKey);
            il.Emit(lengthMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, lengthMethod);
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Ceq);
        }

        private static void StartsWithStringLiteral(ILGenerator il, LocalBuilder remainingKey, string prefix)
        {
            var startsWithMethod = typeof(string).GetMethod(
                "StartsWith",
                BindingFlags.Public | BindingFlags.Instance,
                new Type[] { typeof(string) })
                ?? throw new Exception("no string.StartsWith method");
            il.Emit(OpCodes.Ldloc, remainingKey);
            il.Emit(OpCodes.Ldstr, prefix);
            il.Emit(startsWithMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, startsWithMethod);
        }

        private static void RemovePrefix(ILGenerator il, LocalBuilder remainingKey, string prefix)
        {
            var substringMethod = typeof(string).GetMethod(
                "Substring",
                BindingFlags.Public | BindingFlags.Instance,
                new Type[] { typeof(int) })
                ?? throw new Exception("no string.Substring method");
            il.Emit(OpCodes.Ldloc, remainingKey);
            il.Emit(OpCodes.Ldc_I4, prefix.Length);
            il.Emit(substringMethod.IsVirtual ? OpCodes.Callvirt : OpCodes.Call, substringMethod);
            il.Emit(OpCodes.Stloc, remainingKey);
        }

        private static void ReturnInt(ILGenerator il, int value)
        {
            il.Emit(OpCodes.Ldc_I4, value);
            il.Emit(OpCodes.Ret);
        }

        private static void ThrowKeyNotFoundException(ILGenerator il)
        {
            var exceptionCtor = typeof(KeyNotFoundException).GetConstructor(new Type[] { typeof(string) });
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Newobj, exceptionCtor ?? throw new Exception("no ThrowKeyNotFoundException(string) constructor"));
            il.Emit(OpCodes.Throw);
        }

        private static CustomAttributeBuilder Generated =>
            new(typeof(CompilerGeneratedAttribute).GetConstructors().Single(), Array.Empty<object>());

        public class Node
        {
            public string Prefix { get; set; } = "";
            public bool HasValue { get; set; }
            public int Value { get; set; }
            public List<Node> Children { get; set; } = new();

            public override string ToString() => ToString(0);

            private static string Spaces(int n) => string.Join("", Enumerable.Range(0, n).Select(_ => " "));

            public string ToString(int depth) =>
                Spaces(depth) +
                Prefix +
                (HasValue ? $" : {Value}" : "") +
                string.Join("", Children.Select(c => "\r\n" + c.ToString(depth + 1)));
        }

        public static Node Nodify(IReadOnlyDictionary<string, int> d)
        {
            var root = new Node();

            if (d.TryGetValue("", out var value))
            {
                root.HasValue = true;
                root.Value = value;
            }

            var entries = d.Where(kv => !string.IsNullOrEmpty(kv.Key))
                .Select(kv => (kv.Key, kv.Value))
                .ToList();
            NodifyOnto(root, entries);
            Simplify(root);
            return root;
        }

        private static void NodifyOnto(Node parent, List<(string, int)> entries)
        {
            var empty = entries.FirstOrDefault(p => p.Item1 == "");

            if (empty != default)
            {
                parent.HasValue = true;
                parent.Value = empty.Item2;
            }

            foreach (var group in entries.Where(p => p.Item1 != "").GroupBy(p => p.Item1[..1]))
            {
                var child = new Node
                {
                    Prefix = group.Key
                };
                NodifyOnto(child, group.Select(p => (p.Item1[1..], p.Item2)).ToList());
                parent.Children.Add(child);
            }
        }

        private static void Simplify(Node node)
        {
            while (node.Children.Count == 1 && !node.HasValue)
            {
                var child = node.Children.Single();
                node.Prefix += child.Prefix;
                node.HasValue = child.HasValue;
                node.Value = child.Value;
                node.Children = child.Children;
            }

            foreach (var child in node.Children)
            {
                Simplify(child);
            }
        }
    }
}
