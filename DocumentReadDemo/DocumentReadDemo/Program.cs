using DocumentReadDemo.Services;
using DocumentReadDemo.Models;

var builder = WebApplication.CreateBuilder(args);

// 添加配置
builder.Services.AddRazorPages();

// 添加服务
builder.Services.AddScoped<DocumentService>();
builder.Services.AddControllers();

// 添加CORS策略（允许前端访问）
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {
//        // JWT配置
//    });

var app = builder.Build();

// 配置中间件
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();

// 默认路由到首页
app.MapFallbackToFile("index.html");

app.Run();
