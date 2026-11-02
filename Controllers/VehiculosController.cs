using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoGest.Models;
using AutoGest.Repository;
using AutoGest.Services;

namespace AutoGest.Controllers
{
    [Authorize(Policy = "SoloStaff")]
    public class VehiculosController : Controller
    {
        private readonly IRepository<Vehiculo> _repository;
        private readonly IRepository<Cliente> _clienteRepo;
        private const int PageSize = 10;

        public VehiculosController(IRepository<Vehiculo> repository, IRepository<Cliente> clienteRepo)
        {
            _repository = repository;
            _clienteRepo = clienteRepo;
        }

        public async Task<IActionResult> Index(int pagina = 1, string search = "", int? clienteId = null)
        {
            Expression<Func<Vehiculo, bool>>? filtro = null;

            if (clienteId.HasValue)
            {
                Expression<Func<Vehiculo, bool>> f1 = v => v.ClienteId == clienteId.Value;
                filtro = filtro == null ? f1 : AutoGest.Helpers.PredicateBuilder.And(filtro, f1);

                var cli = await _clienteRepo.GetByIdAsync(clienteId.Value);
                if (cli != null) ViewBag.TituloEspecial = $"Vehículos de: {cli.NombreCompleto}";
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                Expression<Func<Vehiculo, bool>> f2 = v =>
                    v.Patente.ToLower().Contains(s) ||
                    v.Marca.ToLower().Contains(s) ||
                    v.Modelo.ToLower().Contains(s);
                filtro = filtro == null ? f2 : AutoGest.Helpers.PredicateBuilder.And(filtro, f2);
            }

            var (items, total) = await _repository.GetPagedAsync(
                pagina, PageSize, filtro,
                q => q.OrderBy(v => v.Patente),
                v => v.Cliente!);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.TotalRegistros = total;
            ViewBag.Search = search;
            ViewBag.ClienteId = clienteId;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarClientes(string term = "", int page = 1)
        {
            const int pageSize = 20;
            var (items, total) = await _clienteRepo.SearchAsync(
                term, page, pageSize,
                extraFilter: c => c.Activo,
                orderBy: q => q.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre),
                searchFields: new Expression<Func<Cliente, string>>[]
                {
                    c => c.Nombre, c => c.Apellido, c => c.Dni
                });

            return Json(new
            {
                results = items.Select(c => new { id = c.Id, text = $"{c.Apellido}, {c.Nombre} - DNI {c.Dni}" }),
                pagination = new { more = page * pageSize < total }
            });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var vehiculo = await _repository.GetByIdAsync(id.Value);
            if (vehiculo == null) return NotFound();
            if (vehiculo.ClienteId > 0)
                vehiculo.Cliente = await _clienteRepo.GetByIdAsync(vehiculo.ClienteId);
            return View(vehiculo);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Vehiculo vehiculo, IFormFile? imagen, [FromServices] CloudinaryService cloudinary)
        {
            var existePatente = await _repository.FindAsync(x => x.Patente == vehiculo.Patente);
            if (existePatente.Any())
                ModelState.AddModelError("Patente", "Ya existe un vehículo con esta patente");

            if (ModelState.IsValid)
            {
                if (imagen != null && imagen.Length > 0)
                    vehiculo.Imagen = await cloudinary.SubirImagenAsync(imagen, "autogest/vehiculos");

                await _repository.AddAsync(vehiculo);
                await _repository.SaveAsync();
                TempData["SuccessMessage"] = "Vehículo creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            if (vehiculo.ClienteId > 0)
                vehiculo.Cliente = await _clienteRepo.GetByIdAsync(vehiculo.ClienteId);

            return View(vehiculo);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var vehiculo = await _repository.GetByIdAsync(id.Value);
            if (vehiculo == null) return NotFound();
            if (vehiculo.ClienteId > 0)
                vehiculo.Cliente = await _clienteRepo.GetByIdAsync(vehiculo.ClienteId);
            return View(vehiculo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Vehiculo vehiculo, IFormFile? imagen, [FromServices] CloudinaryService cloudinary)
        {
            if (id != vehiculo.Id) return NotFound();

            var existePatente = await _repository.FindAsync(x => x.Patente == vehiculo.Patente && x.Id != vehiculo.Id);
            if (existePatente.Any())
                ModelState.AddModelError("Patente", "Ya existe un vehículo con esta patente");

            if (ModelState.IsValid)
            {
                if (imagen != null && imagen.Length > 0)
                    vehiculo.Imagen = await cloudinary.SubirImagenAsync(imagen, "autogest/vehiculos");

                vehiculo.FechaModificacion = DateTime.Now;
                _repository.Update(vehiculo);
                await _repository.SaveAsync();
                TempData["SuccessMessage"] = "Vehículo actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            if (vehiculo.ClienteId > 0)
                vehiculo.Cliente = await _clienteRepo.GetByIdAsync(vehiculo.ClienteId);

            return View(vehiculo);
        }

        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var vehiculo = await _repository.GetByIdAsync(id.Value);
            if (vehiculo == null) return NotFound();
            if (vehiculo.ClienteId > 0)
                vehiculo.Cliente = await _clienteRepo.GetByIdAsync(vehiculo.ClienteId);
            return View(vehiculo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var vehiculo = await _repository.GetByIdAsync(id);
            if (vehiculo != null)
            {
                _repository.Remove(vehiculo);
                await _repository.SaveAsync();
                TempData["SuccessMessage"] = "Vehículo eliminado exitosamente.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}