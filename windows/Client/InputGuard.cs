using System.Diagnostics;
using System.Runtime.InteropServices;
namespace Kyla;
// Best effort on the normal desktop. Never suppresses Windows' secure desktop.
public sealed class InputGuard:IDisposable {
 private readonly Func<bool> blocked;
 private readonly Hook keyboardCallback,mouseCallback;
 private readonly IntPtr keyboard,mouse;
 private readonly uint pid=(uint)Environment.ProcessId;
 public InputGuard(Func<bool> isBlocked){blocked=isBlocked;keyboardCallback=Keyboard;mouseCallback=Mouse;var module=GetModuleHandle(null);keyboard=SetWindowsHookEx(13,keyboardCallback,module,0);mouse=SetWindowsHookEx(14,mouseCallback,module,0);}
 private bool Ours(IntPtr window){GetWindowThreadProcessId(window,out uint p);return p==pid;}
 private IntPtr Keyboard(int code,IntPtr w,IntPtr l){if(code>=0&&blocked()){
  int key=Marshal.ReadInt32(l);bool alt=(GetAsyncKeyState(0x12)&0x8000)!=0,ctrl=(GetAsyncKeyState(0x11)&0x8000)!=0;
  if(key is 0x5B or 0x5C||(alt&&key is 0x09 or 0x73 or 0x1B)||(ctrl&&key==0x1B)||!Ours(GetForegroundWindow()))return new IntPtr(1);
 }return CallNextHookEx(keyboard,code,w,l);}
 private IntPtr Mouse(int code,IntPtr w,IntPtr l){if(code>=0&&blocked()){
  var point=Marshal.PtrToStructure<POINT>(l);if(!Ours(WindowFromPoint(point)))return new IntPtr(1);
 }return CallNextHookEx(mouse,code,w,l);}
 public void Dispose(){if(keyboard!=IntPtr.Zero)UnhookWindowsHookEx(keyboard);if(mouse!=IntPtr.Zero)UnhookWindowsHookEx(mouse);}
 private delegate IntPtr Hook(int code,IntPtr w,IntPtr l);
 [StructLayout(LayoutKind.Sequential)]private struct POINT{public int X,Y;}
 [DllImport("user32.dll",SetLastError=true)]private static extern IntPtr SetWindowsHookEx(int type,Hook callback,IntPtr module,uint thread);
 [DllImport("user32.dll")]private static extern bool UnhookWindowsHookEx(IntPtr hook);
 [DllImport("user32.dll")]private static extern IntPtr CallNextHookEx(IntPtr hook,int code,IntPtr w,IntPtr l);
 [DllImport("user32.dll")]private static extern short GetAsyncKeyState(int key);
 [DllImport("user32.dll")]private static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")]private static extern IntPtr WindowFromPoint(POINT point);
 [DllImport("user32.dll")]private static extern uint GetWindowThreadProcessId(IntPtr hwnd,out uint process);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode)]private static extern IntPtr GetModuleHandle(string? name);
}
