using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeliveryTracker.DbModels
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class DeliveryTrackerDbContext: IdentityDbContext<UserModel, RoleModel, Guid>
    {

        #region dbsets

        public DbSet<InstanceModel> Instances { get; set; }

        public DbSet<InvitationModel> Invitations { get; set; }

        public DbSet<TaskStateModel> TaskStates { get; set; }

        public DbSet<TaskModel> Tasks { get; set; }
        
        public DbSet<DeviceModel> Devices { get; set; }

        #endregion

        public DeliveryTrackerDbContext(DbContextOptions<DeliveryTrackerDbContext> options)
            : base(options)
        {
            
            this.ChangeTracker.AutoDetectChangesEnabled = false;
        }

        #region configuring

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasPostgresExtension("uuid-ossp");
            modelBuilder.HasPostgresExtension("citext");

            ConfigureIdentity(modelBuilder);
            ConfigureInstanceModel(modelBuilder);
            ConfigureInvitationModel(modelBuilder);
            ConfigureTaskStateModel(modelBuilder);
            ConfigureTaskModel(modelBuilder);
            ConfigureDeviceModel(modelBuilder);
        }

        private static void ConfigureIdentity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>(b => 
            {
                b.Property(u => u.Id)
                    .HasDefaultValueSql("uuid_generate_v4()");

                // longitude и latitude инициализируются автоматически.
                
                b.Property(u => u.Surname)
                    .HasColumnType("citext collate \"ucs_basic\"")
                    .IsRequired();
                
                b.Property(u => u.Name)
                    .HasColumnType("citext collate \"ucs_basic\"")
                    .IsRequired();

                b.Property(u => u.Deleted)
                    .HasDefaultValue(false);
                
                b.HasOne(u => u.Instance)
                    .WithMany(g => g.Users)
                    .HasForeignKey(u => u.InstanceId)
                    .IsRequired();
                
                b.Property(p => p.LastTimePositionUpdated)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("now() at time zone 'utc'")
                    .IsRequired();

                b.HasIndex(p => p.InstanceId);
                b.HasIndex(p => p.LastTimePositionUpdated);
                b.HasIndex(p => new {p.Longitude, p.Latitude});
            });

            modelBuilder.Entity<RoleModel>(b =>
            {
                b.Property(u => u.Id)
                    .HasDefaultValueSql("uuid_generate_v4()");
            });
        }

        private static void ConfigureInvitationModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InvitationModel>(b =>
            {
                b.HasKey(p => p.Id);
                
                b.Property(p => p.InvitationCode)
                    .HasColumnType("varchar(16)");
                b.HasIndex(p => p.InvitationCode)
                    .IsUnique();
                b.Property(p => p.ExpirationDate)
                    .HasColumnType("timestamp")
                    .IsRequired();
                
                b.Property(u => u.Surname)
                    .HasColumnType("citext collate \"ucs_basic\"");
                
                b.Property(u => u.Name)
                    .HasColumnType("citext collate \"ucs_basic\"");

                b.Property(u => u.PhoneNumber)
                    .HasColumnName("varchar(20)");

                b.HasOne(p => p.Role)
                    .WithMany(r => r.Invitations)
                    .HasForeignKey(p => p.RoleId)
                    .IsRequired();
                
                b.HasOne(p => p.Instance)
                    .WithMany(r => r.Invitations)
                    .HasForeignKey(p => p.InstanceId)
                    .IsRequired();
                
                b.HasIndex(p => p.RoleId);
                b.HasIndex(p => p.InstanceId);
            });
        }

        private static void ConfigureTaskStateModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskStateModel>(b =>
            {
                b.HasKey(p => p.Id);

                b.Property(p => p.Alias)
                    .HasColumnType("varchar(255)");
                b.HasIndex(p => p.Alias)
                    .IsUnique();
            });
        }

        private static void ConfigureInstanceModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InstanceModel>(b =>
            {
                b.HasKey(p => p.Id);

                b.Property(p => p.InstanceName)
                    .HasColumnType("citext collate \"ucs_basic\"")
                    .IsRequired();

                b.HasOne(p => p.Creator)
                    .WithOne(p => p.CreatedInstance)
                    .HasForeignKey<InstanceModel>(p => p.CreatorId);
                
                b.HasIndex(p => p.CreatorId);
            });
        }

        private static void ConfigureTaskModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskModel>(b => 
            {
                b.HasKey(p => p.Id);

                b.Property(p => p.Number)
                    .HasColumnType("citext collate \"ucs_basic\"")
                    .IsRequired();
                
                b.Property(p => p.ShippingDesc)
                    .HasColumnType("citext collate \"ucs_basic\"");

                b.Property(p => p.Details)
                    .HasColumnType("citext collate \"ucs_basic\"");
                
                b.Property(p => p.Address)
                    .HasColumnType("citext collate \"ucs_basic\"");

                b.HasOne(t => t.State)
                    .WithMany(s => s.Tasks)
                    .HasForeignKey(t => t.StateId)
                    .IsRequired();

                b.HasOne(t => t.Instance)
                    .WithMany(u => u.Tasks)
                    .HasForeignKey(t => t.InstanceId)
                    .IsRequired();
                
                b.HasOne(t => t.Author)
                    .WithMany(u => u.SentTasks)
                    .HasForeignKey(t => t.AuthorId)
                    .IsRequired();

                b.HasOne(t => t.Performer)
                    .WithMany(u => u.PerformingTasks)
                    .HasForeignKey(t => t.PerformerId);
                
                
                
                b.Property(p => p.DatetimeFrom)
                    .HasColumnType("timestamp");
                
                b.Property(p => p.DatetimeTo)
                    .HasColumnType("timestamp");

                b.Property(p => p.CreationDate)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("now() at time zone 'utc'")
                    .IsRequired();
                
                b.Property(p => p.SetPerformerDate)
                    .HasColumnType("timestamp");
                
                b.Property(p => p.InWorkDate)
                    .HasColumnType("timestamp");
                
                b.Property(p => p.InWorkDate)
                    .HasColumnType("timestamp");

                b.Property(p => p.CompletionDate)
                    .HasColumnType("timestamp");
                
                b.HasIndex(p => p.StateId);
                b.HasIndex(p => p.AuthorId);
                b.HasIndex(p => p.PerformerId);
                b.HasIndex(p => p.InstanceId);
                b.HasIndex(p => p.CreationDate);
            });
        }
        
        private static void ConfigureDeviceModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeviceModel>(b =>
            {
                b.HasKey(p => p.Id);

                b.Property(p => p.FirebaseId)
                    .HasColumnType("varchar(200)")
                    .IsRequired();

                b.HasOne(p => p.User)
                    .WithOne(p => p.Device)
                    .HasForeignKey<DeviceModel>(p => p.UserId);
                
                b.HasIndex(p => p.UserId);
            });
        }

        #endregion
    }
}