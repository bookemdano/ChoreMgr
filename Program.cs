﻿using Microsoft.Extensions.DependencyInjection;
using ChoreMgr.Data;
using ChoreMgr.Models;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

//builder.Services.Configure<ChoreMongoSettings>(builder.Configuration.GetSection(nameof(ChoreMongoSettings)));
//builder.Services.AddSingleton<IChoreMongoSettings>(sp => sp.GetRequiredService<IOptions<ChoreMongoSettings>>().Value);
//builder.Services.AddSingleton<ChoreMongo>();

builder.Services.Configure<ChoreJsonDbSettings>(builder.Configuration.GetSection(nameof(ChoreJsonDbSettings)));

builder.Services.AddSingleton<IChoreJsonDbSettings>(sp => sp.GetRequiredService<IOptions<ChoreJsonDbSettings>>().Value);

builder.Services.AddSingleton<ChoreJsonDb>();

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
