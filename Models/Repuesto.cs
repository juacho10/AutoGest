using System.ComponentModel.DataAnnotations;

namespace AutoGest.Models
{
    public class Repuesto : BaseEntity
    {
        [Required(ErrorMessage = "El código es obligatorio")]
        [StringLength(30)]
        public required string Codigo { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(200)]
        public required string Descripcion { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required(ErrorMessage = "El precio es obligatorio")]
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        [StringLength(100)]
        public string? Proveedor { get; set; }
    }
}