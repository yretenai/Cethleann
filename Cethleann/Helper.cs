using System.Diagnostics;

namespace Cethleann
{
    internal static class Helper
    {
        [Conditional("DEBUG_ASSERTIONS"), DebuggerHidden, DebuggerNonUserCode, DebuggerStepThrough]
        public static void Assert(bool condition, string message, string detail, params object[] args) => Debug.Assert(condition, message, detail, args);

        [Conditional("DEBUG_ASSERTIONS"), DebuggerHidden, DebuggerNonUserCode, DebuggerStepThrough]
        public static void Assert(bool condition, string message, string detail) => Debug.Assert(condition, message, detail);

        [Conditional("DEBUG_ASSERTIONS"), DebuggerHidden, DebuggerNonUserCode, DebuggerStepThrough]
        public static void Assert(bool condition, string message) => Debug.Assert(condition, message);

        [Conditional("DEBUG_ASSERTIONS"), DebuggerHidden, DebuggerNonUserCode, DebuggerStepThrough]
        public static void Assert(bool condition) => Debug.Assert(condition);
    }
}
