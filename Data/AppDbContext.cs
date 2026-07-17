    using Core.API.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

    namespace Core.API.Data;

    public class AppDbContext
        : IdentityDbContext<AppUser, AppRole, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            
        }
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceProduct> InvoicesProduct => Set<InvoiceProduct>();
        public DbSet<Packages> Packages => Set<Packages>();

    //************************************************************************
        public DbSet<Invoice2> Invoices2 => Set<Invoice2>();
        public DbSet<InvoiceProduct2> InvoicesProduct2=> Set<InvoiceProduct2>();
        public DbSet<Packages2> Packages2 => Set<Packages2>();
        
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        
            builder.Entity<Invoice>()
                .HasIndex(o => o.CreatedAt)
                .HasMethod("brin")
                .HasDatabaseName("IX_CreatedAt_BRIN");    


            builder.Entity<InvoiceProduct>()
                .HasIndex(o => o.CreatedAt)
                .HasMethod("brin")
                .HasDatabaseName("IX_CreatedAt_BRIN_product"); 

            builder.Entity<InvoiceProduct>()
                .HasIndex(o => new { o.DKPC, o.PayMethod })
                .HasMethod("btree")
                .HasDatabaseName("IX_btree_dkpc_paymethod");   



            builder.Entity<Invoice>()
                .HasIndex(x => x.DigikalaId)
                .IsUnique();

            builder.Entity<InvoiceProduct>()
                .HasKey(x => x.DigikalaId);

            builder.Entity<Invoice>()
                .HasMany(x => x.Products)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Packages>()
                .HasIndex(o => o.RecievedAt)
                .HasMethod("brin")
                .HasDatabaseName("IX_CreatedAt_BRIN_Packages");  

    //****************************************************************************

            builder.Entity<Invoice2>()
                .HasIndex(o => o.CreatedAt)
                .HasMethod("brin")
                .HasDatabaseName("IX2_CreatedAt_BRIN");    


            builder.Entity<InvoiceProduct2>()
                .HasIndex(o => o.CreatedAt)
                .HasMethod("brin")
                .HasDatabaseName("IX2_CreatedAt_BRIN_product"); 

            builder.Entity<InvoiceProduct2>()
                .HasIndex(o => new { o.DKPC, o.PayMethod })
                .HasMethod("btree")
                .HasDatabaseName("IX2_btree_dkpc_paymethod");   



            builder.Entity<Invoice2>()
                .HasIndex(x => x.DigikalaId)
                .IsUnique();

            builder.Entity<InvoiceProduct2>()
                .HasKey(x => x.DigikalaId);

            builder.Entity<Invoice2>()
                .HasMany(x => x.Products)
                .WithOne(x => x.Invoice)
                .HasForeignKey(x => x.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Packages2>()
                .HasIndex(o => o.RecievedAt)
                .HasMethod("brin")
                .HasDatabaseName("IX2_CreatedAt_BRIN_Packages");  

        }
    }