var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (context) =>
    await context.Response.WriteAsync("<h1>Hello, mother fu*kers!</h1>"));

app.Run();
