using System;
using System.Diagnostics.CodeAnalysis;
using DeliveryTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DeliveryTracker.Db
{
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public sealed class DeliveryTrackerDbContext: IdentityDbContext<UserModel, RoleModel, Guid>
    {

        #region dbsets

        public DbSet<GroupModel> Groups { get; set; }

        public DbSet<InvitationModel> Invitations { get; set; }

        public DbSet<TaskStateModel> TaskStates { get; set; }

        public DbSet<TaskModel> Tasks { get; set; }

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
            ConfigureGroupModel(modelBuilder);
            ConfigureInvitationModel(modelBuilder);
            ConfigureTaskStateModel(modelBuilder);
            ConfigureTaskModel(modelBuilder);
        }

        private static void ConfigureIdentity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserModel>(b => 
            {
                b.Property(u => u.Id)
                    .HasDefaultValueSql("uuid_generate_v4()");

                b.Property(u => u.DisplayableName)
                    .HasColumnType("citext collate \"ucs_basic\"")
                    .IsRequired();
                
                b.HasOne(u => u.Group)
                    .WithMany(g => g.Users)
                    .HasForeignKey(u => u.GroupId)
                    .IsRequired();
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

                b.HasOne(p => p.Role)
                    .WithMany(r => r.Invitations)
                    .HasForeignKey(p => p.RoleId)
                    .IsRequired();
                
                b.HasOne(p => p.Group)
                    .WithMany(r => r.Invitations)
                    .HasForeignKey(p => p.GroupId)
                    .IsRequired();
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

                b.Property(p => p.Caption)
                    .HasColumnType("citext collate \"ucs_basic\"")
                    .IsRequired();
            });
        }

        private static void ConfigureGroupModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GroupModel>(b =>
            {
                b.HasKey(p => p.Id);

                b.Property(p => p.DisplayableName)
                    .HasColumnType("citext collate \"ucs_basic\"")
                    .IsRequired();

                b.HasOne(p => p.Creator)
                    .WithOne(p => p.CreatedGroup)
                    .HasForeignKey<GroupModel>(p => p.CreatorId);
            });
        }

        private static void ConfigureTaskModel(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TaskModel>(b => 
            {
                b.HasKey(p => p.Id);

                b.Property(p => p.Caption)
                    .HasColumnType("citext collate \"ucs_basic\"")
                    .IsRequired();

                b.Property(p => p.Content)
                    .HasColumnType("citext collate \"ucs_basic\"")
                    .IsRequired();

                b.HasOne(t => t.State)
                    .WithMany(s => s.Tasks)
                    .HasForeignKey(t => t.StateId)
                    .IsRequired();

                b.HasOne(t => t.Sender)
                    .WithMany(u => u.SentTasks)
                    .HasForeignKey(t => t.SenderId)
                    .IsRequired();

                b.HasOne(t => t.Performer)
                    .WithMany(u => u.PerformingTasks)
                    .HasForeignKey(t => t.PerformerId);
                
                b.Property(p => p.CreationDate)
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("now() at time zone 'utc'")
                    .IsRequired();
                
                b.Property(p => p.Deadline)
                    .HasColumnType("timestamp");

                b.Property(p => p.InWorkDate)
                    .HasColumnType("timestamp");

                b.Property(p => p.CompletionDate)
                    .HasColumnType("timestamp");
            });
        }

        #endregion
    }
}