using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using AutoGest.Models;
using AutoGest.Repository;

namespace AutoGest.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class VehiculosApiController : ControllerBase
    {
        private readonly IRepository<Vehiculo> _repository;

        public VehiculosApiController(IRepository<Vehiculo> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll(int page = 1, int pageSize = 10)
        {
            var (items, total) = await _repository.GetPagedAsync(
                page, pageSize, null,
                q => q.OrderBy(v => v.Patente));

            return Ok(new
            {
                items = items.Select(v => new
                {
                    v.Id, v.Patente, v.Marca, v.Modelo, v.Anio, v.Kilometraje, v.Color
                }),
                total, page,
                totalPages = (int)Math.Ceiling(total / (double)pageSize)
            });
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var vehiculo = await _repository.GetByIdAsync(id);
            if (vehiculo == null) return NotFound(new { message = "Vehículo no encontrado" });
            return Ok(vehiculo);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create([FromBody] Vehiculo vehiculo)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            await _repository.AddAsync(vehiculo);
            await _repository.SaveAsync();
            return CreatedAtAction(nameof(GetById), new { id = vehiculo.Id }, vehiculo);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, [FromBody] Vehiculo vehiculo)
        {
            if (id != vehiculo.Id) return BadRequest(new { message = "ID no coincide" });
            if (!ModelState.IsValid) return BadRequest(ModelState);
            vehiculo.FechaModificacion = DateTime.Now;
            _repository.Update(vehiculo);
            await _repository.SaveAsync();
            return Ok(new { message = "Actualizado", vehiculo });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var vehiculo = await _repository.GetByIdAsync(id);
            if (vehiculo == null) return NotFound();
            _repository.Remove(vehiculo);
            await _repository.SaveAsync();
            return Ok(new { message = "Eliminado" });
        }
    }
}