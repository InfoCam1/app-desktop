using System.Runtime.Serialization;

namespace InfoCam.Models
{
    [DataContract]
    public class Camera
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "nombre")]
        public string Nombre { get; set; }

        [DataMember(Name = "latitud")]
        public string LatitudString { get; set; }

        [IgnoreDataMember]
        public double Latitud
        {
            get
            {
                if (double.TryParse(LatitudString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double v)) return v;
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
                if (double.TryParse(LongitudString, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double v)) return v;
                return 0;
            }
            set { LongitudString = value.ToString(System.Globalization.CultureInfo.InvariantCulture); }
        }

        [DataMember(Name = "imagen")]
        public string Imagen { get; set; }

        [DataMember(Name = "activa")]
        public bool Activa { get; set; }
    }
}
