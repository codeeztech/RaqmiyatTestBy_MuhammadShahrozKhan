using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace FileProcessing.WebAPI.Data
{
    public class FileProcessingContext : DbContext
    {
        public FileProcessingContext(DbContextOptions<FileProcessingContext> options)
            : base(options)
        { }

        public DbSet<User> Users { get; set; }
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

       
        public string FirstName { get; set; }

        public string LastName { get; set; }

        
        public string Email { get; set; }

        
        public int Rate { get; set; }
    }
}
