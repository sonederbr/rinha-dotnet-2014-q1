using Api.Endpoints;
using Api.Middlewares;
using Microsoft.AspNetCore.Rewrite;
using SourceJsonSerializerContext = Api.Contratos.SourceJsonSerializerContext;

var builder = WebApplication.CreateBuilder(args);

builder.AddAppSettingsEnvironment();

var connectionString = builder.Configuration.GetConnectionString("RinhaDbContext")!;
if (builder.Environment.IsProduction())
    connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

builder.Services.AddNpgsqlDataSource(
    connectionString: connectionString ?? "Erro, Adicione a variável de ambiente CONNECTION_STRING com a connection string."
);

builder.Services.AddProblemDetails();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, SourceJsonSerializerContext.Default);
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
});

builder.Services.AddTransient<GlobalExceptionHandlerMiddleware>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddMySwagger(builder);
}

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseRewriter(new RewriteOptions().AddRedirect("^$", "swagger"));
    app.UseMySwagger();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.AddObterExtratoPorClienteEndpoint(); // GET  /clientes/[id]/extrato 
app.AddCriarTransacaoPorClienteEndpoint(); // POST /clientes/[id]/transacoes
app.AddExecutarManutencaoBdEndpoint(); // POST /manutencao/reseta-bd

app.Run();