using FileServer2;
using FileServer2.Controllers;

var builder = WebApplication.CreateBuilder(args);
var startup = new Startup(builder.Configuration);

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

startup.Configure(app, app.Environment);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

ArquivoController oArquivoController = new ArquivoController(builder.Configuration);

Console.ForegroundColor = ConsoleColor.White;
oArquivoController.LogaErro("--------------------------------------------------------------------------------------",false);
oArquivoController.LogaErro("", false);
Console.ForegroundColor = ConsoleColor.DarkRed;
oArquivoController.LogaErro("8888888888 d8b 888           .d8888b.                                                ", false);
Console.ForegroundColor = ConsoleColor.Red;
oArquivoController.LogaErro("888        Y8P 888          d88P  Y88b                                               ", false);
Console.ForegroundColor = ConsoleColor.Yellow;
oArquivoController.LogaErro("888            888          Y88b.                                                    ", false);
Console.ForegroundColor = ConsoleColor.Green;
oArquivoController.LogaErro("8888888    888 888  .d88b.   \"Y888b.    .d88b.  888d888 888  888  .d88b.  888d888    ", false);
Console.ForegroundColor = ConsoleColor.Blue;
oArquivoController.LogaErro("888        888 888 d8P  Y8b     \"Y88b. d8P  Y8b 888P\"   888  888 d8P  Y8b 888P\"      ", false);
Console.ForegroundColor = ConsoleColor.DarkBlue;
oArquivoController.LogaErro("888        888 888 88888888       \"888 88888888 888     Y88  88P 88888888 888        ", false);
Console.ForegroundColor = ConsoleColor.Magenta;
oArquivoController.LogaErro("888        888 888 Y8b.     Y88b  d88P Y8b.     888      Y8bd8P  Y8b.     888        ", false);
Console.ForegroundColor = ConsoleColor.DarkMagenta;
oArquivoController.LogaErro("888        888 888  \"Y8888   \"Y8888P\"   \"Y8888  888       Y88P    \"Y8888  888        ", false);
Console.ForegroundColor = ConsoleColor.White;
oArquivoController.LogaErro("                                                                          Versão 1.0", false);
Console.ForegroundColor = ConsoleColor.White;
oArquivoController.LogaErro("--------------------------------------------------------------------------------------", false);
oArquivoController.LogaErro("", false);
Console.ForegroundColor = ConsoleColor.Gray;


app.Run();
