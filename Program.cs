using Microsoft.Extensions.DependencyInjection;
using ChoreMgr.Data;
using ChoreMgr.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.Configure<ChoreDatabaseSettings>(builder.Configuration.GetSection(nameof(ChoreDatabaseSettings)));

builder.Services.AddSingleton<IChoreDatabaseSettings>(sp => sp.GetRequiredService<IOptions<ChoreDatabaseSettings>>().Value);


builder.Services.AddSingleton<ChoreService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
