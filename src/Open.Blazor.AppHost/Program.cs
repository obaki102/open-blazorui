
var builder = DistributedApplication.CreateBuilder(args);

var ollama = builder.AddOllama("ollama", null)
    .WithDataVolume()
    .WithContainerRuntimeArgs("--gpus=all")
    .PublishAsContainer()
    .AddModel("phi3.5");

var openBlazorUi = builder.AddProject<Projects.Open_Blazor_Ui>("openblazorui")
    .WithExternalHttpEndpoints()
    .WithReference(ollama);
    
builder.Build().Run();