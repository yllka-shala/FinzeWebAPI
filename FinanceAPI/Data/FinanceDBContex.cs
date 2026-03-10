using FinanceAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceAPI.Data
{
    public class FinanceDBContex : DbContext
    {
        public FinanceDBContex(DbContextOptions<FinanceDBContex> options) : base(options)
        {}

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<PaymentMethod> PaymentMethods { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Frequency> Frequencies { get; set; }
        public DbSet<Recurring> Recurrings { get; set; }
        public DbSet<AccountType> AccountTypes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationType> NotificationTypes { get; set; }
        public DbSet<FinancialAccount> FinancialAccounts { get; set; }
        public DbSet<BillReminder> BillReminders { get; set; }
        public DbSet<Goal> Goals { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            List<PaymentMethod> pm = new List<PaymentMethod>
            {
                new PaymentMethod{ Id=1, Name = "Cash"},
                new PaymentMethod{ Id=2, Name = "Bank Transfer"},
                new PaymentMethod{ Id=3, Name = "Credit Card"},
                new PaymentMethod{ Id=4, Name = "PayPal"}
            };

            modelBuilder.Entity<PaymentMethod>().HasData(pm);

            List<Frequency> fr = new List<Frequency>
            {
                new Frequency{ Id=1, Name = "Weekly"},
                new Frequency{ Id=2, Name = "Monthly"},
                new Frequency{ Id=3, Name = "Yearly"}
            };

            modelBuilder.Entity<Frequency>().HasData(fr);

            List<AccountType> at = new List<AccountType>
            {
                new AccountType{ Id=1, Name = "Checking"},
                new AccountType{ Id=2, Name = "Savings"},
                new AccountType{ Id=3, Name = "Business"}
            };

            modelBuilder.Entity<AccountType>().HasData(at);

            List<NotificationType> nt = new List<NotificationType>
            {
                new NotificationType{ Id=1, Name = "Bill Due"},
                new NotificationType{ Id=2, Name = "Budget Exceeded"},
                new NotificationType{ Id=3, Name = "Subscription Notifications"},
                new NotificationType{ Id=4, Name = "Push Notifications"},
                new NotificationType{ Id=5, Name = "In-App Notifications"},
                new NotificationType{ Id=6, Name = "Bill Reminder"},
                new NotificationType{ Id=7, Name = "Goal Reached"}
            };

            modelBuilder.Entity<NotificationType>().HasData(nt);

            List<Role> ro = new List<Role>
            {
                new Role{ Id=1, Name = "Admin"},
                new Role{ Id=2, Name = "Customer"}
            };

            modelBuilder.Entity<Role>().HasData(ro);
        }
    }
}
