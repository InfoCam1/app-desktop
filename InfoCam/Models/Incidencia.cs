using System;
using System.Runtime.Serialization;

namespace InfoCam.Models
{
    [DataContract]
    public class Incidencia
    {
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public long Id { get; set; }

        [DataMember(Name = "tipoIncidencia")]
        public string TipoIncidencia { get; set; }

        [DataMember(Name = "externalId", EmitDefaultValue = false)]
        public string External_id { get; set; }

        //[DataMember(Name = "usuario_id", EmitDefaultValue = false)]
        //public long Usuario_id { get; set; }

        [DataMember(Name = "causa")]
        public string Causa { get; set; }

        [DataMember(Name = "nombre")]
        public string Nombre { get; set; }

        [DataMember(Name = "descripcion")]
        public string Descripcion { get; set; }

        [DataMember(Name = "fecha_inicio")]
        public string Fecha_inicio_String { get; set; }

        [IgnoreDataMember]
        public DateTime? Fecha_inicio 
        { 
            get 
            {
                if (DateTime.TryParse(Fecha_inicio_String, out DateTime dt)) return dt;
                return null;
            }
            set 
            { 
                if (value.HasValue) Fecha_inicio_String = value.Value.ToString("o"); 
                else Fecha_inicio_String = null;
            }
        }

        [DataMember(Name = "fecha_fin")]
        public string Fecha_fin_String { get; set; }

        [IgnoreDataMember]
        public DateTime? Fecha_fin
        {
            get
            {
                if (DateTime.TryParse(Fecha_fin_String, out DateTime dt)) return dt;
                return null;
            }
            set
            {
                if (value.HasValue) Fecha_fin_String = value.Value.ToString("o");
                else Fecha_fin_String = null;
            }
        }

        [DataMember(Name = "latitud")]
         // The user said "latitud":"43.1" (string in JSON?) in the example: "latitud":"43.1". 
         // Wait, the user provided example: "latitud":"43.1" -> Quoted string!
         // But in C# it is double. DataContractJsonSerializer might fail if it's a string in JSON but double in C#.
         // Let's use string for deserialization and parse it, to be safe.
        public string LatitudString { get; set; }

        [IgnoreDataMember]
        public double Latitud
        {
            get 
            { 
                if (string.IsNullOrEmpty(LatitudString)) return 0;
                string processed = LatitudString.Replace(",", ".");
                if (double.TryParse(processed, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double v))
                {
                     if (v < -90 || v > 90) return 0;
                     return v;
                }
                return 0;
            }
            set { LatitudString = value.ToString(System.Globalization.CultureInfo.InvariantCulture); }
        }

        [DataMember(Name = "longitud")]
        public string LongitudString { get; set; }

        [IgnoreDataMember]
        public double Longitud
        {
            get
            {
                if (string.IsNullOrEmpty(LongitudString)) return 0;
                string processed = LongitudString.Replace(",", ".");
                if (double.TryParse(processed, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double v))
                {
                    if (v < -180 || v > 180) return 0;
                    return v;
                }
                return 0;
            }
            set { LongitudString = value.ToString(System.Globalization.CultureInfo.InvariantCulture); }
        }

        [DataMember(Name = "usuario")]
        public Usuario Usuario { get; set; }
    }
}
