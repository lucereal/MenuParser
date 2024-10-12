using MenuParser.Domain.ExternalServices.impl;
using MenuParser.Domain.ExternalServices.inter;
using MenuParser.Services.impl;
using MenuParser.Services.inter;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets<Program>()
    .Build();

string? connectionString = configuration["connectionString"];


builder.Services.AddCors();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IMenuIntelligenceService, MenuIntelligenceService>();
builder.Services.AddScoped<IOpenAIClient, OpenAIClient>();
builder.Services.AddScoped<IGoogleSearchClient, GoogleSearchClient>();
builder.Services.AddScoped<IWebSearchService, WebSearchService>();
builder.Services.AddScoped<HttpClient, HttpClient>();
//builder.Services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));
//builder.Services.AddSingleton<IMongoDBContext>(new MongoDBContext(MongoClientSettings.FromUrl(new MongoUrl(connectionString))));

//builder.Services.AddSignalR();
//.AddJsonProtocol();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(builder =>
{
    //builder.AllowAnyOrigin()
    //       .AllowAnyMethod()
    //       .AllowAnyHeader();
    builder.AllowAnyMethod()
       .AllowAnyHeader()
       .AllowCredentials()
       .WithOrigins("http://localhost:3000");
});


app.UseRouting();
app.UseAuthorization();


#pragma warning disable ASP0014 // Suggest using top level route registrations
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    //endpoints.MapHub<ChatHub>("/chatHub");
});
#pragma warning restore ASP0014 // Suggest using top level route registrations




app.Run();
