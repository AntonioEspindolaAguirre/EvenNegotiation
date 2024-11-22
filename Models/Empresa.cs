using System;

namespace EventNegotiation.Models
{
    public class Empresa
    {
        public int EmpresaId { get; set; } // ID de la empresa (clave primaria)
        public string Nombre { get; set; } // Nombre de la empresa
        public string Direccion { get; set; } // Dirección de la empresa
        public string Telefono { get; set; } // Teléfono de la empresa
        public DateTime? FechaCreacion { get; set; } // Fecha de creación (se genera automáticamente)

       
    }
}
