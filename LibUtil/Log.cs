using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace LibUtil
{
    public class Log
    {
        private static readonly ConsoleColor[] sColors =
        [
            ConsoleColor.White,
            ConsoleColor.Gray,
            ConsoleColor.Yellow,
            ConsoleColor.Green,
            ConsoleColor.Cyan,
            ConsoleColor.Blue,
        ];

        private static readonly List<string> sClasses = [];

        public static void Write(params string[] lines)
        {
            var line = string.Join("\r\n", lines);
#if DEBUG
            var c = GetCallerMethod();
            lock (sClasses)
            {
                var i = sClasses.FindIndex(c2 => c2 == c);
                if (i == -1)
                {
                    i = sClasses.Count;
                    sClasses.Add(c);
                }

                Console.ForegroundColor = sColors[i % sColors.Length];
                Console.Write($"[{{0:H:mm:ss.fff}}][{c}] ", DateTime.Now);
                Console.WriteLine(line);
            }
#else
            Console.WriteLine($"[{{0:H:mm:ss.fff}}] {line}", DateTime.Now);
#endif
        }

        public static void WriteTitle(string line) =>
            Console.Title = string.Format($"[{{0:H:mm:ss.fff}}] {line}", DateTime.Now);

        private static string GetCallerMethod()
        {
            var st = new StackTrace(2, false);
            var strs = new List<(string, string, string, string)>();
            ToString(st, strs);

            foreach (var (_, Class, Method, _) in strs)
            {
                return $"{ParseClass(Class)}.{ParseMethod(Method)}";
            }

            throw new Exception($"Cannot parse caller from ST: {st}");
        }

        private static string ParseClass(string c) =>
            c.SplitNotEmpty('+', '`')[0];

        private static string ParseMethod(string m)
        {
            if (m == ".ctor")
            {
                return "new";
            }

            if (m.Contains('<'))
            {
                return m[0..(m.IndexOf('>') + 1)];
            }

            return m;
        }

        // https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/Diagnostics/StackTrace.cs
        public static void ToString(StackTrace st, List<(string, string, string, string)> strs, bool args = false, int max = 1)
        {
            for (int iFrameIndex = 0; iFrameIndex < st.FrameCount && strs.Count < max; iFrameIndex++)
            {
                StackFrame? sf = st.GetFrame(iFrameIndex);
                MethodBase? mb = sf?.GetMethod();
                if (mb != null)
                {
                    Type? declaringType = mb.DeclaringType;
                    string methodName = mb.Name;
                    bool methodChanged = false;
                    if (declaringType != null && declaringType.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
                    {
                        bool isAsync = typeof(IAsyncStateMachine).IsAssignableFrom(declaringType);
                        if (isAsync || typeof(IEnumerator).IsAssignableFrom(declaringType))
                        {
                            methodChanged = TryResolveStateMachineMethod(ref mb, out declaringType);
                        }
                    }
                    var sbNamespace = new StringBuilder();
                    var sbClass = new StringBuilder();
                    var sbMethod = new StringBuilder();
                    var sbArgs = new StringBuilder();

                    // if there is a type (non global method) print it
                    // ResolveStateMachineMethod may have set declaringType to null
                    if (declaringType != null)
                    {
                        sbNamespace.Append(declaringType.Namespace);

                        // Append t.FullName, replacing '+' with '.'
                        string fullName = declaringType.FullName!;
                        if (declaringType.Namespace != null)
                        {
                            fullName = fullName[(declaringType.Namespace.Length + 1)..];
                        }
                        sbClass.Append(fullName);
                        /*
                        for (int i = 0; i < fullName.Length; i++)
                        {
                            char ch = fullName[i];
                            sbClass.Append(ch == '+' ? '.' : ch);
                        }
                        */
                    }
                    sbMethod.Append(mb.Name);

                    // deal with the generic portion of the method
                    if (mb is MethodInfo mi && mi.IsGenericMethod)
                    {
                        Type[] typars = mi.GetGenericArguments();
                        sbMethod.Append('[');
                        int k = 0;
                        bool fFirstTyParam = true;
                        while (k < typars.Length)
                        {
                            if (!fFirstTyParam)
                                sbMethod.Append(',');
                            else
                                fFirstTyParam = false;

                            sbMethod.Append(typars[k].Name);
                            k++;
                        }
                        sbMethod.Append(']');
                    }

                    if (args)
                    {
                        ParameterInfo[]? pi = null;
                        try
                        {
                            pi = mb.GetParameters();
                        }
                        catch
                        {
                            // The parameter info cannot be loaded, so we don't
                            // append the parameter list.
                        }
                        if (pi != null)
                        {
                            // arguments printing
                            sbArgs.Append('(');
                            bool fFirstParam = true;
                            for (int j = 0; j < pi.Length; j++)
                            {
                                if (!fFirstParam)
                                    sbArgs.Append(", ");
                                else
                                    fFirstParam = false;

                                string typeName = "<UnknownType>";
                                if (pi[j].ParameterType != null)
                                    typeName = pi[j].ParameterType.Name;
                                sbArgs.Append(typeName);
                                sbArgs.Append(' ');
                                sbArgs.Append(pi[j].Name);
                            }
                            sbArgs.Append(')');
                        }
                    }

                    if (methodChanged)
                    {
                        // Append original method name e.g. +MoveNext()
                        sbMethod.Append('+');
                        sbMethod.Append(methodName);
                        sbMethod.Append('(').Append(')');
                    }

                    strs.Add((sbNamespace.ToString(), sbClass.ToString(), sbMethod.ToString(), sbArgs.ToString()));
                }
            }
        }

        private static bool TryResolveStateMachineMethod(ref MethodBase method, out Type declaringType)
        {
            Debug.Assert(method != null);
            Debug.Assert(method.DeclaringType != null);

            declaringType = method.DeclaringType;

            Type? parentType = declaringType.DeclaringType;
            if (parentType == null)
            {
                return false;
            }

            static MethodInfo[]? GetDeclaredMethods(Type type) =>
                type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            MethodInfo[]? methods = GetDeclaredMethods(parentType);
            if (methods == null)
            {
                return false;
            }

            foreach (MethodInfo candidateMethod in methods)
            {
                IEnumerable<StateMachineAttribute>? attributes = candidateMethod.GetCustomAttributes<StateMachineAttribute>(inherit: false);
                if (attributes == null)
                {
                    continue;
                }

                bool foundAttribute = false, foundIteratorAttribute = false;
                foreach (StateMachineAttribute asma in attributes)
                {
                    if (asma.StateMachineType == declaringType)
                    {
                        foundAttribute = true;
                        foundIteratorAttribute |= asma is IteratorStateMachineAttribute || asma is AsyncIteratorStateMachineAttribute;
                    }
                }

                if (foundAttribute)
                {
                    // If this is an iterator (sync or async), mark the iterator as changed, so it gets the + annotation
                    // of the original method. Non-iterator async state machines resolve directly to their builder methods
                    // so aren't marked as changed.
                    method = candidateMethod;
                    declaringType = candidateMethod.DeclaringType!;
                    return foundIteratorAttribute;
                }
            }

            return false;
        }
    }
}
