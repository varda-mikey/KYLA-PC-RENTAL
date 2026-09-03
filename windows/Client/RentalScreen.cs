using System.IO.Pipes;
using System.Runtime.InteropServices;
namespace Kyla;
public sealed class RentalScreen:Form {
 private readonly System.Windows.Forms.Timer timer=new(){Interval=1000};
 private readonly Label title=new(){Text="PLEASE ENTER YOUR VOUCHER",ForeColor=Color.White,AutoSize=true,Font=new Font("Segoe UI",26,FontStyle.Bold)};
 private readonly Label countdown=new(){Text="00:00:00",AutoSize=true,ForeColor=Color.FromArgb(120,170,255),Font=new Font("Segoe UI",32,FontStyle.Bold)};
 private readonly TextBox codes=new(){Multiline=true,Width=480,Height=100,Font=new Font("Consolas",16),MaxLength=800};
 private readonly Label info=new(){ForeColor=Color.LightGray,MaximumSize=new Size(500,0),AutoSize=true,Text="Buy a voucher from the admin. Enter one code per line."};
 private readonly Button redeem=new(){Text="REDEEM / EXTEND TIME",AutoSize=true,Padding=new Padding(18,10,18,10),BackColor=Color.RoyalBlue,ForeColor=Color.White,FlatStyle=FlatStyle.Flat};
 private readonly Button back=new(){Text="Back to game",AutoSize=true,Padding=new Padding(12)};
 private readonly FlowLayoutPanel panel=new(){AutoSize=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(25)};
 private readonly List<Form> covers=[];
 private readonly Form hud=new(){FormBorderStyle=FormBorderStyle.None,TopMost=true,ShowInTaskbar=false,Size=new Size(295,48),BackColor=Color.FromArgb(15,26,46)};
 private readonly Button hudButton=new(){Dock=DockStyle.Fill,FlatStyle=FlatStyle.Flat,ForeColor=Color.White,BackColor=Color.FromArgb(15,26,46)};
 private bool busy,locked=true,warningShown,extension,closing,arranged;
 private InputGuard? guard;
 private long remaining,lastStatus;
 private bool maintenance;
 private Command? retry;
 public RentalScreen(){
  Text="KYLA Piso Net";BackColor=Color.FromArgb(7,12,22);ForeColor=Color.White;Font=new Font("Segoe UI",12);
  FormBorderStyle=FormBorderStyle.None;TopMost=true;ShowInTaskbar=false;StartPosition=FormStartPosition.Manual;KeyPreview=true;
  panel.Controls.Add(new Label{Text="KYLA PISO NET",AutoSize=true,ForeColor=Color.LightBlue,Margin=new Padding(0,0,0,25)});
  foreach(Control c in new Control[]{title,countdown,info,codes,redeem,back}){c.Margin=new Padding(0,8,0,8);panel.Controls.Add(c);}Controls.Add(panel);
  hud.Controls.Add(hudButton);hudButton.Click+=(_,_)=>{extension=true;Show();Activate();};
  back.Click+=(_,_)=>{if(!locked){extension=false;Hide();}};
  redeem.Click+=async(_,_)=>await Redeem();
  FormClosing+=(_,e)=>{if(!closing&&e.CloseReason==CloseReason.UserClosing)e.Cancel=true;};
  Resize+=(_,_)=>CenterPanel();
  Shown+=(_,_)=>{guard=new InputGuard(()=>locked);SetBlocked(true);timer.Start();};
  timer.Tick+=async(_,_)=>await Tick();
  Microsoft.Win32.SystemEvents.DisplaySettingsChanged+=DisplayChanged;
 }
 private void DisplayChanged(object? s,EventArgs e){if(!IsDisposed)BeginInvoke(()=>{RebuildCovers();LayoutScreen();});}
 private void CenterPanel(){panel.MaximumSize=new Size(Math.Max(250,ClientSize.Width-30),0);codes.Width=Math.Min(480,Math.Max(220,ClientSize.Width-70));info.MaximumSize=new Size(codes.Width,0);panel.Left=Math.Max(0,(ClientSize.Width-panel.Width)/2);panel.Top=Math.Max(0,(ClientSize.Height-panel.Height)/2);}
 private void LayoutScreen(){var screen=Screen.PrimaryScreen??Screen.AllScreens[0];Bounds=locked?screen.Bounds:new Rectangle(screen.WorkingArea.Left+Math.Max(0,(screen.WorkingArea.Width-650)/2),screen.WorkingArea.Top+40,Math.Min(650,screen.WorkingArea.Width),Math.Min(600,screen.WorkingArea.Height));hud.Location=new Point(screen.WorkingArea.Right-hud.Width-12,screen.WorkingArea.Top+12);CenterPanel();}
 private void RebuildCovers(){foreach(var c in covers)c.Dispose();covers.Clear();if(!locked)return;foreach(var screen in Screen.AllScreens.Where(s=>!s.Primary)){var c=new Form{BackColor=Color.Black,FormBorderStyle=FormBorderStyle.None,StartPosition=FormStartPosition.Manual,Bounds=screen.Bounds,TopMost=true,ShowInTaskbar=false};c.FormClosing+=(_,e)=>{if(!closing&&e.CloseReason==CloseReason.UserClosing)e.Cancel=true;};c.Show();covers.Add(c);}}
 private void SetBlocked(bool value){bool changed=locked!=value||!IsHandleCreated;locked=value;back.Visible=!locked;title.Text=locked?"PLEASE ENTER YOUR VOUCHER":"EXTEND YOUR TIME";
  if(changed||!arranged){arranged=true;RebuildCovers();LayoutScreen();}
  if(locked){hud.Hide();if(!Visible)Show();WindowState=FormWindowState.Normal;TopMost=true;BringToFront();Activate();SetForegroundWindow(Handle);foreach(var c in covers)c.TopMost=true;}
  else{if(!extension)Hide();if(!hud.Visible)hud.Show();}
 }
 private async Task<Status> Send(Command cmd){using var ct=new CancellationTokenSource(14000);using var pipe=new NamedPipeClientStream(".",Wire.PipeName,PipeDirection.InOut,PipeOptions.Asynchronous);await pipe.ConnectAsync(ct.Token);await Wire.Write(pipe,cmd,ct.Token);return await Wire.Read<Status>(pipe,ct.Token);}
 private void Apply(Status s){remaining=s.RemainingMs;maintenance=s.Maintenance;lastStatus=Environment.TickCount64;if(!string.IsNullOrEmpty(s.Message))info.Text=s.Message;else info.Text="Buy a voucher from the admin. Enter one code per line.";if(remaining>300000)warningShown=false;}
 private async Task Tick(){
  // Local rendering expires independently even while a cloud redemption is waiting.
  var left=Math.Max(0,remaining-(Environment.TickCount64-lastStatus));bool stale=Environment.TickCount64-lastStatus>16000;
  SetBlocked(stale||(!maintenance&&left<=0));
  string clock=TimeSpan.FromMilliseconds(left).ToString(@"hh\:mm\:ss");if(left>=86400000)clock=$"{(int)(left/3600000)}:"+TimeSpan.FromMilliseconds(left).ToString(@"mm\:ss");
  countdown.Text=maintenance?"MAINTENANCE":clock;hudButton.Text=maintenance?"KYLA · Maintenance mode":$"{clock}   |   Extend time";
  if(!locked&&!maintenance&&left<=300000&&!warningShown){warningShown=true;hudButton.BackColor=Color.DarkOrange;hudButton.Text="5 MINUTES LEFT · Click to extend";}else if(left>300000)hudButton.BackColor=Color.FromArgb(15,26,46);
  if(busy)return;busy=true;try{Apply(await Send(new("status")));}catch{info.Text="Connecting to the rental service. Please contact the admin if this continues.";}finally{busy=false;}
 }
 private async Task Redeem(){if(busy)return;var list=codes.Text.Split(new[]{'\r','\n',',',';'},StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries);if(list.Length==0||list.Length>20){info.Text="Enter 1–20 codes, one per line.";return;}
  busy=true;redeem.Enabled=false;try{retry??=new("redeem",list,Guid.NewGuid().ToString());var result=await Send(retry);Apply(result);retry=null;if(result.Message.StartsWith("Voucher accepted")){codes.Clear();extension=false;}}catch{info.Text="Checking your voucher. Retry this submission; time will not be added twice.";}finally{busy=false;redeem.Enabled=true;}
 }
 protected override void Dispose(bool disposing){if(disposing){closing=true;guard?.Dispose();timer.Dispose();Microsoft.Win32.SystemEvents.DisplaySettingsChanged-=DisplayChanged;foreach(var c in covers)c.Dispose();hud.Dispose();}base.Dispose(disposing);}
 [DllImport("user32.dll")]private static extern bool SetForegroundWindow(IntPtr handle);
}
