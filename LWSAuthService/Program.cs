using LWSAuthService.Configuration;
using LWSAuthService.Repository;
using LWSAuthService.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var targetSection = builder.Configuration.GetSection("MongoSection").Get<MongoConfiguration>();
builder.Services.AddSingleton(targetSection);
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Scoped Service(Mostly Business Logic)
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<AccessTokenService>();

// Add Singleton Service(Mostly Data Logic)
builder.Services.AddSingleton<MongoContext>();
builder.Services.AddSingleton<IAccountRepository, AccountRepository>();
builder.Services.AddSingleton<IAccessTokenRepository, AccessTokenRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();