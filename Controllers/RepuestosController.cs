using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoGest.Models;
using AutoGest.Repository;

namespace AutoGest.Controllers
{
    [Authorize(Policy = "SoloStaff")]
    public class RepuestosController : Controller
    {
        private readonly IRepository<Repuesto> _repository;
        private const int PageSize = 10;

        public RepuestosController(IRepository<Repuesto> repository)
        {
            _repository = repository;
        }

        public async Task<IActionResult> Index(int pagina = 1, string search = "")
        {
            Expression<Func<Repuesto, bool>>? filtro = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                filtro = r => r.Codigo.ToLower().Contains(s)
                           || r.Descripcion.ToLower().Contains(s)
                           || (r.Proveedor != null && r.Proveedor.ToLower().Contains(s));
            }

            var (items, total) = await _repository.GetPagedAsync(
                pagina, PageSize, filtro,
                q => q.OrderBy(r => r.Descripcion));

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.TotalRegistros = total;
            ViewBag.Search = search;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Buscar(string term = "", int page = 1)
        {
            const int pageSize = 20;
            var (items, total) = await _repository.SearchAsync(
                term, page, pageSize,
                orderBy: q => q.OrderBy(r => r.Descripcion),
                searchFields: new Expression<Func<Repuesto, string>>[]
                {
                    r => r.Codigo, r => r.Descripcion
                });

            return Json(new
            {
                results = items.Select(r => new { id = r.Id, text = $"{r.Codigo} - {r.Descripcion} (${r.Precio:0.00}) - Stock: {r.Stock}" }),
                pagination = new { more = page * pageSize < total }
            });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var repuesto = await _repository.GetByIdAsync(id.Value);
            if (repuesto == null) return NotFound();
            return View(repuesto);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Repuesto repuesto)
        {
            var existeCodigo = await _repository.FindAsync(x => x.Codigo == repuesto.Codigo);
            if (existeCodigo.Any()) ModelState.AddModelError("Codigo", "Ya existe un repuesto con este código");

            if (ModelState.IsValid)
            {
                await _repository.AddAsync(repuesto);
                await _repository.SaveAsync();
                TempData["SuccessMessage"] = "Repuesto creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(repuesto);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var repuesto = await _repository.GetByIdAsync(id.Value);
            if (repuesto == null) return NotFound();
            return View(repuesto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Repuesto repuesto)
        {
            if (id != repuesto.Id) return NotFound();

            var existeCodigo = await _repository.FindAsync(x => x.Codigo == repuesto.Codigo && x.Id != repuesto.Id);
            if (existeCodigo.Any()) ModelState.AddModelError("Codigo", "Ya existe un repuesto con este código");

            if (ModelState.IsValid)
            {
                repuesto.FechaModificacion = DateTime.Now;
                _repository.Update(repuesto);
                await _repository.SaveAsync();
                TempData["SuccessMessage"] = "Repuesto actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(repuesto);
        }

        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var repuesto = await _repository.GetByIdAsync(id.Value);
            if (repuesto == null) return NotFound();
            return View(repuesto);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var repuesto = await _repository.GetByIdAsync(id);
            if (repuesto != null)
            {
                _repository.Remove(repuesto);
                await _repository.SaveAsync();
                TempData["SuccessMessage"] = "Repuesto eliminado exitosamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}