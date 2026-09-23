using GestaoFinanceira.Model;
using Microsoft.EntityFrameworkCore;

namespace GestaoFinanceira.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Caixinha> Caixinhas => Set<Caixinha>();
        public DbSet<Movimentacao> Movimentacoes => Set<Movimentacao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Caixinha>(entity =>
            {
                entity.Property(c => c.Nome).IsRequired().HasMaxLength(80);
                entity.Property(c => c.Descricao).HasMaxLength(280);
                entity.Property(c => c.Cor).HasMaxLength(9);
                entity.Property(c => c.Icone).HasMaxLength(40);
                // SQLite nao tem tipo decimal nativo. Guardamos como TEXT invariante
                // para preservar precisao exata (calculos monetarios sao feitos em memoria).
                entity.Property(c => c.Meta)
                    .HasConversion(
                        v => v.HasValue ? v.Value.ToString(System.Globalization.CultureInfo.InvariantCulture) : null,
                        v => string.IsNullOrEmpty(v) ? (decimal?)null : decimal.Parse(v, System.Globalization.CultureInfo.InvariantCulture));
            });

            modelBuilder.Entity<Movimentacao>(entity =>
            {
                entity.Property(m => m.Observacao).HasMaxLength(280);
                entity.Property(m => m.Valor)
                    .HasConversion(
                        v => v.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        v => decimal.Parse(v, System.Globalization.CultureInfo.InvariantCulture));
                entity.HasOne(m => m.Caixinha)
                    .WithMany(c => c.Movimentacoes)
                    .HasForeignKey(m => m.CaixinhaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(m => new { m.CaixinhaId, m.Data });
            });
        }
    }
}
