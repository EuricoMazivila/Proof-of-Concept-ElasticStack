using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.File;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .WriteTo.Console()
        .WriteTo.Elasticsearch(
            new ElasticsearchSinkOptions(new Uri(context.Configuration["ElasticConfiguration:Uri"])) 
            {
                AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.ESv7,
                BatchAction = ElasticOpType.Create,
                TypeName = null,
                IndexFormat = 
                    $"{context.Configuration["ApplicationName"]}-" +
                    $"logs-{context.HostingEnvironment.EnvironmentName?.ToLower().Replace(".", "-")}-" +
                    $"{DateTime.UtcNow:yyyy-MM}", 
                AutoRegisterTemplate = true, 
                NumberOfShards = 2, 
                NumberOfReplicas = 1,
                ModifyConnectionSettings = x => 
                    x.BasicAuthentication(
                        context.Configuration["ElasticConfiguration:Username"], 
                        context.Configuration["ElasticConfiguration:Password"])
                        .ServerCertificateValidationCallback((sender, certificate,chain,errors) => true),
                EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                   EmitEventFailureHandling.WriteToFailureSink |
                                   EmitEventFailureHandling.RaiseCallback,
                FailureSink = new FileSink(context.Configuration["LogElasticStackPath"], new JsonFormatter(), null)
            })
        .Enrich.WithProperty("Envirnoment", context.HostingEnvironment.EnvironmentName)
        .ReadFrom.Configuration(context.Configuration);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();