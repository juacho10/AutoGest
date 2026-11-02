using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoGest.Models
{
    public class Pago : BaseEntity
    {
        [Required]
        [Display(Name = "Número de Pago")]
        public int NumeroPago { get; set; }

        [Required]
        [Display(Name = "Fecha de Pago")]
        [DataType(DataType.Date)]
        public DateTime FechaPago { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [DataType(DataType.Currency)]
        public decimal Importe { get; set; }

        [Required]
        [StringLength(150)]
        public required string Concepto { get; set; }

        [Display(Name = "Anulado")]
        public bool Anulado { get; set; } = false;

        [Required]
        public int OrdenId { get; set; }

        [ForeignKey("OrdenId")]
        public OrdenReparacion? Orden { get; set; }

        public int UsuarioCreacionId { get; set; }
        public int? UsuarioAnulacionId { get; set; }

        [ForeignKey("UsuarioCreacionId")]
        public Usuario? UsuarioCreacion { get; set; }

        [ForeignKey("UsuarioAnulacionId")]
        public Usuario? UsuarioAnulacion { get; set; }
    }
}