using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WeatherAPI_Sample {
    // class intended for serialization or deserialization of xml documents
    public class Forecast {
        [XmlRoot("current")]
        public class Current {
            [XmlElement("city")]
            public City City { get; set; }
            [XmlElement("temperature")]
            public Temperature Temperature { get; set; }
            [XmlElement("humidity")]
            public Humidity Humidity { get; set; }
            [XmlElement("pressure")]
            public Pressure Pressure { get; set; }
            [XmlElement("wind")]
            public Wind Wind { get; set; }
            [XmlElement("clouds")]
            public Clouds Clouds { get; set; }
            [XmlElement("visibility")]
            public Visibility Visibility { get; set; }
            [XmlElement("precipitation")]
            public Precipitation Precipitation { get; set; }
            [XmlElement("weather")]
            public Weather Weather { get; set; }
            [XmlElement("lastupdate")]
            public LastUpdate LastUpdate { get; set; }
        }
        public class City {
            [XmlAttribute("id")]
            public long Id { get; set; }
            [XmlAttribute("name")]
            public string Name { get; set; }
            [XmlElement("coord")]
            public CoordinateInfo Coordinate { get; set; }
            [XmlElement("country")]
            public string Country { get; set; }
            [XmlElement("sun")]
            public SunInfo Sun { get; set; }
            public class CoordinateInfo {
                [XmlAttribute("lon")]
                public float Longitude { get; set; }
                [XmlAttribute("lat")]
                public float Latitude { get; set; }
            }
            public class SunInfo {
                [XmlAttribute("rise")]
                public string Rise { get; set; }
                [XmlAttribute("set")]
                public string Set { get; set; }
            }
        }
        public class LastUpdate {
            [XmlAttribute("value")]
            public string Value { get; set; }
        }

        public class Weather {
            [XmlAttribute("number")]
            public int Number { get; set; }
            [XmlAttribute("value")]
            public string Value { get; set; }
            [XmlAttribute("icon")]
            public string Icon { get; set; }
        }

        public class Precipitation {
            [XmlAttribute("value")]
            public string Value { get; set; }
            [XmlAttribute("mode")]
            public string Mode { get; set; }
        }

        public class Visibility {
            [XmlAttribute("value")]
            public string Value { get; set; }
        }

        public class Clouds {
            [XmlAttribute("value")]
            public int Value { get; set; }
            [XmlAttribute("name")]
            public string Name { get; set; }
        }

        public class Wind {
            [XmlElement("speed")]
            public SpeedInfo Speed { get; set; }
            [XmlElement("direction")]
            public DirectionInfo Direction { get; set; }
            public class SpeedInfo {
                [XmlAttribute("value")]
                public float Value { get; set; }
                [XmlAttribute("name")]
                public string Name { get; set; }
            }
            public class DirectionInfo {
                [XmlAttribute("value")]
                public float Value { get; set; }
                [XmlAttribute("code")]
                public string Code { get; set; }
                [XmlAttribute("name")]
                public string Name { get; set; }
            }
        }

        public class Pressure {
            [XmlAttribute("value")]
            public float Value { get; set; }
            [XmlAttribute("unit")]
            public string Unit { get; set; }
        }

        public class Humidity {
            [XmlAttribute("value")]
            public float Value { get; set; }
            [XmlAttribute("unit")]
            public string Unit { get; set; }
        }

        public class Temperature {
            [XmlAttribute("value")]
            public float Value { get; set; }
            [XmlAttribute("min")]
            public float Min { get; set; }
            [XmlAttribute("max")]
            public float Max { get; set; }
            [XmlAttribute("unit")]
            public string Unit { get; set; }
        }
    }
}