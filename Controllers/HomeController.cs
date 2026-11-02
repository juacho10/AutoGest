using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AutoGest.Models;
using AutoGest.Repository;
using Microsoft.AspNetCore.Authorization;

namespace AutoGest.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IRepository<Cliente> _clienteRepo;
    private readonly IRepository<Vehiculo> _vehiculoRepo;
    private readonly IRepository<OrdenReparacion> _ordenRepo;
    private readonly IRepository<Pago> _pagoRepo;

    public HomeController(
        ILogger<HomeController> logger,
        IRepository<Cliente> clienteRepo,
        IRepository<Vehiculo> vehiculoRepo,
        IRepository<OrdenReparacion> ordenRepo,
        IRepository<Pago> pagoRepo)
    {
        _logger = logger;
        _clienteRepo = clienteRepo;
        _vehiculoRepo = vehiculoRepo;
        _ordenRepo = ordenRepo;
        _pagoRepo = pagoRepo;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalClientes = (await _clienteRepo.GetAllAsync()).Count();
        ViewBag.TotalVehiculos = (await _vehiculoRepo.GetAllAsync()).Count();
        ViewBag.TotalOrdenes = (await _ordenRepo.FindAsync(o => o.Estado != "Entregado")).Count();
        ViewBag.TotalPagos = (await _pagoRepo.GetAllAsync()).Count();
        return View();
    }

    [AllowAnonymous]
    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [AllowAnonymous]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}