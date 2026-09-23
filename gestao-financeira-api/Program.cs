using System.Text.Json.Serialization;
using GestaoFinanceira.Data;
using GestaoFinanceira.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "FrontendPolicy";

// Controllers: enums serializados como string (ex.: "Aporte") para o front.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Persistencia: SQLite via connection string do appsettings.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=gestaofinanceira.db";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<CalculoInvestimentoService>();

// CORS liberado para o front Angular em desenvolvimento.
var origensPermitidas = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.WithOrigins(origensPermitidas)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

// Aplica migrations pendentes automaticamente na subida.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Backfill idempotente: congela o rendimento das fotos de saldo legadas
    // (RendimentoCongelado ainda nulo), preservando o valor historico correto.
    var calculo = scope.ServiceProvider.GetRequiredService<CalculoInvestimentoService>();
    CongelarFotosLegadas(db, calculo);
}

static void CongelarFotosLegadas(AppDbContext db, CalculoInvestimentoService calculo)
{
    var fotosPendentes = db.Movimentacoes
        .Where(m => m.Tipo == GestaoFinanceira.Model.TipoMovimentacao.Saldo
                    && m.RendimentoCongelado == null)
        .ToList();

    if (fotosPendentes.Count == 0)
    {
        return;
    }

    // Agrupa por caixinha para calcular o saldo esperado usando as demais movimentacoes.
    foreach (var grupo in fotosPendentes.GroupBy(m => m.CaixinhaId))
    {
        var todasDaCaixinha = db.Movimentacoes
            .Where(m => m.CaixinhaId == grupo.Key)
            .ToList();

        foreach (var foto in grupo)
        {
            foto.RendimentoCongelado = calculo.CalcularRendimentoCongelado(
                foto.Valor, foto.Data, foto.Id, todasDaCaixinha);
        }
    }

    db.SaveChanges();
}

app.UseCors(p => p
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors(CorsPolicy);
app.UseAuthorization();
app.MapControllers();

app.Run();
