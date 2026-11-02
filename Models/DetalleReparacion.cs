using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoGest.Models
{
    public class DetalleReparacion : BaseEntity
    {
        [Required]
        public int OrdenId { get; set; }

        [ForeignKey("OrdenId")]
        public OrdenReparacion? Orden { get; set; }

        [Required]
        public int RepuestoId { get; set; }

        [ForeignKey("RepuestoId")]
        public Repuesto? Repuesto { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal PrecioUnitario { get; set; }

        [NotMapped]
        public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}