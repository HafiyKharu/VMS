using Visitor.Company;
using Abp.IdentityServer4vNext;
using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Visitor.Authorization.Delegation;
using Visitor.Authorization.Roles;
using Visitor.Authorization.Users;
using Visitor.Blacklist;
using Visitor.Chat;
using Visitor.Editions;
using Visitor.Friendships;
using Visitor.MultiTenancy;
using Visitor.MultiTenancy.Accounting;
using Visitor.MultiTenancy.Payments;
using Visitor.Storage;
using Visitor.Appointment;
using Visitor.Appointment.ExpiredUrl;
using System.Linq.Dynamic.Core;
using Visitor.Tower;
using Visitor.Seed;
using Abp;
using Visitor.PurposeOfVisit;
using Visitor.Title;
using Visitor.Departments;
using Visitor.Level;
using Visitor.Appointment.Dashboard;

namespace Visitor.EntityFrameworkCore
{
    public class VisitorDbContext : AbpZeroDbContext<Tenant, Role, User, VisitorDbContext>, IAbpPersistedGrantDbContext
    {
        public virtual DbSet<Company.CompanyEnt> Companies { get; set; }

        /* Define an IDbSet for each entity of the application */

        public virtual DbSet<Appointment.AppointmentEnt> Appointments { get; set; }
        public virtual DbSet<Title.TitleEnt> VisitorTitles { get; set; }
        public virtual DbSet<Tower.TowerEnt> Towers { get; set; }
        public virtual DbSet<PurposeOfVisit.PurposeOfVisitEnt> PurposeOfVisitAppointments { get; set; }
        public virtual DbSet<Level.LevelEnt> LevelAppointments { get; set; }
        public virtual DbSet<Departments.Department> Departments { get; set; }
        public virtual DbSet<VMSDashboardEnt> VMSDashboards { get; set; }

        public virtual DbSet<BinaryObject> BinaryObjects { get; set; }

        public virtual DbSet<Friendship> Friendships { get; set; }

        public virtual DbSet<ChatMessage> ChatMessages { get; set; }

        public virtual DbSet<SubscribableEdition> SubscribableEditions { get; set; }

        public virtual DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }

        public virtual DbSet<Invoice> Invoices { get; set; }

        public virtual DbSet<PersistedGrantEntity> PersistedGrants { get; set; }

        public virtual DbSet<SubscriptionPaymentExtensionData> SubscriptionPaymentExtensionDatas { get; set; }

        public virtual DbSet<UserDelegation> UserDelegations { get; set; }

        public virtual DbSet<RecentPassword> RecentPasswords { get; set; }
        public virtual DbSet<BlacklistEnt> Blacklist { get; set; }
        public virtual DbSet<ExpiredUrlEnt> ExpiredUrls { get; set; }

        public VisitorDbContext(DbContextOptions<VisitorDbContext> options)
            : base(options)
        {   
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            RegularGuidGenerator generator = new RegularGuidGenerator();

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BinaryObject>(b =>
            {
                b.HasIndex(e => new { e.TenantId });
            });

            modelBuilder.Entity<ChatMessage>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId, e.ReadState });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.TargetUserId, e.ReadState });
                b.HasIndex(e => new { e.TargetTenantId, e.UserId, e.ReadState });
            });

            modelBuilder.Entity<Friendship>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.UserId });
                b.HasIndex(e => new { e.TenantId, e.FriendUserId });
                b.HasIndex(e => new { e.FriendTenantId, e.UserId });
                b.HasIndex(e => new { e.FriendTenantId, e.FriendUserId });
            });

            modelBuilder.Entity<Tenant>(b =>
            {
                b.HasIndex(e => new { e.SubscriptionEndDateUtc });
                b.HasIndex(e => new { e.CreationTime });
            });

            modelBuilder.Entity<SubscriptionPayment>(b =>
            {
                b.HasIndex(e => new { e.Status, e.CreationTime });
                b.HasIndex(e => new { PaymentId = e.ExternalPaymentId, e.Gateway });
            });

            modelBuilder.Entity<SubscriptionPaymentExtensionData>(b =>
            {
                b.HasQueryFilter(m => !m.IsDeleted)
                    .HasIndex(e => new { e.SubscriptionPaymentId, e.Key, e.IsDeleted })
                    .IsUnique()
                    .HasFilter("[IsDeleted] = 0");
            });

            modelBuilder.Entity<UserDelegation>(b =>
            {
                b.HasIndex(e => new { e.TenantId, e.SourceUserId });
                b.HasIndex(e => new { e.TenantId, e.TargetUserId });
            });

            modelBuilder.ConfigurePersistedGrantEntity();

            modelBuilder.Entity<TowerEnt>().HasData(new TowerEnt{ Id = generator.Create(), TowerBankRakyat = "Tower 1"}, new TowerEnt { Id = generator.Create(), TowerBankRakyat = "Tower 2"});
            modelBuilder.Entity<PurposeOfVisitEnt>().HasData(new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Interview" }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Installation" }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Event" }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Event" }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Discussion" }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Delivery" }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Admission"  }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Collect Cheque" }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Document Collection" }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Meeting" }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Training" }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Vendor" }, new PurposeOfVisitEnt { Id = generator.Create(), PurposeOfVisitApp = "Visit" });
            modelBuilder.Entity<TitleEnt>().HasData(new TitleEnt { Id = generator.Create(), VisitorTitle = "Mrs" }, new TitleEnt { Id = generator.Create(), VisitorTitle = "Mr" }, new TitleEnt { Id = generator.Create(), VisitorTitle = "Ms" }, new TitleEnt { Id = generator.Create(), VisitorTitle = "Sir" }, new TitleEnt { Id = generator.Create(), VisitorTitle = "Tan Sri" }, new TitleEnt { Id = generator.Create(), VisitorTitle = "Puan Sri" }, new TitleEnt { Id = generator.Create(), VisitorTitle = "Dato Sri" });
            modelBuilder.Entity<CompanyEnt>().HasData(new CompanyEnt{ Id = generator.Create(), CompanyName = "Bank Rakyat", CompanyEmail="bankrakyat@bankrakyat.com", CompanyAddress="No. 33, Jalan Rakyat, KL Sentral, 50740 Kuala Lumpur", OfficePhoneNumber = "0123456789" }, new CompanyEnt { Id = generator.Create(), CompanyName = "Meco Furniture Trading Co.", CompanyEmail = "mecofurniture@yahoo.com", CompanyAddress = "Lot 1327, Centre Point Commercial Centre, Jalan Melayu, 98007 Miri, Sarawak", OfficePhoneNumber = "085437705" }, new CompanyEnt { Id = generator.Create(), CompanyName = "Mieco", CompanyEmail = "elaine@mieco.com", CompanyAddress = "No. 1, Blok C, Jalan Indah 2/6, Taman Indah, Batu 11, Cheras, 43200, Selangor, Darul Ehsan", OfficePhoneNumber = "0390759991" });
            modelBuilder.Entity<Department>().HasData(new Department { Id = generator.Create(), DepartmentName = "Information Security" }, new Department { Id = generator.Create(), DepartmentName = "Information Systems" }, new Department { Id = generator.Create(), DepartmentName = "IT Administration" }, new Department { Id = generator.Create(), DepartmentName = "IT Solutions" }, new Department { Id = generator.Create(), DepartmentName = "Network" });

            string level;
            for (int i = 1; i < 39; i++)
            {
                level = i.ToString();
                modelBuilder.Entity<LevelEnt>().HasData(new LevelEnt { Id = generator.Create(), LevelBankRakyat = level });
            }
        }

        public void SeedData()
        {
            if (!Towers.Any())
            {
                Towers.AddRange(SeedDataGenerator.GetSeedData());
                SaveChanges();
            }   
        }
    }
}