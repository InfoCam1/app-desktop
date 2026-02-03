using InfoCam.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InfoCam.Services
{
    public class ApiService
    {
        private const string BaseUrl = "http://10.10.16.85:8080/api";
        // private const string BaseUrl = "http://db.toadstudios.net:8080/api";
        private readonly HttpClient _client;

        public ApiService()
        {
            _client = new HttpClient();
        }

        public async Task<List<Camera>> GetCamerasAsync()
        {
            var stream = await _client.GetStreamAsync($"{BaseUrl}/camaras");
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Camera>));
            return (List<Camera>)serializer.ReadObject(stream);
        }

        public async Task<List<Camera>> GetActiveCamerasAsync()
        {
            var stream = await _client.GetStreamAsync($"{BaseUrl}/camaras/activas");
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Camera>));
            return (List<Camera>)serializer.ReadObject(stream);
        }

        public async Task<List<Incidencia>> GetIncidenciasAsync()
        {
            var stream = await _client.GetStreamAsync($"{BaseUrl}/incidencias");
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Incidencia>));
            return (List<Incidencia>)serializer.ReadObject(stream);
        }

        public async Task<List<string>> GetTiposIncidenciasAsync()
        {
            var stream = await _client.GetStreamAsync($"{BaseUrl}/incidencias/tipos");
            var serializer = new DataContractJsonSerializer(typeof(List<string>));
            return (List<string>)serializer.ReadObject(stream);
        }

        public async Task<List<Incidencia>> GetActiveIncidenciasAsync(string date)
        {
            var stream = await _client.GetStreamAsync($"{BaseUrl}/incidencias/activas?fecha={date}");
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Incidencia>));
            return (List<Incidencia>)serializer.ReadObject(stream);
        }

        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            var stream = await _client.GetStreamAsync($"{BaseUrl}/usuarios");
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Usuario>));
            return (List<Usuario>)serializer.ReadObject(stream);
        }

        public async Task<bool> CreateIncidenciaAsync(Incidencia incidencia)
        {
            var serializer = new DataContractJsonSerializer(typeof(Incidencia));
            using (var stream = new System.IO.MemoryStream())
            {
                serializer.WriteObject(stream, incidencia);
                string json = Encoding.UTF8.GetString(stream.ToArray());
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _client.PostAsync($"{BaseUrl}/incidencias", content);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> UpdateIncidenciaAsync(Incidencia incidencia)
        {
            var serializer = new DataContractJsonSerializer(typeof(Incidencia));
            using (var stream = new System.IO.MemoryStream())
            {
                serializer.WriteObject(stream, incidencia);
                string json = Encoding.UTF8.GetString(stream.ToArray());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PutAsync($"{BaseUrl}/incidencias/{incidencia.Id}", content);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> DeleteIncidenciaAsync(long id)
        {
            var response = await _client.DeleteAsync($"{BaseUrl}/incidencias/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteCameraAsync(int id)
        {
            var response = await _client.DeleteAsync($"{BaseUrl}/camaras/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<Usuario> LoginAsync(string username, string password)
        {
            // 1. Hashear la contraseña con SHA-256
            string hashedPassword = "";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                hashedPassword = builder.ToString();
            }

            // 2. Construir el JSON con la contraseña ya hasheada
            // Usamos string interpolation con cuidado o un objeto anónimo si usaras System.Text.Json
            string json = $"{{\"username\":\"{username}\",\"password\":\"{hashedPassword}\"}}";
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _client.PostAsync($"{BaseUrl}/auth/login", content);

                if (!response.IsSuccessStatusCode)
                    return null;

                string responseBody = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseBody) || responseBody == "{}")
                    return null;

                // 3. Deserializar la respuesta
                using (var stream = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(responseBody)))
                {
                    DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Usuario));
                    Usuario user = (Usuario)serializer.ReadObject(stream);
                    return user;
                }
            }
            catch (HttpRequestException)
            {
                throw new Exception("Could not connect to server.");
            }
        }

        public async Task<bool> CreateUsuarioAsync(Usuario usuario)
        {
            // 1. Calcular el Hash SHA-256 de la contraseña
            if (!string.IsNullOrEmpty(usuario.Password))
            {
                using (SHA256 sha256Hash = SHA256.Create())
                {
                    // Convertimos la contraseña a bytes, calculamos el hash y lo pasamos a hexadecimal
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(usuario.Password));
                    StringBuilder builder = new StringBuilder();
                    foreach (byte b in bytes)
                    {
                        builder.Append(b.ToString("x2"));
                    }
                    usuario.Password = builder.ToString();
                }
            }
            var serializer = new DataContractJsonSerializer(typeof(Usuario));
            using (var stream = new System.IO.MemoryStream())
            {
                serializer.WriteObject(stream, usuario);
                string json = Encoding.UTF8.GetString(stream.ToArray());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PostAsync($"{BaseUrl}/usuarios", content);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> UpdateUsuarioAsync(Usuario usuario)
        {
            var serializer = new DataContractJsonSerializer(typeof(Usuario));
            using (var stream = new System.IO.MemoryStream())
            {
                serializer.WriteObject(stream, usuario);
                string json = Encoding.UTF8.GetString(stream.ToArray());
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _client.PutAsync($"{BaseUrl}/usuarios/{usuario.Id}", content);
                return response.IsSuccessStatusCode;
            }
        }

        public async Task<bool> DeleteUsuarioAsync(long id)
        {
            var response = await _client.DeleteAsync($"{BaseUrl}/usuarios/{id}");
            return response.IsSuccessStatusCode;
        }


    }
}
