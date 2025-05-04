using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateProductDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "A gripping journey through time, where destiny and science intertwine. When a forgotten relic is uncovered, one man must solve ancient riddles to protect the fabric of reality.\r\n\r\nRich in suspense and imagination, this time-travel adventure blends mystery with powerful storytelling.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "A haunting tale of survival beneath an ever-darkening sky. As the world grapples with a mysterious eclipse, one woman uncovers a secret that could save—or destroy—humanity.\r\n\r\nWith atmospheric tension and emotional depth, this thriller keeps readers on edge.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "An emotional story of love and letting go, set against the backdrop of a coastal town. As past wounds resurface, a photographer and a writer find healing in unexpected places.\r\n\r\nA bittersweet romance that captures the fleeting beauty of second chances.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "A delightful tale that explores childhood nostalgia and the sweetness of dreams. Follow a group of friends as they navigate small-town joys, heartbreaks, and coming-of-age truths.\r\n\r\nPlayful and heartwarming, this story blends humor with gentle life lessons.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "Description",
                value: "Set on a remote island, this novel tells the story of resilience and hope after disaster strikes. A stranded marine biologist must rely on nature, memory, and courage to survive.\r\n\r\nA stirring blend of suspense and serenity, ideal for fans of introspective survival tales.");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "An enchanting journey through forest folklore and natural magic. A botanist stumbles upon a hidden realm where leaves whisper stories and trees guard ancient wisdom.\r\n\r\nBlending fantasy and serenity, this tale invites readers into a world of quiet wonder.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1,
                column: "Description",
                value: "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "Description",
                value: "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "Description",
                value: "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "Description",
                value: "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5,
                column: "Description",
                value: "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ");

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6,
                column: "Description",
                value: "Praesent vitae sodales libero. Praesent molestie orci augue, vitae euismod velit sollicitudin ac. Praesent vestibulum facilisis nibh ut ultricies.\r\n\r\nNunc malesuada viverra ipsum sit amet tincidunt. ");
        }
    }
}
