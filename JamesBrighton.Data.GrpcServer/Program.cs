using System.Data.Common;
using JamesBrighton.Data.GrpcServer.Services;
using FirebirdSql.Data.FirebirdClient;

const string providerInvariantName = "FirebirdSql.Data.FirebirdClient";
DbProviderFactories.RegisterFactory(providerInvariantName, FirebirdClientFactory.Instance);

var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();
app.UseGrpcWeb();
// Configure the HTTP request pipeline.
app.MapGrpcService<DatabaseService>().EnableGrpcWeb();
app.MapGet("/", () =>"Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();