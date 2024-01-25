using Microsoft.AspNetCore.Mvc;
using MvcFotoBlogEf.Data;
using MvcFotoBlogEf.Models;

namespace MvcFotoBlogEf.Controllers
{
    public class GonderilerController : Controller
    {
        private readonly UygulamaDbContext _db;
        private readonly IWebHostEnvironment _env;

        public GonderilerController(UygulamaDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }
        public IActionResult Yeni()
        {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Yeni(YeniGonderiVewModel vm)
        {
            if (ModelState.IsValid)
            {
                string ext = Path.GetExtension(vm.Resim.FileName);
                string yeniDosyaAdi = Guid.NewGuid() + ext;
                string yol = Path.Combine(_env.WebRootPath, "img", "upload", yeniDosyaAdi);
                using (var fs = new FileStream(yol, FileMode.CreateNew))
                {
                    vm.Resim.CopyTo(fs);
                }

                _db.Gonderiler.Add(new Gonderi
                {
                    Baslik = vm.Baslik,
                    Resim = yeniDosyaAdi
                });
                _db.SaveChanges();
                return RedirectToAction("Index", "Home", new { Sonuc = "Eklendi" });
            }

            return View(vm);
        }

        public IActionResult Guncelle(int id)
        {
            Gonderi duzenlenecek = _db.Gonderiler.Find(id);
            if (duzenlenecek == null)
                return NotFound();

            DuzenleGonderiViewModel vm = new DuzenleGonderiViewModel()
            {
                Id = duzenlenecek.Id,
                Baslik = duzenlenecek.Baslik,
                ResimYolu = duzenlenecek.Resim,
            };
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Guncelle(DuzenleGonderiViewModel vm)
        {
            if (ModelState.IsValid)
            {
                Gonderi duzenlenecek = _db.Gonderiler.Find(vm.Id);

                if (duzenlenecek == null) return NotFound();

                if (vm.Resim != null)
                {
                    System.IO.File.Delete(Path.Combine(_env.WebRootPath, "img", "upload", duzenlenecek.Resim));
                    string ext = Path.GetExtension(vm.Resim.FileName);
                    string yeniDosyaAd = Guid.NewGuid() + ext;
                    string yol = Path.Combine(_env.WebRootPath, "img", "upload", yeniDosyaAd);
                    using (var fs = new FileStream(yol, FileMode.CreateNew))
                    {
                        vm.Resim.CopyTo(fs);
                    }
                    duzenlenecek.Resim = yeniDosyaAd;
                }
                duzenlenecek.Baslik = vm.Baslik;
                _db.Gonderiler.Update(duzenlenecek);
                _db.SaveChanges();
                return RedirectToAction("Index", "Home", new { Sonuc = "Duzenlendi" });
            }
            return View(vm);
        }

        public IActionResult Sil(int id)
        {
            Gonderi silinecek = _db.Gonderiler.Find(id);

            if (silinecek == null) return NotFound();

            return View(silinecek);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Sil(Gonderi silinecek)
        {
            if (ModelState.IsValid)
            {
                _db.Gonderiler.Remove(silinecek);
                _db.SaveChanges();
                System.IO.File.Delete(Path.Combine(_env.WebRootPath, "img", "upload", silinecek.Resim));
                return RedirectToAction("Index", "Home");
            }

            return View(silinecek);
        }
    }
}
