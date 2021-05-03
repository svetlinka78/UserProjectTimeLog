
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace UserProject
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<TimeLog> TimeLogs { get; set; }
    }

    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<TimeLog> TimeLogs { get; set; }

    }

    public class TimeLog
    {
        public int UserId { get; set; }
        public virtual User User { get; set; }
        public int ProjectId { get; set; } 
        public virtual Project Project { get; set; }
        public DateTime DH { get; set; }
    }

    public class TimeLogModel
    {
        public int Id { get; set; }
        public string DH { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
      
    }

    public class UserModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ProjectModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UPTLModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserSurName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public DateTime DH { get; set; }

    }

    public class UPTLPage
    {
        public int RowCountTotal { get; set; }
        
    }

    public class UserProjectTimeLogContext : DbContext
    {
        public UserProjectTimeLogContext(DbContextOptions<UserProjectTimeLogContext> options)
            : base(options)
        { }
   
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TimeLog>()
            .HasKey(bc => new { bc.UserId, bc.ProjectId });
            modelBuilder.Entity<TimeLog>()
                .HasOne(bc => bc.User)
                .WithMany(b => b.TimeLogs)
                .HasForeignKey(bc => bc.UserId);
            modelBuilder.Entity<TimeLog>()
                .HasOne(bc => bc.Project)
                .WithMany(c => c.TimeLogs)
                .HasForeignKey(bc => bc.ProjectId);
        }

        public  DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TimeLog> TimeLogs { get; set; }

    }
}

    

