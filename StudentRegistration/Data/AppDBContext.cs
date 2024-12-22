using Microsoft.EntityFrameworkCore;
using StudentRegistration.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StudentRegidtration.Models;

namespace StudentRegistration.Data
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }

        public DbSet<Teacher> Teachers{ get; set; } 
        public DbSet<Faculty> Faculty{ get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<AttendanceDetail> AttendanceDetails { get; set; }   

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Faculty>()
                .HasMany(pr => pr.Students)
                .WithOne(s => s.Faculty)
                .HasForeignKey(s => s.FacultyId);

            modelBuilder.Entity<Faculty>()
                .HasMany(pr => pr.Teachers)
                .WithOne(t => t.Faculty)
                .HasForeignKey(t => t.FacultyId);


            modelBuilder.Entity<AttendanceDetail>()
                .HasOne(a => a.Student)
                .WithMany(s => s.AttendanceDetails)
                .HasForeignKey(s => s.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Attendance>()
                .HasMany(a => a.AttendanceDetails)
                .WithOne(ad => ad.Attendance)
                .HasForeignKey(a => a.AttendanceId);
              

            /*modelBuilder.Entity<Faculty>()
                .HasMany(f => f.Attendances)
                .WithOne(a => a.Faculty)
                .HasForeignKey(a => a.FacultyId);*/

            modelBuilder.Entity<User>()
                .HasOne(u => u.Student)
                .WithOne(s => s.User)
                .HasForeignKey<User>(s => s.StudentId);



            modelBuilder.Entity<User>()
               .HasOne(u => u.Teacher)
               .WithOne(t => t.User)
               .HasForeignKey<User>(s => s.TeacherId);
           


        }

    }
}
