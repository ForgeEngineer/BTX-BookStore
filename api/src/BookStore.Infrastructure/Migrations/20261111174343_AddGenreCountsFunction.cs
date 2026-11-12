using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookStore.Infrastructure.Migrations
{
    public partial class AddGenreCountsFunction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE OR REPLACE FUNCTION get_genre_counts()
                RETURNS TABLE (genre text, count integer)
                LANGUAGE sql
                STABLE
                AS $$
                    SELECT "Genre", COUNT(*)::integer
                    FROM "Books"
                    GROUP BY "Genre"
                    ORDER BY "Genre";
                $$;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_genre_counts();");
        }
    }
}
