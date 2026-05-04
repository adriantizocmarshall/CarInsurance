using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CarInsurance.Models;

namespace CarInsurance.Data
{
    public class CarInsuranceContext : DbContext
    {
        public CarInsuranceContext (DbContextOptions<CarInsuranceContext> options)
            : base(options)
        {
        }

        public DbSet<CarInsurance.Models.Insuree> Insuree { get; set; } = default!;
    }
}
