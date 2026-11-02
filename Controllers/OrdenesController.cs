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
    public class OrdenesController : Controller
    {
        private readonly IRepository<OrdenReparacion> _repository;
        private readonly IRepository<Vehiculo> _vehiculoRepo;
        private readonly IRepository<Usuario> _usuarioRepo;
        private readonly IRepository<Repuesto> _repuestoRepo;
        private readonly IRepository<DetalleReparacion> _detalleRepo;
        private const int PageSize = 10;

        public OrdenesController(
            IRepository<OrdenReparacion> repository,
            IRepository<Vehiculo> vehiculoRepo,
            IRepository<Usuario> usuarioRepo,
            IRepository<Repuesto> repuestoRepo,
            IRepository<DetalleReparacion> detalleRepo)
        {
            _repository = repository;
            _vehiculoRepo = vehiculoRepo;
            _usuarioRepo = usuarioRepo;
            _repuestoRepo = repuestoRepo;
            _detalleRepo = detalleRepo;
        }

        public async Task<IActionResult> Index(int pagina = 1, string search = "", string? estado = null)
        {
            Expression<Func<OrdenReparacion, bool>>? filtro = null;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                Expression<Func<OrdenReparacion, bool>> f = o =>
                    o.Vehiculo!.Patente.ToLower().Contains(s) ||
                    o.Vehiculo!.Marca.ToLower().Contains(s) ||
                    o.Descripcion.ToLower().Contains(s);
                filtro = f;
            }

            if (!string.IsNullOrWhiteSpace(estado))
            {
                Expression<Func<OrdenReparacion, bool>> f2 = o => o.Estado == estado;
                filtro = filtro == null ? f2 : Helpers.PredicateBuilder.And(filtro, f2);
            }

            var (items, total) = await _repository.GetPagedAsync(
                pagina, PageSize, filtro,
                q => q.OrderByDescending(o => o.FechaIngreso),
                o => o.Vehiculo!,
                o => o.Mecanico!);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.TotalRegistros = total;
            ViewBag.Search = search;
            ViewBag.Estado = estado;

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> BuscarVehiculos(string term = "", int page = 1)
        {
            const int pageSize = 20;
            var (items, total) = await _vehiculoRepo.SearchAsync(
                term, page, pageSize,
                extraFilter: v => v.Activo,
                orderBy: q => q.OrderBy(v => v.Patente),
                searchFields: new Expression<Func<Vehiculo, string>>[]
                {
                    v => v.Patente, v => v.Marca, v => v.Modelo
                });

            return Json(new
            {
                results = items.Select(v => new { id = v.Id, text = $"{v.Patente} - {v.Marca} {v.Modelo}" }),
                pagination = new { more = page * pageSize < total }
            });
        }

        [HttpGet]
        public async Task<IActionResult> BuscarMecanicos(string term = "", int page = 1)
        {
            const int pageSize = 20;
            var (items, total) = await _usuarioRepo.SearchAsync(
                term, page, pageSize,
                extraFilter: u => u.Activo,
                orderBy: q => q.OrderBy(u => u.Apellido).ThenBy(u => u.Nombre),
                searchFields: new Expression<Func<Usuario, string>>[]
                {
                    u => u.Nombre, u => u.Apellido, u => u.Email
                });

            return Json(new
            {
                results = items.Select(u => new { id = u.Id, text = $"{u.Apellido}, {u.Nombre} ({u.Rol})" }),
                pagination = new { more = page * pageSize < total }
            });
        }

        [HttpGet]
        public async Task<IActionResult> BuscarRepuestos(string term = "", int page = 1)
        {
            const int pageSize = 20;
            var (items, total) = await _repuestoRepo.SearchAsync(
                term, page, pageSize,
                orderBy: q => q.OrderBy(r => r.Descripcion),
                searchFields: new Expression<Func<Repuesto, string>>[]
                {
                    r => r.Codigo, r => r.Descripcion
                });

            return Json(new
            {
                results = items.Select(r => new
                {
                    id = r.Id,
                    text = $"{r.Codigo} - {r.Descripcion} - ${r.Precio:0.00} (Stock: {r.Stock})",
                    precio = r.Precio
                }),
                pagination = new { more = page * pageSize < total }
            });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var orden = await _repository.GetByIdAsync(id.Value);
            if (orden == null) return NotFound();

            if (orden.VehiculoId > 0)
                orden.Vehiculo = await _vehiculoRepo.GetByIdAsync(orden.VehiculoId);
            if (orden.MecanicoId > 0)
                orden.Mecanico = await _usuarioRepo.GetByIdAsync(orden.MecanicoId);

            var detalles = await _detalleRepo.FindAsync(d => d.OrdenId == orden.Id);
            foreach (var d in detalles)
                d.Repuesto = await _repuestoRepo.GetByIdAsync(d.RepuestoId);
            orden.Detalles = detalles.ToList();

            return View(orden);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrdenReparacion orden, int[]? repuestoIds, int[]? cantidades)
        {
            if (ModelState.IsValid)
            {
                var usuarioEmail = User?.Identity?.Name;
                if (!string.IsNullOrEmpty(usuarioEmail))
                {
                    var usuarios = await _usuarioRepo.FindAsync(u => u.Email == usuarioEmail);
                    var usuario = usuarios.FirstOrDefault();
                    if (usuario != null) orden.UsuarioCreacionId = usuario.Id;
                }

                // Calcular monto total
                decimal montoTotal = orden.ManoDeObra;

                await _repository.AddAsync(orden);
                await _repository.SaveAsync();

                if (repuestoIds != null && cantidades != null && repuestoIds.Length == cantidades.Length)
                {
                    for (int i = 0; i < repuestoIds.Length; i++)
                    {
                        if (repuestoIds[i] <= 0 || cantidades[i] <= 0) continue;

                        var rep = await _repuestoRepo.GetByIdAsync(repuestoIds[i]);
                        if (rep == null) continue;

                        var detalle = new DetalleReparacion
                        {
                            OrdenId = orden.Id,
                            RepuestoId = rep.Id,
                            Cantidad = cantidades[i],
                            PrecioUnitario = rep.Precio
                        };
                        await _detalleRepo.AddAsync(detalle);
                        montoTotal += detalle.Subtotal;

                        rep.Stock -= cantidades[i];
                        _repuestoRepo.Update(rep);
                    }
                    await _detalleRepo.SaveAsync();
                    await _repuestoRepo.SaveAsync();
                }

                orden.MontoTotal = montoTotal;
                _repository.Update(orden);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Orden creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            if (orden.VehiculoId > 0)
                orden.Vehiculo = await _vehiculoRepo.GetByIdAsync(orden.VehiculoId);
            if (orden.MecanicoId > 0)
                orden.Mecanico = await _usuarioRepo.GetByIdAsync(orden.MecanicoId);

            return View(orden);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var orden = await _repository.GetByIdAsync(id.Value);
            if (orden == null) return NotFound();

            if (orden.VehiculoId > 0)
                orden.Vehiculo = await _vehiculoRepo.GetByIdAsync(orden.VehiculoId);
            if (orden.MecanicoId > 0)
                orden.Mecanico = await _usuarioRepo.GetByIdAsync(orden.MecanicoId);

            return View(orden);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OrdenReparacion orden)
        {
            if (id != orden.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var usuarioEmail = User?.Identity?.Name;
                var usuario = (usuarioEmail != null)
                    ? (await _usuarioRepo.FindAsync(u => u.Email == usuarioEmail)).FirstOrDefault()
                    : null;
                if (usuario != null) orden.UsuarioModificacionId = usuario.Id;

                orden.FechaModificacion = DateTime.Now;
                _repository.Update(orden);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Orden actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            if (orden.VehiculoId > 0)
                orden.Vehiculo = await _vehiculoRepo.GetByIdAsync(orden.VehiculoId);
            if (orden.MecanicoId > 0)
                orden.Mecanico = await _usuarioRepo.GetByIdAsync(orden.MecanicoId);

            return View(orden);
        }

        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var orden = await _repository.GetByIdAsync(id.Value);
            if (orden == null) return NotFound();
            if (orden.VehiculoId > 0)
                orden.Vehiculo = await _vehiculoRepo.GetByIdAsync(orden.VehiculoId);
            if (orden.MecanicoId > 0)
                orden.Mecanico = await _usuarioRepo.GetByIdAsync(orden.MecanicoId);
            return View(orden);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var orden = await _repository.GetByIdAsync(id);
            if (orden != null)
            {
                _repository.Remove(orden);
                await _repository.SaveAsync();
                TempData["SuccessMessage"] = "Orden eliminada.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}