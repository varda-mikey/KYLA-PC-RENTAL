using System.IO.Pipes;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
namespace Kyla;
public sealed class RentalAgent(ILogger<RentalAgent> log):BackgroundService {
 private readonly HttpClient http=new(new HttpClientHandler{AllowAutoRedirect=false}){Timeout=TimeSpan.FromSeconds(10)};
 private readonly SemaphoreSlim gate=new(1,1);
 private Pairing config=null!;
 private long deadline,lastCloud;
 private bool maintenance,online;
 private string message="Connecting to rental server…";
 private readonly string folder=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),"KylaRental");
 private string PendingPath=>Path.Combine(folder,"pending.json");
 private Command? pending;
 private Status Current(string? msg=null)=>new(Math.Max(0,deadline-Environment.TickCount64),maintenance&&Environment.TickCount64-lastCloud<30000,msg??message,online);
 protected override async Task ExecuteAsync(CancellationToken stop){
  config=JsonSerializer.Deserialize<Pairing>(await File.ReadAllTextAsync(Path.Combine(folder,"config.json"),stop),Wire.Json)??throw new InvalidOperationException("Missing pairing");
  if(!Uri.TryCreate(config.BaseUrl,UriKind.Absolute,out var baseUri)||baseUri.Scheme!="https"||!string.IsNullOrEmpty(baseUri.UserInfo))throw new InvalidOperationException("Pairing requires HTTPS");
  _=new SecurityIdentifier(config.CustomerSid);
  http.BaseAddress=new Uri(baseUri.GetLeftPart(UriPartial.Authority)+"/");
  http.DefaultRequestHeaders.Authorization=new AuthenticationHeaderValue("Bearer",config.DeviceToken);
  if(File.Exists(PendingPath))pending=JsonSerializer.Deserialize<Command>(await File.ReadAllTextAsync(PendingPath,stop),Wire.Json);
  // No persisted unlocked state: after a service/PC restart, get authoritative time online first.
  await Task.WhenAll(PollCloud(stop),Listen(stop),WatchClient(stop));
 }
 private async Task PollCloud(CancellationToken stop){while(!stop.IsCancellationRequested){
  await gate.WaitAsync(stop);try{if(pending!=null)await Redeem(pending,stop);else await Refresh(stop);}catch(Exception ex)when(ex is not OperationCanceledException||!stop.IsCancellationRequested){online=false;message="Offline. Paid time keeps counting; reconnect to add a voucher.";log.LogWarning("Rental server unavailable: {Type}",ex.GetType().Name);}finally{gate.Release();}
  await Task.Delay(10000,stop);
 }}
 private void Apply(CloudState s){deadline=Environment.TickCount64+Math.Max(0,s.Expires-s.ServerNow);maintenance=s.Maintenance;lastCloud=Environment.TickCount64;online=true;message="";}
 private async Task Refresh(CancellationToken ct){
  using var response=await http.GetAsync("api/device",ct);
  if(response.StatusCode==HttpStatusCode.Unauthorized){deadline=0;maintenance=false;online=false;message="This PC pairing is disabled. Please contact the admin.";return;}
  response.EnsureSuccessStatusCode();Apply(await response.Content.ReadFromJsonAsync<CloudState>(Wire.Json,ct)??throw new IOException("Invalid server response"));
 }
 private async Task Redeem(Command cmd,CancellationToken ct){
  using var response=await http.PostAsJsonAsync("api/device",new {codes=cmd.Codes,requestId=cmd.RequestId},Wire.Json,ct);
  if(response.IsSuccessStatusCode){Apply(await response.Content.ReadFromJsonAsync<CloudState>(Wire.Json,ct)??throw new IOException("Invalid response"));pending=null;File.Delete(PendingPath);message="Voucher accepted. Your time has been added.";return;}
  var error="Could not redeem. Please contact the admin.";
  try{using var json=JsonDocument.Parse(await response.Content.ReadAsStringAsync(ct));error=json.RootElement.GetProperty("error").GetString()??error;}catch(JsonException){}catch(KeyNotFoundException){}
  if((int)response.StatusCode>=500||(int)response.StatusCode==429){message=error;online=false;return;}
  pending=null;File.Delete(PendingPath);message=error;
  if(response.StatusCode==HttpStatusCode.Unauthorized){deadline=0;maintenance=false;online=false;}
 }
 private async Task Listen(CancellationToken stop){
  var security=new PipeSecurity();security.SetAccessRuleProtection(true,false);
  security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid,null),PipeAccessRights.FullControl,AccessControlType.Allow));
  security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(config.CustomerSid),PipeAccessRights.ReadWrite,AccessControlType.Allow));
  while(!stop.IsCancellationRequested){
   using var pipe=NamedPipeServerStreamAcl.Create(Wire.PipeName,PipeDirection.InOut,1,PipeTransmissionMode.Byte,PipeOptions.Asynchronous,8192,8192,security);
   await pipe.WaitForConnectionAsync(stop);
   using var timeout=CancellationTokenSource.CreateLinkedTokenSource(stop);timeout.CancelAfter(15000);
   try{var cmd=await Wire.Read<Command>(pipe,timeout.Token);
    if(cmd.Action=="status"){await Wire.Write(pipe,Current(),timeout.Token);continue;}
    if(cmd.Action!="redeem"||cmd.Codes is not {Length:>0 and <=20}||cmd.Codes.Any(c=>c==null||c.Length>40)||!Guid.TryParse(cmd.RequestId,out _)){await Wire.Write(pipe,Current("Enter valid voucher codes."),timeout.Token);continue;}
    await gate.WaitAsync(timeout.Token);try{
     if(pending==null){pending=cmd;await File.WriteAllTextAsync(PendingPath+".tmp",JsonSerializer.Serialize(cmd,Wire.Json),timeout.Token);File.Move(PendingPath+".tmp",PendingPath,true);}
     // A lost response keeps the original request on protected disk and retries that same request.
     await Redeem(pending,timeout.Token);
     await Wire.Write(pipe,Current(),timeout.Token);
    }finally{gate.Release();}
   }catch(Exception ex)when(!stop.IsCancellationRequested){log.LogWarning("Client request interrupted: {Type}",ex.GetType().Name);}
  }
 }
 private async Task WatchClient(CancellationToken stop){
  var launcher=new InteractiveLauncher(config.CustomerSid,Path.Combine(AppContext.BaseDirectory,"..","Client","Kyla.Client.exe"));
  while(!stop.IsCancellationRequested){try{launcher.EnsureRunning();}catch(Exception ex){log.LogWarning("Unable to start customer screen: {Message}",ex.Message);}await Task.Delay(2000,stop);}
 }
 public override void Dispose(){http.Dispose();gate.Dispose();base.Dispose();}
}
