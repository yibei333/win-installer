using CliFx;

var builder = new CliApplicationBuilder();
builder.AddCommandsFromThisAssembly();
builder.SetExecutableName("win-installer");

var app = builder.Build();
await app.RunAsync();