using OpenTK.Graphics.OpenGL;
using System.Runtime.InteropServices;
using LibUtil;

namespace LibGL
{
    public class Debug
    {
        private readonly static string[][] DEBUG_EXCEPTIONS =
        [
            ["will use VIDEO memory"],
            ["allocated multisample storage"],
            ["correctly render to an integer framebuffer"],
            //["GL_SHADER_STORAGE_BUFFER", "from VIDEO memory to HOST memory"],
        ];

        public static void Enable()
        {
            Log.Write("Enabling Debug");

            GL.Enable(EnableCap.DebugOutput);
            GL.Enable(EnableCap.DebugOutputSynchronous);

            GL.DebugMessageCallback((source, type, id, severity, length, message, userParam) =>
            {
                var messageText = Marshal.PtrToStringAnsi(message, length);

                if (DEBUG_EXCEPTIONS.Any(_ => _.All(messageText.Contains)))
                {
                    return;
                }

                Log.Write($"---- OpenGL Debug Message");
                Log.Write($"-- Source: {source} | Type: {type} | Id: {id} | Severity: {severity}");
                Log.Write($"-- Message: {messageText}");

                if (severity == DebugSeverity.DebugSeverityHigh)
                {
                    //throw new Exception();
                }
            }, IntPtr.Zero);

            GL.DebugMessageControl(DebugSourceControl.DontCare, DebugTypeControl.DontCare, DebugSeverityControl.DontCare, 0, Array.Empty<int>(), true);
            //GL.DebugMessageInsert(DebugSourceExternal.DebugSourceApplication, DebugType.DebugTypeMarker, 0, DebugSeverity.DebugSeverityNotification, -1, "Enable Debug");
        }

        public static void CheckError(string msg)
        {
            var err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                Log.Write($"{msg}: {err}");
            }
        }
    }
}
