using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeatherAPI_Sample {
    // a simple class to faciliate filtering functioning
    public class Filter {
        public class CompareFilter<T> : INotifyPropertyChanged {
            private T v1;
            private T v2;
            private CompareOperators o;
            public T operant1 {
                get { return v1; }
                set {
                    v1 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("operant1"));
                }
            }
            public T operant2 {
                get { return v2; }
                set {
                    v2 = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("operant2"));
                }
            }
            public CompareOperators op {
                get { return o; }
                set {
                    o = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("op"));
                }
            }
            public event PropertyChangedEventHandler PropertyChanged;
        }
        // comparison selections
        public enum CompareOperators {
            LessThan,
            GreaterThan,
            EqualTo,
            Between
        }
        // filter selections
        public enum FilterBy {
            City,
            Country,
            Latitude,
            Longitude,
            Sunrise,
            Sunset,
            TemperatureCurrent,
            TemperatureLow,
            TemperatureHigh,
            Humidity,
            Pressure,
            Visibility,
            WindDirection,
            WindSpeed,
            PrecipitationAmount,
            Precipitation,
            Cloudiness,
            None
        }
    }
}
