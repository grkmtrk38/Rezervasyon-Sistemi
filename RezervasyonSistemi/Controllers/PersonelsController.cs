using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RezervasyonSistemi.Models;
using RezervasyonSistemi.Models.ViewModels;

namespace RezervasyonSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PersonelsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PersonelsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Personels
        public async Task<IActionResult> Index()
        {
            return View(await _context.Personeller.ToListAsync());
        }

        // GET: Personels/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personel = await _context.Personeller
                .FirstOrDefaultAsync(m => m.Id == id);
            if (personel == null)
            {
                return NotFound();
            }

            return View(personel);
        }

        // GET: Personels/Create
        public IActionResult Create()
        {
            return View(new PersonelCreateViewModel());
        }

        // POST: Personels/Create (Tek Adımda Personel + ApplicationUser Oluşturma)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PersonelCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                // 1. Önce Personel kaydı veritabanına eklenir
                var personel = new Personel
                {
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    Birim = model.Birim ?? "Genel"
                };

                _context.Personeller.Add(personel);
                await _context.SaveChangesAsync();

                // 2. Identity Kullanıcı hesabı oluşturulur ve PersonelId bağlanır
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    Ad = model.Ad,
                    Soyad = model.Soyad,
                    Birim = model.Birim,
                    PersonelId = personel.Id,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (createResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "User");
                    return RedirectToAction(nameof(Index));
                }

                // 3. Kullanıcı hesabı açılamazsa (şifre zayıf vb.) oluşturulan personel kaydı geri alınır (Rollback)
                _context.Personeller.Remove(personel);
                await _context.SaveChangesAsync();

                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // GET: Personels/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personel = await _context.Personeller.FindAsync(id);
            if (personel == null)
            {
                return NotFound();
            }
            return View(personel);
        }

        // POST: Personels/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ad,Soyad,Birim")] Personel personel)
        {
            if (id != personel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(personel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonelExists(personel.Id))
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
            return View(personel);
        }

        // GET: Personels/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var personel = await _context.Personeller
                .FirstOrDefaultAsync(m => m.Id == id);
            if (personel == null)
            {
                return NotFound();
            }

            return View(personel);
        }

        // POST: Personels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var personel = await _context.Personeller.FindAsync(id);
            if (personel != null)
            {
                // İlişkili ApplicationUser varsa onu da siliyoruz
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PersonelId == id);
                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _context.Personeller.Remove(personel);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool PersonelExists(int id)
        {
            return _context.Personeller.Any(e => e.Id == id);
        }
    }
}
