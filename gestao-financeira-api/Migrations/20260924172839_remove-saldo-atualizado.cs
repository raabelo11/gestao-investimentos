using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoFinanceira.Migrations
{
    /// <inheritdoc />
    public partial class removesaldoatualizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SaldoAtualizado",
                table: "Caixinhas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "SaldoAtualizado",
                table: "Caixinhas",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
