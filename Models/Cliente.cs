using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AutoGest.Models
{
    public class Cliente : BaseEntity
    {
        [Required(ErrorMessage = "El DNI es obligatorio")]
        [StringLength(10, ErrorMessage = "El DNI no puede tener más de 10 caracteres")]
        public required string Dni { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50)]
        public required string Nombre { get; set; }

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50)]
        public required string Apellido { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        public required string Telefono { get; set; }

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public required string Email { get; set; }

        [StringLength(200)]
        public string? Direccion { get; set; }

        public string NombreCompleto => $"{Apellido}, {Nombre}";

        public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
    }
}