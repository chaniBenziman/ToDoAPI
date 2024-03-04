using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using TodoApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Todo API", Version = "v1" });
});

var connectionString = builder.Configuration.GetConnectionString("ToDoDB");
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.Parse("8.0.36-mysql")), ServiceLifetime.Singleton);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseCors("AllowAll");
// שליפת כל המשימות
app.MapGet("/tasks", async (ToDoDbContext dbContext, HttpContext httpContext) =>
{
    
    var tasks = await dbContext.Items.ToListAsync();
    await httpContext.Response.WriteAsJsonAsync(tasks);
});

// הוספת משימה חדשה
app.MapPost("/tasks", async (ToDoDbContext dbContext, HttpContext httpContext,Item item) =>
{
     item.IsComplete = false;
    dbContext.Add(item);
    await dbContext.SaveChangesAsync();
    httpContext.Response.StatusCode = StatusCodes.Status201Created;
    await httpContext.Response.WriteAsJsonAsync(item);
 
});

   // עדכון משימה
app.MapPut("/task/{id}", async (int id, ToDoDbContext dbContext, HttpContext httpContext,Item item) =>
{
 
  if (item == null)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync("the data isnt valid");
        return;
    }
    var exist = await dbContext.Items.FindAsync(id);
    if (exist == null)
    {
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    if (item.Name != null)
        exist.Name = item.Name;
    exist.IsComplete = item.IsComplete;
    await dbContext.SaveChangesAsync();
    httpContext.Response.StatusCode = StatusCodes.Status200OK;
});

 // מחיקת משימה
app.MapDelete("/tasks/{id}", async ( ToDoDbContext dbContext, HttpContext httpContext,int id) =>
{
   
    var itemToDelete = await dbContext.Items.FindAsync(id);
    if (itemToDelete == null)
    {
        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }

    dbContext.Items.Remove(itemToDelete);
    await dbContext.SaveChangesAsync();
});
app.Run();