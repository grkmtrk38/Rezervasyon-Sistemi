using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RezervasyonSistemi.Models;

namespace RezervasyonSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class KaynaksController : Controller
    {
        private readonly AppDbContext _context;

        public KaynaksController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Kaynaks
        public async Task<IActionResult> Index()
        {
            return View(await _context.Kaynaklar.ToListAsync());
        }

        // GET: Kaynaks/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kaynak = await _context.Kaynaklar
                .FirstOrDefaultAsync(m => m.Id == id);
            if (kaynak == null)
            {
                return NotFound();
            }

            return View(kaynak);
        }

        // GET: Kaynaks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Kaynaks/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ad,Tip,Kapasite,Aktif")] Kaynak kaynak)
        {
            if (ModelState.IsValid)
            {
                _context.Add(kaynak);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(kaynak);
        }

        // GET: Kaynaks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kaynak = await _context.Kaynaklar.FindAsync(id);
            if (kaynak == null)
            {
                return NotFound();
            }
            return View(kaynak);
        }

        // POST: Kaynaks/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Tip,Kapasite,Aktif")] Kaynak kaynak)
        {
            if (id != kaynak.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(kaynak);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!KaynakExists(kaynak.Id))
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
            return View(kaynak);
        }

        // GET: Kaynaks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var kaynak = await _context.Kaynaklar
                .FirstOrDefaultAsync(m => m.Id == id);
            if (kaynak == null)
            {
                return NotFound();
            }

            return View(kaynak);
        }

        // POST: Kaynaks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var kaynak = await _context.Kaynaklar.FindAsync(id);
            if (kaynak != null)
            {
                _context.Kaynaklar.Remove(kaynak);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool KaynakExists(int id)
        {
            return _context.Kaynaklar.Any(e => e.Id == id);
        }
    }
}
