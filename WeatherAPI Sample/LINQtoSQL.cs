using WeatherAPI_Sample;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using static WeatherAPI_Sample.Filter;

public static class DatabaseManager {
    private const string API_URL = "http://api.openweathermap.org/data/2.5/weather?appid=8f8e1fa2eb2a31ca384a31ed27e79541&mode=xml&units=imperial&type=accurate";
    // insert weather data into the database
    // called to initialize and populate the database with weather data
    public static void PopulateDB() {
        Forecast.Current[] current = GetEuropeCitiesWeatherData();
        using (var db = new LINQtoSQLDataContext()) {
            foreach (var forecast in current) {
                PopulateCityAndCoordTable(db, forecast);
                PopulateWeatherDataTables(db, forecast);
                db.SubmitChanges();
            }
        }
    }
    // update weather data to reflect new weather data
    public static void UpdateDB() {
        Forecast.Current[] current = GetEuropeCitiesWeatherData();
        using (var db = new LINQtoSQLDataContext()) {
            // remove all rows in all tables other than location and coord
            // location and coord are not removed because its not varying.
            db.weathers.DeleteAllOnSubmit(db.weathers);
            db.astronomies.DeleteAllOnSubmit(db.astronomies);
            db.temperatures.DeleteAllOnSubmit(db.temperatures);
            db.atmospheres.DeleteAllOnSubmit(db.atmospheres);
            db.winds.DeleteAllOnSubmit(db.winds);
            db.conditions.DeleteAllOnSubmit(db.conditions);
            db.SubmitChanges();
            foreach (var forecast in current) {
                PopulateWeatherDataTables(db, forecast);
                db.SubmitChanges();
            }
        }
    }
    // populate location and coord tables
    private static void PopulateCityAndCoordTable(LINQtoSQLDataContext db, Forecast.Current forecast) {
        location loc = new location() {
            id = (int)forecast.City.Id,
            city = forecast.City.Name,
            country = forecast.City.Country
        };
        db.locations.InsertOnSubmit(loc);
        db.SubmitChanges();
        coord coordination = new coord() {
            latitude = forecast.City.Coordinate.Latitude,
            longitude = forecast.City.Coordinate.Longitude,
            location_id = (int)forecast.City.Id
        };
        db.coords.InsertOnSubmit(coordination);
    }
    // populate the rest of the tables
    private static void PopulateWeatherDataTables(LINQtoSQLDataContext db, Forecast.Current forecast) {
        weather weather = new weather() {
            code = forecast.Weather.Number,
            description = forecast.Weather.Value,
            icon = forecast.Weather.Icon,
            last_updated = DateTime.Parse(forecast.LastUpdate.Value),
            location_id = (int)forecast.City.Id
        };
        db.weathers.InsertOnSubmit(weather);
        db.SubmitChanges();
        astronomy astronomy = new astronomy() {
            sunrise = DateTime.Parse(forecast.City.Sun.Rise),
            sunset = DateTime.Parse(forecast.City.Sun.Set),
            weather_id = weather.id
        };
        temperature temp = new temperature() {
            value = forecast.Temperature.Value,
            min = forecast.Temperature.Min,
            max = forecast.Temperature.Max,
            unit = forecast.Temperature.Unit,
            weather_id = weather.id
        };
        atmosphere atmosphere = new atmosphere() {
            humidity = forecast.Humidity.Value,
            pressure = forecast.Pressure.Value,
            visibility = float.Parse(forecast.Visibility.Value ?? "0"),
            weather_id = weather.id
        };
        wind wind = new wind() {
            direction = (int)forecast.Wind.Direction.Value,
            direction_code = forecast.Wind.Direction.Code,
            direction_name = forecast.Wind.Direction.Name,
            description = forecast.Wind.Direction.Name,
            speed = forecast.Wind.Speed.Value,
            weather_id = weather.id
        };
        condition condition = new condition() {
            cloudiness = forecast.Clouds.Value,
            cloudiness_name = forecast.Clouds.Name,
            precipitation = float.Parse(forecast.Precipitation.Value ?? "0"),
            precipitation_mode = forecast.Precipitation.Mode,
            weather_id = weather.id
        };
        db.astronomies.InsertOnSubmit(astronomy);
        db.temperatures.InsertOnSubmit(temp);
        db.atmospheres.InsertOnSubmit(atmosphere);
        db.winds.InsertOnSubmit(wind);
        db.conditions.InsertOnSubmit(condition);
    }

    public static bool IsFirstRun {
        get {
            bool result;
            using (var db = new LINQtoSQLDataContext()) {
                result = !(db.locations.Count() > 0);
            }
            return result;
        }
    }
    public static IEnumerable<object> GetWeatherData() {
        return GetWeatherData(FilterBy.None, null);
    }
    public static IEnumerable<object> GetWeatherData(FilterBy filter, Filter.CompareFilter<string> value) {
        LINQtoSQLDataContext db = new LINQtoSQLDataContext();
        var data = (from loc in db.locations
                    join coord in db.coords on loc.id equals coord.location_id
                    join weather in db.weathers on loc.id equals weather.location_id
                    join astronomy in db.astronomies on weather.id equals astronomy.weather_id
                    join temp in db.temperatures on weather.id equals temp.weather_id
                    join atmosphere in db.atmospheres on weather.id equals atmosphere.weather_id
                    join wind in db.winds on weather.id equals wind.weather_id
                    join cond in db.conditions on weather.id equals cond.weather_id
                    select new {
                        City = loc.city,
                        Region = loc.country,
                        Latitude = Math.Round((float)coord.latitude, 2, MidpointRounding.ToEven),
                        Longitude = Math.Round((float)coord.longitude, 2, MidpointRounding.ToEven),
                        Description = weather.description,
                        LastUpdated = weather.last_updated,
                        Sunrise = astronomy.sunrise.Value.TimeOfDay,
                        Sunset = astronomy.sunset.Value.TimeOfDay,
                        CurrentTemperature = Math.Round((float)temp.value, 2, MidpointRounding.ToEven),
                        Low = Math.Round((float)temp.min, 2, MidpointRounding.ToEven),
                        High = Math.Round((float)temp.max, 2, MidpointRounding.ToEven),
                        Humidity = atmosphere.humidity,
                        Pressure = Math.Round((float)atmosphere.pressure, 2, MidpointRounding.ToEven),
                        atmosphere.visibility,
                        WindDirection = wind.direction_code,
                        WindSpeed = Math.Round((float)wind.speed, 2, MidpointRounding.ToEven),
                        cond.cloudiness,
                        cond.cloudiness_name,
                        Precipitation = Math.Round((float)cond.precipitation, 2, MidpointRounding.ToEven),
                        cond.precipitation_mode
                    }
             );
        // data filtering
        switch (filter) {
            case FilterBy.City: return data.Where(d => d.City == value.operant1);
            case FilterBy.Country:
                var countryCode = db.country_codes.SingleOrDefault(cc => cc.iso == value.operant1);
                return data.Where(d => d.Region == countryCode.iso);
            case FilterBy.WindDirection: return data.Where(d => d.WindDirection == value.operant1);
            case FilterBy.Precipitation: return data.Where(d => d.precipitation_mode == (value.operant1 == "Yes" ? "rain" : "no"));
            case FilterBy.Sunrise:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.Sunrise < DateTime.Parse(ParseTime(value.operant1)).TimeOfDay);
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.Sunrise > DateTime.Parse(ParseTime(value.operant1)).TimeOfDay);
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.Sunrise == DateTime.Parse(ParseTime(value.operant1)).TimeOfDay);
                    case Filter.CompareOperators.Between: return data.Where(d => DateTime.Parse(ParseTime(value.operant1)).TimeOfDay < d.Sunrise && d.Sunrise < DateTime.Parse(ParseTime(value.operant2)).TimeOfDay);
                }
                return null;
            case FilterBy.Sunset:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.Sunset < DateTime.Parse(ParseTime(value.operant1)).TimeOfDay);
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.Sunset > DateTime.Parse(ParseTime(value.operant1)).TimeOfDay);
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.Sunset == DateTime.Parse(ParseTime(value.operant1)).TimeOfDay);
                    case Filter.CompareOperators.Between: return data.Where(d => DateTime.Parse(ParseTime(value.operant1)).TimeOfDay < d.Sunset && d.Sunset < DateTime.Parse(ParseTime(value.operant2)).TimeOfDay);
                }
                return null;
            case FilterBy.TemperatureCurrent:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.CurrentTemperature < float.Parse(value.operant1));
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.CurrentTemperature > float.Parse(value.operant1));
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.CurrentTemperature == float.Parse(value.operant1));
                    case Filter.CompareOperators.Between: return data.Where(d => float.Parse(value.operant1) < d.CurrentTemperature && d.CurrentTemperature < float.Parse(value.operant2));
                }
                return null;
            case FilterBy.TemperatureLow:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.Low < float.Parse(value.operant1));
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.Low > float.Parse(value.operant1));
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.Low == float.Parse(value.operant1));
                    case Filter.CompareOperators.Between: return data.Where(d => float.Parse(value.operant1) < d.Low && d.Low < float.Parse(value.operant2));
                }
                return null;
            case FilterBy.TemperatureHigh:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.High < float.Parse(value.operant1));
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.High > float.Parse(value.operant1));
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.High == float.Parse(value.operant1));
                    case Filter.CompareOperators.Between: return data.Where(d => float.Parse(value.operant1) < d.High && d.High < float.Parse(value.operant2));
                }
                return null;
            case FilterBy.Humidity:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.Humidity < float.Parse(value.operant1));
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.Humidity > float.Parse(value.operant1));
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.Humidity == float.Parse(value.operant1));
                    case Filter.CompareOperators.Between: return data.Where(d => float.Parse(value.operant1) < d.Humidity && d.Humidity < float.Parse(value.operant2));
                }
                return null;
            case FilterBy.Pressure:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.Pressure < float.Parse(value.operant1));
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.Pressure > float.Parse(value.operant1));
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.Pressure == float.Parse(value.operant1));
                    case Filter.CompareOperators.Between: return data.Where(d => float.Parse(value.operant1) < d.Pressure && d.Pressure < float.Parse(value.operant2));
                }
                return null;
            case FilterBy.Visibility:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.visibility < float.Parse(value.operant1));
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.visibility > float.Parse(value.operant1));
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.visibility == float.Parse(value.operant1));
                    case Filter.CompareOperators.Between: return data.Where(d => float.Parse(value.operant1) < d.visibility && d.visibility < float.Parse(value.operant2));
                }
                return null;
            case FilterBy.WindSpeed:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.WindSpeed < float.Parse(value.operant1));
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.WindSpeed > float.Parse(value.operant1));
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.WindSpeed == float.Parse(value.operant1));
                    case Filter.CompareOperators.Between: return data.Where(d => float.Parse(value.operant1) < d.WindSpeed && d.WindSpeed < float.Parse(value.operant2));
                }
                return null;
            case FilterBy.PrecipitationAmount:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.Precipitation < float.Parse(value.operant1));
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.Precipitation > float.Parse(value.operant1));
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.Precipitation == float.Parse(value.operant1));
                    case Filter.CompareOperators.Between: return data.Where(d => float.Parse(value.operant1) < d.Precipitation && d.Precipitation < float.Parse(value.operant2));
                }
                return null;
            case FilterBy.Latitude:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.Latitude < float.Parse(value.operant1));
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.Latitude > float.Parse(value.operant1));
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.Latitude == float.Parse(value.operant1));
                    case Filter.CompareOperators.Between: return data.Where(d => float.Parse(value.operant1) < d.Latitude && d.Latitude < float.Parse(value.operant2));
                }
                return null;
            case FilterBy.Longitude:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.Longitude < float.Parse(value.operant1));
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.Longitude > float.Parse(value.operant1));
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.Longitude == float.Parse(value.operant1));
                    case Filter.CompareOperators.Between: return data.Where(d => float.Parse(value.operant1) < d.Longitude && d.Longitude < float.Parse(value.operant2));
                }
                return null;
            case FilterBy.Cloudiness:
                switch (value.op) {
                    case Filter.CompareOperators.LessThan: return data.Where(d => d.cloudiness < float.Parse(value.operant1));
                    case Filter.CompareOperators.GreaterThan: return data.Where(d => d.cloudiness > float.Parse(value.operant1));
                    case Filter.CompareOperators.EqualTo: return data.Where(d => d.cloudiness == float.Parse(value.operant1));
                    case Filter.CompareOperators.Between: return data.Where(d => float.Parse(value.operant1) < d.cloudiness && d.cloudiness < float.Parse(value.operant2));
                }
                return null;
            default: return data; // no filtering; show all data
        }
    }
    // get all cities in Europe
    public static string[] GetEuropeanCities() {
        using (var db = new LINQtoSQLDataContext()) {
            return db.europe_capitals.Select(c => c.capital).ToArray();
        }
    }
    // get all countries in Europe
    public static IEnumerable<KeyValuePair<string, string>> GetEuropeanCountries() {
        LINQtoSQLDataContext db = new LINQtoSQLDataContext();
        return (
                from ec in db.europe_capitals
                join cc in db.country_codes
                on ec.country equals cc.country
                select new KeyValuePair<string, string>(cc.country, cc.iso)
                );

    }
    // get weather data for all cities in europe
    private static Forecast.Current[] GetEuropeCitiesWeatherData() {
        List<Forecast.Current> weatherdata = new List<Forecast.Current>();
        using (var db = new LINQtoSQLDataContext()) {
            // retrieve all cities in europe
            var cities = (from ec in db.europe_capitals
                          join cc in db.country_codes on ec.country equals cc.country
                          select new { ec.capital, cc.country, cc.iso }
                          );
            XDocument doc;
            // pull weather data of the cities from web api
            foreach (var city in cities) {
                try {
                    doc = XDocument.Load(API_URL + $"&q={city.capital.ToLower()},{city.iso.ToLower()}");
                    XmlSerializer ser = new XmlSerializer(typeof(Forecast.Current));
                    weatherdata.Add(ser.Deserialize(doc.CreateReader()) as Forecast.Current);
                }
                catch { }
            }
        }
        return weatherdata.ToArray();
    }
    // get available wind directions
    public static IEnumerable<KeyValuePair<string, string>> GetAvailableWindDirections() {
        LINQtoSQLDataContext db = new LINQtoSQLDataContext();
        return (
            from w in db.winds
            where w.direction_code != "" 
            select new KeyValuePair<string, string>(w.direction_name, w.direction_code)).Distinct();

    }
    public static string ParseTime(string text) {
        //Regex r = new Regex(@"^\s*?0*?([0-2]?(?(?<=[0-1])[0-9]|[0-4]))(?::([0-9](?(?<=[0-5])[0-9]))?)?");
        //Match m = r.Match(text);
        //if (!m.Success) return null;
        //int hour = int.Parse(m.Groups[1].Value);
        //int min = int.Parse(m.Groups[2].Value);
        //if ((hour >= 0 && hour < 24 && min >= 0 && min < 60) || (hour == 24 && min == 0)) return m.Value;
        //return null;
        DateTime result;
        DateTime.TryParse(text, out result);
        return result.ToShortTimeString();
    }
}