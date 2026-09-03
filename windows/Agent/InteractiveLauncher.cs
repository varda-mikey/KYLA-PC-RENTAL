using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
namespace Kyla;
// The service runs as LocalSystem, but the UI is created with the customer's own token.
public sealed class InteractiveLauncher(string customerSid,string executable){
 public void EnsureRunning(){
  uint session=WTSGetActiveConsoleSessionId();if(session==uint.MaxValue)return;
  if(!WTSQueryUserToken(session,out var token))return;
  try{
   using var identity=new WindowsIdentity(token);if(identity.User?.Value!=customerSid)return;
   string full=Path.GetFullPath(executable);
   foreach(var p in Process.GetProcessesByName("Kyla.Client")){using(p){try{if(p.SessionId==(int)session&&string.Equals(p.MainModule?.FileName,full,StringComparison.OrdinalIgnoreCase))return;}catch{}}}
   var si=new STARTUPINFO{cb=Marshal.SizeOf<STARTUPINFO>(),lpDesktop=@"winsta0\default"};
   IntPtr environment=IntPtr.Zero;
   try{
    if(!CreateEnvironmentBlock(out environment,token,false))throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    if(!CreateProcessAsUser(token,full,new StringBuilder('"'+full+'"'),IntPtr.Zero,IntPtr.Zero,false,0x400,environment,Path.GetDirectoryName(full),ref si,out var process))throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
    CloseHandle(process.hProcess);CloseHandle(process.hThread);
   }finally{if(environment!=IntPtr.Zero)DestroyEnvironmentBlock(environment);}
  }finally{CloseHandle(token);}
 }
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]private struct STARTUPINFO{public int cb;public string? lpReserved;public string? lpDesktop;public string? lpTitle;public int dwX,dwY,dwXSize,dwYSize,dwXCountChars,dwYCountChars,dwFillAttribute,dwFlags;public short wShowWindow,cbReserved2;public IntPtr lpReserved2,hStdInput,hStdOutput,hStdError;}
 [StructLayout(LayoutKind.Sequential)]private struct PROCESS_INFORMATION{public IntPtr hProcess,hThread;public uint dwProcessId,dwThreadId;}
 [DllImport("kernel32.dll")]private static extern uint WTSGetActiveConsoleSessionId();
 [DllImport("wtsapi32.dll",SetLastError=true)]private static extern bool WTSQueryUserToken(uint session,out IntPtr token);
 [DllImport("userenv.dll",SetLastError=true)]private static extern bool CreateEnvironmentBlock(out IntPtr environment,IntPtr token,bool inherit);
 [DllImport("userenv.dll")]private static extern bool DestroyEnvironmentBlock(IntPtr environment);
 [DllImport("advapi32.dll",CharSet=CharSet.Unicode,SetLastError=true)]private static extern bool CreateProcessAsUser(IntPtr token,string app,StringBuilder command,IntPtr processAttributes,IntPtr threadAttributes,bool inherit,uint flags,IntPtr environment,string? directory,ref STARTUPINFO startup,out PROCESS_INFORMATION process);
 [DllImport("kernel32.dll")]private static extern bool CloseHandle(IntPtr handle);
}
