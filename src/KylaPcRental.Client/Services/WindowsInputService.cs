using System;
using System.Runtime.InteropServices;

namespace KylaPcRental.Client.Services;

/// <summary>
/// Sends a normal ESC key press to the foreground Windows application.
/// Used as a best-effort game pause request at session expiry.
/// </summary>
public sealed class WindowsInputService
{
    private const uint InputKeyboard = 1;
    private const ushort VirtualKeyEscape = 0x1B;
    private const uint KeyUp = 0x0002;

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInput);

    public bool SendEscape()
    {
        var inputs = new[]
        {
            new INPUT
            {
                type = InputKeyboard,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT { wVk = VirtualKeyEscape }
                }
            },
            new INPUT
            {
                type = InputKeyboard,
                u = new INPUTUNION
                {
                    ki = new KEYBDINPUT { wVk = VirtualKeyEscape, dwFlags = KeyUp }
                }
            }
        };

        return SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<INPUT>()) == inputs.Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public INPUTUNION u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct INPUTUNION
    {
        [FieldOffset(0)] public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public nint dwExtraInfo;
    }
}
