using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AutoGest.Models
{
    public class Vehiculo : BaseEntity
    {
        [Required(ErrorMessage = "La patente es obligatoria")]
        [StringLength(10)]
        [Display(Name = "Patente")]
        public required string Patente { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        [StringLength(50)]
        public required string Marca { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio")]
        [StringLength(50)]
        public required string Modelo { get; set; }

        [Required(ErrorMessage = "El año es obligatorio")]
        [Range(1900, 2100)]
        [Display(Name = "Año")]
        public int Anio { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Kilometraje { get; set; }

        [StringLength(30)]
        public string? Color { get; set; }

        [Display(Name = "Imagen")]
        public string? Imagen { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }

        public virtual ICollection<OrdenReparacion> Ordenes { get; set; } = new List<OrdenReparacion>();

        public string Descripcion => $"{Marca} {Modelo} ({Anio}) - {Patente}";
    }
}