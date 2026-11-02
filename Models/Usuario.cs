using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace AutoGest.Models
{
    public class Usuario : BaseEntity
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(50)]
        public string Nombre { get; set; } = "";

        [Required(ErrorMessage = "El apellido es obligatorio")]
        [StringLength(50)]
        public string Apellido { get; set; } = "";

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; } = "";

        public string Avatar { get; set; } = "";

        [Required]
        public string Rol { get; set; } = "Empleado";

        public string NombreCompleto => $"{Apellido}, {Nombre}";

        public virtual ICollection<OrdenReparacion> OrdenesAsignadas { get; set; } = new List<OrdenReparacion>();
        public virtual ICollection<OrdenReparacion> OrdenesCreadas { get; set; } = new List<OrdenReparacion>();
        public virtual ICollection<OrdenReparacion> OrdenesModificadas { get; set; } = new List<OrdenReparacion>();
        public virtual ICollection<Pago> PagosCreados { get; set; } = new List<Pago>();
        public virtual ICollection<Pago> PagosAnulados { get; set; } = new List<Pago>();
    }
}