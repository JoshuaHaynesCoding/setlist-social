using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SetlistSocial.Api.Migrations
{
    /// <inheritdoc />
    public partial class SyncCurrentModelForProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                return;
            }

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "WishlistItems",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.Sql("""
                ALTER TABLE "WishlistItems"
                ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone
                USING ("UpdatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "SourceUrl",
                table: "WishlistItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SourceName",
                table: "WishlistItems",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "WishlistItems",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE "WishlistItems"
                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
                USING ("CreatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ArtistId",
                table: "WishlistItems",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "WishlistItems",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("""
                ALTER TABLE "UserProfiles"
                ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone
                USING ("UpdatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "OAuthSubject",
                table: "UserProfiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "UserProfiles",
                type: "character varying(120)",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 120);

            migrationBuilder.Sql("""
                ALTER TABLE "UserProfiles"
                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
                USING ("CreatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "UserProfiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "UserProfiles",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("""
                ALTER TABLE "Tags"
                ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone
                USING ("UpdatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tags",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 80);

            migrationBuilder.Sql("""
                ALTER TABLE "Tags"
                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
                USING ("CreatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Tags",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "Reviews",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.Sql("""
                ALTER TABLE "Reviews"
                ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone
                USING ("UpdatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "Reviews",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.Sql("""
                ALTER TABLE "Reviews"
                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
                USING ("CreatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ConcertId",
                table: "Reviews",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "Reviews",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Reviews",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "TagsId",
                table: "ConcertTag",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "ConcertsId",
                table: "ConcertTag",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "VenueName",
                table: "Concerts",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "Concerts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.Sql("""
                ALTER TABLE "Concerts"
                ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone
                USING ("UpdatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Concerts",
                type: "character varying(240)",
                maxLength: 240,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 240);

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Concerts",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE "Concerts"
                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
                USING ("CreatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Concerts",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE "Concerts"
                ALTER COLUMN "ConcertDate" TYPE timestamp with time zone
                USING ("ConcertDate"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Concerts",
                type: "character varying(120)",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ArtistId",
                table: "Concerts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Concerts",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("""
                ALTER TABLE "Artists"
                ALTER COLUMN "UpdatedAt" TYPE timestamp with time zone
                USING ("UpdatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Artists",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "LastFmUrl",
                table: "Artists",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE "Artists"
                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
                USING ("CreatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Artists",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "ActivityEvents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "ActivityEvents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "ActivityEvents",
                type: "character varying(80)",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 80);

            migrationBuilder.Sql("""
                ALTER TABLE "ActivityEvents"
                ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
                USING ("CreatedAt"::timestamp without time zone AT TIME ZONE 'UTC');
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ConcertId",
                table: "ActivityEvents",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ActivityEvents",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            if (migrationBuilder.ActiveProvider != "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                return;
            }

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "WishlistItems",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql("""
                ALTER TABLE "WishlistItems"
                ALTER COLUMN "UpdatedAt" TYPE text
                USING ("UpdatedAt"::text);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "SourceUrl",
                table: "WishlistItems",
                type: "TEXT",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SourceName",
                table: "WishlistItems",
                type: "TEXT",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "WishlistItems",
                type: "TEXT",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE "WishlistItems"
                ALTER COLUMN "CreatedAt" TYPE text
                USING ("CreatedAt"::text);
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ArtistId",
                table: "WishlistItems",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "WishlistItems",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("""
                ALTER TABLE "UserProfiles"
                ALTER COLUMN "UpdatedAt" TYPE text
                USING ("UpdatedAt"::text);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "OAuthSubject",
                table: "UserProfiles",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "DisplayName",
                table: "UserProfiles",
                type: "TEXT",
                maxLength: 120,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120);

            migrationBuilder.Sql("""
                ALTER TABLE "UserProfiles"
                ALTER COLUMN "CreatedAt" TYPE text
                USING ("CreatedAt"::text);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Bio",
                table: "UserProfiles",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "UserProfiles",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("""
                ALTER TABLE "Tags"
                ALTER COLUMN "UpdatedAt" TYPE text
                USING ("UpdatedAt"::text);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Tags",
                type: "TEXT",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80);

            migrationBuilder.Sql("""
                ALTER TABLE "Tags"
                ALTER COLUMN "CreatedAt" TYPE text
                USING ("CreatedAt"::text);
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Tags",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "Reviews",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql("""
                ALTER TABLE "Reviews"
                ALTER COLUMN "UpdatedAt" TYPE text
                USING ("UpdatedAt"::text);
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "Reviews",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql("""
                ALTER TABLE "Reviews"
                ALTER COLUMN "CreatedAt" TYPE text
                USING ("CreatedAt"::text);
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ConcertId",
                table: "Reviews",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Body",
                table: "Reviews",
                type: "TEXT",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Reviews",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "TagsId",
                table: "ConcertTag",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "ConcertsId",
                table: "ConcertTag",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "VenueName",
                table: "Concerts",
                type: "TEXT",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "Concerts",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.Sql("""
                ALTER TABLE "Concerts"
                ALTER COLUMN "UpdatedAt" TYPE text
                USING ("UpdatedAt"::text);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Concerts",
                type: "TEXT",
                maxLength: 240,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(240)",
                oldMaxLength: 240);

            migrationBuilder.AlterColumn<string>(
                name: "Region",
                table: "Concerts",
                type: "TEXT",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE "Concerts"
                ALTER COLUMN "CreatedAt" TYPE text
                USING ("CreatedAt"::text);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Concerts",
                type: "TEXT",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE "Concerts"
                ALTER COLUMN "ConcertDate" TYPE text
                USING ("ConcertDate"::text);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Concerts",
                type: "TEXT",
                maxLength: 120,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(120)",
                oldMaxLength: 120,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ArtistId",
                table: "Concerts",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Concerts",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.Sql("""
                ALTER TABLE "Artists"
                ALTER COLUMN "UpdatedAt" TYPE text
                USING ("UpdatedAt"::text);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Artists",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "LastFmUrl",
                table: "Artists",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.Sql("""
                ALTER TABLE "Artists"
                ALTER COLUMN "CreatedAt" TYPE text
                USING ("CreatedAt"::text);
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Artists",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<int>(
                name: "UserProfileId",
                table: "ActivityEvents",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "ActivityEvents",
                type: "TEXT",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "EventType",
                table: "ActivityEvents",
                type: "TEXT",
                maxLength: 80,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(80)",
                oldMaxLength: 80);

            migrationBuilder.Sql("""
                ALTER TABLE "ActivityEvents"
                ALTER COLUMN "CreatedAt" TYPE text
                USING ("CreatedAt"::text);
                """);

            migrationBuilder.AlterColumn<int>(
                name: "ConcertId",
                table: "ActivityEvents",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "ActivityEvents",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }
    }
}
