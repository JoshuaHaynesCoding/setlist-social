using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SetlistSocial.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWishlistSourceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SourceName",
                table: "WishlistItems",
                type: "TEXT",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                table: "WishlistItems",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SourceName",
                table: "WishlistItems");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                table: "WishlistItems");
        }
    }
}
