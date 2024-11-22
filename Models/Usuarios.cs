using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EventNegotiation.Models
{
    [Table("usuarios")]  // Asegura que se mapee a la tabla "usuarios"
    public class Usuarios
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nombre { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Phone]
        public string Telefono { get; set; }

        public int ErrorAutentificacion { get; set; }
    }
}
