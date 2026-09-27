using ClientEngagementFlow.Infrastructure;
using ClientEngagementFlow.Worker;

var builder = Host.CreateApplicationBuilder(args);

_ = builder.Configuration["InternalApi:BaseUrl"] ?? throw new InvalidOperationException("InternalApi:BaseUrl was not found.");
_ = builder.Configuration["InternalApi:NotificationApiKey"] ?? throw new InvalidOperationException("InternalApi:NotificationApiKey was not found.");

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddHostedService<Worker>();
builder.Services.AddHttpClient();

var host = builder.Build();
host.Run();
