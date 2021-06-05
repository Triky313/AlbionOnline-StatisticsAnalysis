using Newtonsoft.Json;
using Pastel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

namespace StatisticsAnalysisTool.Common
{
    [SuppressUnmanagedCodeSecurity]
    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";

        [DllImport(Kernel32_DllName)]
        private static extern bool AllocConsole();

        [DllImport(Kernel32_DllName)]
        private static extern bool FreeConsole();

        [DllImport(Kernel32_DllName)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(Kernel32_DllName)]
        // ReSharper disable once UnusedMember.Local
        private static extern int GetConsoleOutputCP();

        public static bool HasConsole => GetConsoleWindow() != IntPtr.Zero;

        /// <summary>
        /// Creates a new console instance if the process is not attached to a console already.
        /// </summary>
        public static void Show()
        {
            if (!HasConsole)
            {
                AllocConsole();
                InvalidateOutAndError();
            }
        }

        /// <summary>
        /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.
        /// </summary>
        public static void Hide()
        {
            if (HasConsole)
            {
                SetOutAndErrorNull();
                FreeConsole();
            }
        }

        public static void Toggle()
        {
            if (HasConsole)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private static void InvalidateOutAndError()
        {
            var type = typeof(Console);

            var _out = type.GetField("_out", BindingFlags.Static | BindingFlags.NonPublic);
            var _error = type.GetField("_error", BindingFlags.Static | BindingFlags.NonPublic);
            var _InitializeStdOutError = type.GetMethod("InitializeStdOutError", BindingFlags.Static | BindingFlags.NonPublic);

            Debug.Assert(_out != null);
            Debug.Assert(_error != null);

            Debug.Assert(_InitializeStdOutError != null);

            _out.SetValue(null, null);
            _error.SetValue(null, null);

            _InitializeStdOutError.Invoke(null, new object[] { true });
        }

        private static void SetOutAndErrorNull()
        {
            Console.SetOut(TextWriter.Null);
            Console.SetError(TextWriter.Null);
        }

        private const string _errorColor = "#ca3431";
        private const string _warnColor = "#faa627";
        private const string _eventColor = "#248A84";
        private const string _eventMapChangeColor = "#0279be";

        public static void WriteLineForNetworkHandler(string name, Dictionary<byte, object> parameters)
        {
            if (HasConsole)
            {
                Console.WriteLine($@"[{DateTime.UtcNow}] {name}: ".Pastel(_eventColor) + $@"{JsonConvert.SerializeObject(parameters)}");
            }
        }

        public static void WriteLineForWarning(Type declaringType, Exception e)
        {
            if (HasConsole)
            {
                Console.WriteLine($@"[{DateTime.UtcNow}] {declaringType}: {e.Message}".Pastel(_warnColor));
                if (!string.IsNullOrEmpty(e.StackTrace))
                {
                    Console.WriteLine($"{e.StackTrace}".Pastel(_warnColor));
                }
            }
        }

        public static void WriteLineForWarning(Type declaringType, string message)
        {
            if (HasConsole)
            {
                Console.WriteLine($@"[{DateTime.UtcNow}] {declaringType}: {message}".Pastel(_warnColor));
                if (!string.IsNullOrEmpty(message))
                {
                    Console.WriteLine($"{message}".Pastel(_warnColor));
                }
            }
        }

        public static void WriteLineForError(Type declaringType, Exception e)
        {
            if (HasConsole)
            {
                Console.WriteLine($@"[{DateTime.UtcNow}] {declaringType}: {e.Message}".Pastel(_errorColor));
                Console.WriteLine($"{e.StackTrace}".Pastel(_errorColor));
            }
        }

        public static void WriteLineForMessage(Type declaringType, string message)
        {
            if (HasConsole)
            {
                Console.WriteLine($@"[{DateTime.UtcNow}] {declaringType}: {message}".Pastel(_eventMapChangeColor));
            }
        }
    }
}