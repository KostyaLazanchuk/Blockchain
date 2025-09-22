using Blockchain.Core.Infrastructure;
using Blockchain.Core.Interfaces;
using Blockchain.Core.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IHashService, HashService>();

var powSuffix = builder.Configuration["Pow:TargetSuffix"] ?? "03";

builder.Services.AddSingleton<IProofOfWork>(sp =>
{
    var hashService = sp.GetRequiredService<IHashService>();
    return new ProofOfWorkService(hashService, powSuffix);
});

builder.Services.AddSingleton<IBlockRepository, BlockService>();


builder.Services.AddSingleton<BlockchainService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var bc = scope.ServiceProvider.GetRequiredService<BlockchainService>();
    var surname = app.Configuration["Blockchain:Genesis:Surname"] ?? "Lazanchuk";
    var nonce = app.Configuration["Blockchain:Genesis:Nonce"] ?? "03032001";
    bc.CreateGenesis(surname, nonce);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
