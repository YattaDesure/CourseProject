using GreenQuarter.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GreenQuarter.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Apartment> Apartments { get; set; }
    public DbSet<ParkingSpace> ParkingSpaces { get; set; }
    public DbSet<StorageRoom> StorageRooms { get; set; }
    public DbSet<UserApartment> UserApartments { get; set; }
    public DbSet<UserParkingSpace> UserParkingSpaces { get; set; }
    public DbSet<UserStorageRoom> UserStorageRooms { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure relationships
        builder.Entity<UserApartment>()
            .HasKey(ua => ua.Id);

        builder.Entity<UserApartment>()
            .HasOne(ua => ua.User)
            .WithMany(u => u.UserApartments)
            .HasForeignKey(ua => ua.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserApartment>()
            .HasOne(ua => ua.Apartment)
            .WithMany(a => a.UserApartments)
            .HasForeignKey(ua => ua.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserParkingSpace>()
            .HasKey(ups => ups.Id);

        builder.Entity<UserParkingSpace>()
            .HasOne(ups => ups.User)
            .WithMany(u => u.UserParkingSpaces)
            .HasForeignKey(ups => ups.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserParkingSpace>()
            .HasOne(ups => ups.ParkingSpace)
            .WithMany(ps => ps.UserParkingSpaces)
            .HasForeignKey(ups => ups.ParkingSpaceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserStorageRoom>()
            .HasKey(usr => usr.Id);

        builder.Entity<UserStorageRoom>()
            .HasOne(usr => usr.User)
            .WithMany(u => u.UserStorageRooms)
            .HasForeignKey(usr => usr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<UserStorageRoom>()
            .HasOne(usr => usr.StorageRoom)
            .WithMany(sr => sr.UserStorageRooms)
            .HasForeignKey(usr => usr.StorageRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.Entity<Apartment>()
            .HasIndex(a => a.Number);

        builder.Entity<ParkingSpace>()
            .HasIndex(ps => ps.SlotNumber);
    }
}

