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

        [DataMember(Name = "imagen")]
        public string Imagen { get; set; }

        [DataMember(Name = "activa")]
        public bool Activa { get; set; }
    }
}
