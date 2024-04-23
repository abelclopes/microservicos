global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Configuration;
global using Microsoft.IdentityModel.Tokens;
global using System;
global using System.Text;
global using AuthService.Data;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Http;
global using Microsoft.AspNetCore.Mvc;
global using MongoDB.Bson;
global using MongoDB.Driver;
using AuthService.Data;

var builder = WebApplication.CreateBuilder(args);

// Configuração do JWT
var key = Encoding.ASCII.GetBytes("oiqhowhe012eh108fh9=1gf9137198e9d1d9"); // Substitua pela sua chave secreta
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Configuração do MongoDB
var connectionString = builder.Configuration.GetConnectionString("MongoDB:ConnectionString");
var databaseName = builder.Configuration.GetConnectionString("MongoDB:DatabaseName");
builder.Services.AddSingleton<IMongoClient>(s => new MongoClient(connectionString));
builder.Services.AddScoped<IMongoDatabase>(s => s.GetRequiredService<IMongoClient>().GetDatabase(databaseName));
builder.Services.AddTransient<IMongoAuthDbContext, MongoAuthDbContext>();
builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
