using Npgsql;
using EventNegotiation.Models;
using System.Data;
using System.Data.Common;
using Dapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using EventNegotiation.Data;


namespace EventNegotiation.Data.Servicios
{
    public class GeneralServicio
    {
        private readonly Contexto _contexto;

        public GeneralServicio(Contexto contexto)
        {
            _contexto = contexto;
        }

        public int AgregarUsuario(string nombre, string email, string password, string telefono)
        {

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            using (var conn = new NpgsqlConnection(_contexto.Conexion))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("SELECT agregar_usuarios(@nombre, @email, @password, @telefono)", conn))
                {
                    // Añadir parámetros a la función
                    cmd.Parameters.AddWithValue("nombre", nombre);
                    cmd.Parameters.AddWithValue("email", email);
                    cmd.Parameters.AddWithValue("password", hashedPassword);
                    cmd.Parameters.AddWithValue("telefono", telefono);

                    // Ejecutar la función y obtener el ID del nuevo usuario
                    var usuarioId = (int)cmd.ExecuteScalar();
                    return usuarioId;
                }
            }
        }

        //verifucar email
        public bool EmailExists(string email)
        {
            using (var connection = new NpgsqlConnection(_contexto.Conexion))
            {
                connection.Open();
                using (var cmd = new NpgsqlCommand("SELECT COUNT(1) FROM usuario WHERE email = @p_email", connection))
                {
                    cmd.Parameters.AddWithValue("p_email", email);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }


        public int AgregarEmpresa(string nombreEmpresa, string direccionEmpresa, string telefonoEmpresa)
        {
            using (var conn = new NpgsqlConnection(_contexto.Conexion))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("INSERT INTO public.empresas (nombre, direccion, telefono) VALUES (@nombre, @direccion, @telefono) RETURNING empresa_id;", conn))
                {
                    cmd.Parameters.AddWithValue("nombre", nombreEmpresa);
                    cmd.Parameters.AddWithValue("direccion", direccionEmpresa);
                    cmd.Parameters.AddWithValue("telefono", telefonoEmpresa);

                    // Obtener el ID de la empresa recién creada
                    int empresaId = (int)cmd.ExecuteScalar();
                    return empresaId; // Retorna el ID de la empresa
                }
            }
        }


        public void AsociarEmpresaAUsuario(int usuarioId, int empresaId)
        {
            using (var conn = new NpgsqlConnection(_contexto.Conexion))
            {
                conn.Open();

                // Llamamos a la función SQL desde C#
                using (var cmd = new NpgsqlCommand("SELECT asociar_empresa_a_usuario(@usuario_id, @empresa_id)", conn))
                {
                    cmd.Parameters.AddWithValue("usuario_id", usuarioId);
                    cmd.Parameters.AddWithValue("empresa_id", empresaId);

                    // Ejecutamos la función
                    cmd.ExecuteNonQuery();
                }
            }
        }


        public List<Evento> ObtenerEventosPorUsuario(int usuarioId)
        {
            var eventos = new List<Evento>();

            using (var connection = new NpgsqlConnection(_contexto.Conexion))
            {
                connection.Open();

                // Usamos la función SQL como una consulta directa
                using (var command = new NpgsqlCommand("SELECT * FROM public.obtener_datos_usuario(@usuario_id)", connection))
                {
                    command.Parameters.AddWithValue("@usuario_id", usuarioId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var evento = new Evento
                            {
                                EventoId = reader.GetInt32(reader.GetOrdinal("evento_id")),
                                EventoNombre = reader.GetString(reader.GetOrdinal("evento_nombre")),
                                Fecha = reader.GetDateTime(reader.GetOrdinal("fecha")),
                                Ubicacion = reader.GetString(reader.GetOrdinal("ubicacion")),
                                EventoDescripcion = reader.GetString(reader.GetOrdinal("evento_descripcion")),
                                AcuerdoId = reader.GetInt32(reader.GetOrdinal("acuerdo_id")),
                                AcuerdoDescripcion = reader.GetString(reader.GetOrdinal("acuerdo_descripcion")),
                                AcuerdoFecha = reader.GetDateTime(reader.GetOrdinal("acuerdo_fecha")),
                                AgendaId = reader.GetInt32(reader.GetOrdinal("agenda_id")),
                                AgendaTema = reader.GetString(reader.GetOrdinal("agenda_tema")),
                                AgendaTiempo = reader.GetTimeSpan(reader.GetOrdinal("agenda_tiempo")),
                                DocumentoId = reader.GetInt32(reader.GetOrdinal("documento_id")),
                                NombreArchivo = reader.GetString(reader.GetOrdinal("nombre_archivo")),
                                Url = reader.GetString(reader.GetOrdinal("url")),
                                DocumentoFecha = reader.GetDateTime(reader.GetOrdinal("documento_fecha")),
                                ParticipanteId = reader.GetInt32(reader.GetOrdinal("participante_id")),
                                ParticipanteRol = reader.GetString(reader.GetOrdinal("participante_rol"))
                            };

                            eventos.Add(evento);
                        }
                    }
                }
            }

            return eventos;
        }











    }
}
