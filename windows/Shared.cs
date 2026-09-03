using System.Text.Json;
namespace Kyla;
public sealed record Pairing(string BaseUrl,string DeviceToken,string CustomerSid);
public sealed record Command(string Action,string[]? Codes=null,string? RequestId=null);
public sealed record Status(long RemainingMs,bool Maintenance,string Message,bool Online);
public sealed record CloudState(string Name,long Expires,long ServerNow,bool Maintenance);
public static class Wire {
 public const string PipeName="KylaRental.v1";
 public static readonly JsonSerializerOptions Json=new(JsonSerializerDefaults.Web);
 public static async Task<T> Read<T>(Stream stream,CancellationToken ct){
  var prefix=new byte[4];await stream.ReadExactlyAsync(prefix,ct);int length=BitConverter.ToInt32(prefix);
  if(length<1||length>8192)throw new IOException("Invalid message size");
  var bytes=new byte[length];await stream.ReadExactlyAsync(bytes,ct);
  return JsonSerializer.Deserialize<T>(bytes,Json)??throw new IOException("Invalid message");
 }
 public static async Task Write<T>(Stream stream,T value,CancellationToken ct){
  var bytes=JsonSerializer.SerializeToUtf8Bytes(value,Json);if(bytes.Length>8192)throw new IOException("Message too large");
  await stream.WriteAsync(BitConverter.GetBytes(bytes.Length),ct);await stream.WriteAsync(bytes,ct);await stream.FlushAsync(ct);
 }
}
