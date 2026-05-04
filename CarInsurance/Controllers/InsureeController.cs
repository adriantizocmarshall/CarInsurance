using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CarInsurance.Data;
using CarInsurance.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Client;

namespace CarInsurance.Controllers
{
    public class InsureeController : Controller
    {
        private readonly CarInsuranceContext _context;

        private int CalculateQuote(Insuree insuree)
        {
            int basePrice = 50;

            // Calculate age based on date of birth as year only
            int age = DateTime.Now.Year - insuree.DateOfBirth;
                if (age <= 18)
                {
                    basePrice += 100;
                }
                else if (age >= 19 && age <= 25)
                {
                    basePrice += 50;
                }
                else if (age >= 26)
                {
                    basePrice += 25;
                }

                // Adjustments based on car year
                if (insuree.CarYear < 2000)
                {
                    basePrice += 25;
                }
                else if (insuree.CarYear > 2015)
                {
                    basePrice += 25;
                }

                // Adjustments based on car make
                if (insuree.CarMake != null && insuree.CarMake.ToLower() == "porsche")
                {
                    basePrice += 25;

                    // Additional charge for Porsche models
                    if (insuree.CarModel != null && insuree.CarModel.ToLower() == "911 carrera")
                    {
                        basePrice += 25;
                    }
                }

                // Price adjustment based on number of speeding tickets
                if (insuree.SpeedingTickets > 0)
                {
                    basePrice += (insuree.SpeedingTickets * 10);
                }

                // Change to coverage price if insuree has DUI
                if (insuree.DUI)
                {
                    basePrice = (int)(basePrice * 1.25); // Adjusted by adding 25%
                }

                // Adjustment for coverage type
                if (insuree.CoverageType != null && insuree.CoverageType.ToLower() == "full")
                {
                    basePrice = (int)(basePrice * 1.5); // Adds 50% to base coverage price
                }

                return basePrice;
        }

        public InsureeController(CarInsuranceContext context)
        {
            _context = context;
        }

        // GET: Insurees/Admin
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var adminViewModel = await _context.Insuree
                .Select(i => new AdminViewModel
                {
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    EmailAddress = i.EmailAddress,
                    Quote  = i.Quote
                })
                .ToListAsync();

            return View(adminViewModel);
        }

        // GET: Insurees
        public async Task<IActionResult> Index()
        {
            return View(await _context.Insuree.ToListAsync());
        }

        // GET: Insurees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuree = await _context.Insuree
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insuree == null)
            {
                return NotFound();
            }

            return View(insuree);
        }

        // GET: Insurees/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Insurees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,EmailAddress,DateOfBirth,CarYear,CarMake,CarModel,DUI,SpeedingTickets,CoverageType,Quote")] Insuree insuree)
        {
            if (ModelState.IsValid)
            {
                // Calculate estimated cost of insurance based on insuree info
                insuree.Quote = CalculateQuote(insuree);

                _context.Add(insuree);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(insuree);
        }


        // GET: Insurees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuree = await _context.Insuree.FindAsync(id);
            if (insuree == null)
            {
                return NotFound();
            }
            return View(insuree);
        }

        // POST: Insurees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,EmailAddress,DateOfBirth,CarYear,CarMake,CarModel,DUI,SpeedingTickets,CoverageType,Quote")] Insuree insuree)
        {
            if (id != insuree.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(insuree);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InsureeExists(insuree.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(insuree);
        }

        // GET: Insurees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuree = await _context.Insuree
                .FirstOrDefaultAsync(m => m.Id == id);
            if (insuree == null)
            {
                return NotFound();
            }

            return View(insuree);
        }

        // POST: Insurees/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var insuree = await _context.Insuree.FindAsync(id);
            if (insuree != null)
            {
                _context.Insuree.Remove(insuree);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InsureeExists(int id)
        {
            return _context.Insuree.Any(e => e.Id == id);
        }
    }
}
