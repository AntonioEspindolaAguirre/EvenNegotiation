namespace EventNegotiation.Models
{
    public class EventoResponse
    {
        public int EventoId { get; set; }
        public string EventoNombre { get; set; }
        public DateTime Fecha { get; set; }
        public string Ubicacion { get; set; }
        public string EventoDescripcion { get; set; }

        // Detalles adicionales como acuerdos, agendas, documentos y participantes
        public List<Acuerdo> Acuerdos { get; set; }
        public List<Agenda> Agendas { get; set; }
        public List<Documento> Documentos { get; set; }
        public List<Participante> Participantes { get; set; }
    }
}
