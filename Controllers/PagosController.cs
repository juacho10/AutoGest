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
    public class PagosController : Controller
    {
        private readonly IRepository<Pago> _repository;
        private readonly IRepository<OrdenReparacion> _ordenRepo;
        private readonly IRepository<Usuario> _usuarioRepo;
        private const int PageSize = 10;

        public PagosController(
            IRepository<Pago> repository,
            IRepository<OrdenReparacion> ordenRepo,
            IRepository<Usuario> usuarioRepo)
        {
            _repository = repository;
            _ordenRepo = ordenRepo;
            _usuarioRepo = usuarioRepo;
        }

        public async Task<IActionResult> Index(int pagina = 1, int? ordenId = null, string search = "", bool? anulado = null)
        {
            Expression<Func<Pago, bool>>? filtro = null;

            if (ordenId.HasValue)
            {
                Expression<Func<Pago, bool>> f = p => p.OrdenId == ordenId.Value;
                filtro = filtro == null ? f : Helpers.PredicateBuilder.And(filtro, f);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                Expression<Func<Pago, bool>> f = p => p.Concepto.ToLower().Contains(s);
                filtro = filtro == null ? f : Helpers.PredicateBuilder.And(filtro, f);
            }

            if (anulado.HasValue)
            {
                Expression<Func<Pago, bool>> f = p => p.Anulado == anulado.Value;
                filtro = filtro == null ? f : Helpers.PredicateBuilder.And(filtro, f);
            }

            var (items, total) = await _repository.GetPagedAsync(
                pagina, PageSize, filtro,
                q => q.OrderByDescending(p => p.FechaPago),
                p => p.Orden!,
                p => p.UsuarioCreacion!);

            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.TotalRegistros = total;
            ViewBag.Search = search;
            ViewBag.Anulado = anulado;
            ViewBag.OrdenId = ordenId;

            return View(items);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var pago = await _repository.GetByIdAsync(id.Value);
            if (pago == null) return NotFound();
            if (pago.OrdenId > 0) pago.Orden = await _ordenRepo.GetByIdAsync(pago.OrdenId);
            if (pago.UsuarioCreacionId > 0) pago.UsuarioCreacion = await _usuarioRepo.GetByIdAsync(pago.UsuarioCreacionId);
            if (pago.UsuarioAnulacionId.HasValue) pago.UsuarioAnulacion = await _usuarioRepo.GetByIdAsync(pago.UsuarioAnulacionId.Value);
            return View(pago);
        }

        public async Task<IActionResult> Create(int? ordenId = null)
{
    if (ordenId.HasValue)
    {
        var orden = await _ordenRepo.GetByIdAsync(ordenId.Value);
        if (orden == null) return NotFound();

        var nuevoPago = new Pago
        {
            OrdenId = orden.Id,
            FechaPago = DateTime.Today,
            Importe = orden.SaldoPendiente > 0 ? orden.SaldoPendiente : orden.MontoTotal,
            Concepto = $"Pago de orden #{orden.Id}",
            Orden = orden
        };
        return View(nuevoPago);
    }
    return View(new Pago { FechaPago = DateTime.Today, Concepto = "", Importe = 0 });
}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Pago pago)
        {
            var orden = await _ordenRepo.GetByIdAsync(pago.OrdenId);
            if (orden == null)
                ModelState.AddModelError("OrdenId", "La orden seleccionada no existe.");

            if (ModelState.IsValid)
            {
                var pagosExistentes = await _repository.FindAsync(p => p.OrdenId == pago.OrdenId);
                pago.NumeroPago = pagosExistentes.Any()
                    ? pagosExistentes.Max(p => p.NumeroPago) + 1
                    : 1;

                var usuarioEmail = User?.Identity?.Name;
                if (!string.IsNullOrEmpty(usuarioEmail))
                {
                    var usuario = (await _usuarioRepo.FindAsync(u => u.Email == usuarioEmail)).FirstOrDefault();
                    if (usuario != null) pago.UsuarioCreacionId = usuario.Id;
                }

                await _repository.AddAsync(pago);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Pago registrado exitosamente.";
                return RedirectToAction(nameof(Index), new { ordenId = pago.OrdenId });
            }

            pago.Orden = await _ordenRepo.GetByIdAsync(pago.OrdenId);
            return View(pago);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var pago = await _repository.GetByIdAsync(id.Value);
            if (pago == null) return NotFound();
            if (pago.OrdenId > 0) pago.Orden = await _ordenRepo.GetByIdAsync(pago.OrdenId);
            return View(pago);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string concepto)
        {
            var pago = await _repository.GetByIdAsync(id);
            if (pago == null) return NotFound();

            if (string.IsNullOrEmpty(concepto))
            {
                ModelState.AddModelError("Concepto", "El concepto es obligatorio.");
            }
            else
            {
                pago.Concepto = concepto;
                pago.FechaModificacion = DateTime.Now;
                _repository.Update(pago);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Concepto actualizado correctamente.";
                return RedirectToAction(nameof(Index), new { ordenId = pago.OrdenId });
            }

            pago.Orden = await _ordenRepo.GetByIdAsync(pago.OrdenId);
            return View(pago);
        }

        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> Anular(int? id)
        {
            if (id == null) return NotFound();
            var pago = await _repository.GetByIdAsync(id.Value);
            if (pago == null) return NotFound();
            if (pago.OrdenId > 0) pago.Orden = await _ordenRepo.GetByIdAsync(pago.OrdenId);
            return View(pago);
        }

        [HttpPost, ActionName("Anular")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = "Administrador")]
        public async Task<IActionResult> AnularConfirmed(int id)
        {
            var pago = await _repository.GetByIdAsync(id);
            if (pago != null)
            {
                pago.Anulado = true;
                pago.FechaModificacion = DateTime.Now;

                var usuarioEmail = User?.Identity?.Name;
                if (!string.IsNullOrEmpty(usuarioEmail))
                {
                    var usuario = (await _usuarioRepo.FindAsync(u => u.Email == usuarioEmail)).FirstOrDefault();
                    if (usuario != null) pago.UsuarioAnulacionId = usuario.Id;
                }

                _repository.Update(pago);
                await _repository.SaveAsync();

                TempData["SuccessMessage"] = "Pago anulado correctamente.";
            }
            return RedirectToAction(nameof(Index), new { ordenId = pago?.OrdenId });
        }
    }
}