// En tu archivo C:\Users\anton\source\repos\EventNegotiation\Models\DatosUsuario.cs
namespace EventNegotiation.Models
{
    public class DatosUsuario
    {
        public int EventoId { get; set; }
        public string EventoNombre { get; set; }
        public DateTime Fecha { get; set; }
        public string Ubicacion { get; set; }
        public string EventoDescripcion { get; set; }
        public int? AcuerdoId { get; set; }
        public string AcuerdoDescripcion { get; set; }
        public DateTime? AcuerdoFecha { get; set; }
        public int? AgendaId { get; set; }
        public string AgendaTema { get; set; }
        public TimeSpan? AgendaTiempo { get; set; }
        public int? DocumentoId { get; set; }
        public string NombreArchivo { get; set; }
        public string Url { get; set; }
        public DateTime? DocumentoFecha { get; set; }
        public int? ParticipanteId { get; set; }
        public string ParticipanteRol { get; set; }
    }
}
