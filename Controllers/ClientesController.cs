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
    public class ClientesController : Controller
    {
        private readonly IRepository<Cliente> _repository;
        private const int PageSize = 10;

        public ClientesController(IRepository<Cliente> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult VueIndex() => View();

        public async Task<IActionResult> Index(int pagina = 1, string search = "")
        {
            Expression<Func<Cliente, bool>>? filtro = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                filtro = c => c.Nombre.ToLower().Contains(s)
                           || c.Apellido.ToLower().Contains(s)
                           || c.Dni.Contains(s)
                           || c.Email.ToLower().Contains(s);
            }

            var (items, total) = await _repository.GetPagedAsync(
                pagina, PageSize, filtro,
                q => q.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre));

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
                extraFilter: c => c.Activo,
                orderBy: q => q.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre),
                searchFields: new Expression<Func<Cliente, string>>[]
                {
                    c => c.Nombre, c => c.Apellido, c => c.Dni, c => c.Email
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
            var cliente = await _repository.GetByIdAsync(id.Value);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            var existeDni = await _repository.FindAsync(x => x.Dni == cliente.Dni);
            if (existeDni.Any()) ModelState.AddModelError("Dni", "Ya existe un cliente con este DNI");

            var existeEmail = await _repository.FindAsync(x => x.Email == cliente.Email);
            if (existeEmail.Any()) ModelState.AddModelError("Email", "Ya existe un cliente con este Email");

            if (ModelState.IsValid)
            {
                await _repository.AddAsync(cliente);
                await _repository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _repository.GetByIdAsync(id.Value);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Cliente cliente)
        {
            if (id != cliente.Id) return NotFound();

            var existeDni = await _repository.FindAsync(x => x.Dni == cliente.Dni && x.Id != cliente.Id);
            if (existeDni.Any()) ModelState.AddModelError("Dni", "Ya existe un cliente con este DNI");

            var existeEmail = await _repository.FindAsync(x => x.Email == cliente.Email && x.Id != cliente.Id);
            if (existeEmail.Any()) ModelState.AddModelError("Email", "Ya existe un cliente con este Email");

            if (ModelState.IsValid)
            {
                cliente.FechaModificacion = DateTime.Now;
                _repository.Update(cliente);
                await _repository.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _repository.GetByIdAsync(id.Value);
            if (cliente == null) return NotFound();
            return View(cliente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _repository.GetByIdAsync(id);
            if (cliente != null)
            {
                _repository.Remove(cliente);
                await _repository.SaveAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==================== ENDPOINTS AJAX PARA VUE ====================

        [HttpGet]
        public async Task<IActionResult> GetClientes(int page = 1, int pageSize = 10, string search = "")
        {
            Expression<Func<Cliente, bool>>? filtro = null;
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                filtro = c => c.Nombre.ToLower().Contains(s)
                           || c.Apellido.ToLower().Contains(s)
                           || c.Dni.Contains(s)
                           || c.Email.ToLower().Contains(s);
            }

            var (items, total) = await _repository.GetPagedAsync(
                page, pageSize, filtro,
                q => q.OrderBy(c => c.Apellido).ThenBy(c => c.Nombre));

            return Json(new
            {
                items = items.Select(c => new
                {
                    c.Id, c.Dni, c.Nombre, c.Apellido, c.Telefono, c.Email, c.Direccion, c.Activo
                }),
                total, page, pageSize,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateAjax([FromBody] Cliente cliente)
        {
            if (cliente == null) return BadRequest(new { message = "Datos inválidos" });

            var existeDni = await _repository.FindAsync(x => x.Dni == cliente.Dni);
            if (existeDni.Any()) return BadRequest(new { message = "Ya existe un cliente con este DNI" });

            var existeEmail = await _repository.FindAsync(x => x.Email == cliente.Email);
            if (existeEmail.Any()) return BadRequest(new { message = "Ya existe un cliente con este Email" });

            await _repository.AddAsync(cliente);
            await _repository.SaveAsync();
            return Json(new { success = true, id = cliente.Id, message = "Cliente creado exitosamente" });
        }

        [HttpPut]
        public async Task<IActionResult> EditAjax(int id, [FromBody] Cliente cliente)
        {
            if (cliente == null) return BadRequest(new { message = "Datos inválidos" });
            if (id != cliente.Id) return BadRequest(new { message = "ID no coincide" });

            var existeDni = await _repository.FindAsync(x => x.Dni == cliente.Dni && x.Id != cliente.Id);
            if (existeDni.Any()) return BadRequest(new { message = "Ya existe un cliente con este DNI" });

            var existeEmail = await _repository.FindAsync(x => x.Email == cliente.Email && x.Id != cliente.Id);
            if (existeEmail.Any()) return BadRequest(new { message = "Ya existe un cliente con este Email" });

            var entidad = await _repository.GetByIdAsync(id);
            if (entidad == null) return NotFound(new { message = "Cliente no encontrado" });

            entidad.Dni = cliente.Dni;
            entidad.Nombre = cliente.Nombre;
            entidad.Apellido = cliente.Apellido;
            entidad.Telefono = cliente.Telefono;
            entidad.Email = cliente.Email;
            entidad.Direccion = cliente.Direccion;
            entidad.Activo = cliente.Activo;
            entidad.FechaModificacion = DateTime.Now;

            _repository.Update(entidad);
            await _repository.SaveAsync();
            return Json(new { success = true, message = "Cliente actualizado exitosamente" });
        }

        [HttpDelete]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            var cliente = await _repository.GetByIdAsync(id);
            if (cliente == null) return NotFound(new { message = "Cliente no encontrado" });

            _repository.Remove(cliente);
            await _repository.SaveAsync();
            return Json(new { success = true, message = "Cliente eliminado exitosamente" });
        }
    }
}