using System.Data.Common;
using JamesBrighton.Data.GrpcServer.Services;
using FirebirdSql.Data.FirebirdClient;
using JamesBrighton.Data.Common;

const string providerInvariantName = "FirebirdSql.Data.FirebirdClient";
DbProviderFactories.RegisterFactory(providerInvariantName, FirebirdClientFactory.Instance);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(o => o.AddPolicy("AllowAll", x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders("Grpc-Status", "Grpc-Message", "Grpc-Encoding", "Grpc-Accept-Encoding")));
builder.Services.AddGrpc();

var app = builder.Build();

app.UseGrpcWeb();
app.UseCors();

app.MapGrpcService<DatabaseService>().EnableGrpcWeb().RequireCors("AllowAll");
app.MapGet("/", () =>"Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();