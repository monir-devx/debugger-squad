using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Ecommerce.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder builder) //Overriding given function
        {
            base.OnModelCreating(builder);

            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Action", DisplayOrder = 1 },
                new Category { Id = 2, Name = "SciFi", DisplayOrder = 2 },
                new Category { Id = 3, Name = "History", DisplayOrder = 3 }
            );

            builder.Entity<Company>().HasData(
               new Company
               {
                   Id = 1,
                   Name = "Tech Solution",
                   StreetAddress = "123 Tech St",
                   City = "Tech City",
                   PostalCode = "12121",
                   State = "IL",
                   PhoneNumber = "6669990000"
               },
                new Company
                {
                    Id = 2,
                    Name = "Vivid Books",
                    StreetAddress = "999 Vid St",
                    City = "Vid City",
                    PostalCode = "66666",
                    State = "IL",
                    PhoneNumber = "7779990000"
                },
                new Company
                {
                    Id = 3,
                    Name = "Readers Club",
                    StreetAddress = "999 Main St",
                    City = "Lala land",
                    PostalCode = "99999",
                    State = "NY",
                    PhoneNumber = "1113335555"
                }
            );

            builder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Title = "Fortune of Time",
                    Author = "Billy Spark",
                    Description = "A gripping journey through time, where destiny and science intertwine. When a forgotten relic is uncovered, one man must solve ancient riddles to protect the fabric of reality.\r\n\r\nRich in suspense and imagination, this time-travel adventure blends mystery with powerful storytelling.",
                    ISBN = "SWD9999001",
                    ListPrice = 99,
                    Price = 90,
                    Price50 = 85,
                    Price100 = 80,
                    CategoryId = 1
                },
                new Product
                {
                    Id = 2,
                    Title = "Dark Skies",
                    Author = "Nancy Hoover",
                    Description = "A haunting tale of survival beneath an ever-darkening sky. As the world grapples with a mysterious eclipse, one woman uncovers a secret that could save—or destroy—humanity.\r\n\r\nWith atmospheric tension and emotional depth, this thriller keeps readers on edge.",
                    ISBN = "CAW777777701",
                    ListPrice = 40,
                    Price = 30,
                    Price50 = 25,
                    Price100 = 20,
                    CategoryId = 1
                },
                new Product
                {
                    Id = 3,
                    Title = "Vanish in the Sunset",
                    Author = "Julian Button",
                    Description = "An emotional story of love and letting go, set against the backdrop of a coastal town. As past wounds resurface, a photographer and a writer find healing in unexpected places.\r\n\r\nA bittersweet romance that captures the fleeting beauty of second chances.",
                    ISBN = "RITO5555501",
                    ListPrice = 55,
                    Price = 50,
                    Price50 = 40,
                    Price100 = 35,
                    CategoryId = 1
                },
                new Product
                {
                    Id = 4,
                    Title = "Cotton Candy",
                    Author = "Abby Muscles",
                    Description = "A delightful tale that explores childhood nostalgia and the sweetness of dreams. Follow a group of friends as they navigate small-town joys, heartbreaks, and coming-of-age truths.\r\n\r\nPlayful and heartwarming, this story blends humor with gentle life lessons.",
                    ISBN = "WS3333333301",
                    ListPrice = 70,
                    Price = 65,
                    Price50 = 60,
                    Price100 = 55,
                    CategoryId = 2
                },
                new Product
                {
                    Id = 5,
                    Title = "Rock in the Ocean",
                    Author = "Ron Parker",
                    Description = "Set on a remote island, this novel tells the story of resilience and hope after disaster strikes. A stranded marine biologist must rely on nature, memory, and courage to survive.\r\n\r\nA stirring blend of suspense and serenity, ideal for fans of introspective survival tales.",
                    ISBN = "SOTJ1111111101",
                    ListPrice = 30,
                    Price = 27,
                    Price50 = 25,
                    Price100 = 20,
                    CategoryId = 2
                },
                new Product
                {
                    Id = 6,
                    Title = "Leaves and Wonders",
                    Author = "Laura Phantom",
                    Description = "An enchanting journey through forest folklore and natural magic. A botanist stumbles upon a hidden realm where leaves whisper stories and trees guard ancient wisdom.\r\n\r\nBlending fantasy and serenity, this tale invites readers into a world of quiet wonder.",
                    ISBN = "FOT000000001",
                    ListPrice = 25,
                    Price = 23,
                    Price50 = 22,
                    Price100 = 20,
                    CategoryId = 3
                }
            );
        }
    }
}
