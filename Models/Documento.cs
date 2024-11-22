namespace EventNegotiation.Models
{
    public class Documento
    {
        public int DocumentoId { get; set; }
        public string NombreArchivo { get; set; }
        public string Url { get; set; }
        public DateTime FechaSubida { get; set; }
    }
}
