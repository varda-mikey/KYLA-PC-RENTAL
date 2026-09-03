using Kyla;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
var builder=Host.CreateApplicationBuilder(args);
builder.Services.AddWindowsService(options=>options.ServiceName="KylaRental");
builder.Services.AddHostedService<RentalAgent>();
await builder.Build().RunAsync();
