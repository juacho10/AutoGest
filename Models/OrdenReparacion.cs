using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AutoGest.Models
{
    public class OrdenReparacion : BaseEntity
    {
        [Required(ErrorMessage = "El vehículo es obligatorio")]
        public int VehiculoId { get; set; }

        [ForeignKey("VehiculoId")]
        public Vehiculo? Vehiculo { get; set; }

        [Required(ErrorMessage = "El mecánico es obligatorio")]
        [Display(Name = "Mecánico Asignado")]
        public int MecanicoId { get; set; }

        [ForeignKey("MecanicoId")]
        public Usuario? Mecanico { get; set; }

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
        [Display(Name = "Fecha de Ingreso")]
        [DataType(DataType.Date)]
        public DateTime FechaIngreso { get; set; }

        [Display(Name = "Fecha de Egreso")]
        [DataType(DataType.Date)]
        public DateTime? FechaEgreso { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria")]
        [StringLength(500)]
        [Display(Name = "Descripción del Problema")]
        public required string Descripcion { get; set; }

        [Display(Name = "Estado")]
        [StringLength(20)]
        public string Estado { get; set; } = "Pendiente";

        [Display(Name = "Mano de Obra")]
        [DataType(DataType.Currency)]
        public decimal ManoDeObra { get; set; }

        [Display(Name = "Monto Total")]
        [DataType(DataType.Currency)]
        public decimal MontoTotal { get; set; }

        // Auditoría
        public int? UsuarioCreacionId { get; set; }
        public int? UsuarioModificacionId { get; set; }

        [ForeignKey("UsuarioCreacionId")]
        public Usuario? UsuarioCreacion { get; set; }

        [ForeignKey("UsuarioModificacionId")]
        public Usuario? UsuarioModificacion { get; set; }

        public virtual ICollection<DetalleReparacion> Detalles { get; set; } = new List<DetalleReparacion>();
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

        [NotMapped]
        public decimal TotalPagos => Pagos.Where(p => !p.Anulado).Sum(p => p.Importe);

        [NotMapped]
        public decimal SaldoPendiente => MontoTotal - TotalPagos;
    }
}