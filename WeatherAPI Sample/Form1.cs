using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
using static WeatherAPI_Sample.Filter;

namespace WeatherAPI_Sample {
    public partial class Form1 : Form {
        private CompareFilter<string> filter;
        public Form1() {
            InitializeComponent();
            if (DatabaseManager.IsFirstRun) DatabaseManager.PopulateDB();
            //else DatabaseManager.UpdateDB();
            DisplayWeatherData();
            filter = new CompareFilter<string>();
            // bind filters to control
            cboFilter.DataSource = Enum.GetValues(typeof(Filter.FilterBy));
            cboOperator.SelectedIndexChanged += (s, e) => {
                switch ((CompareOperators)cboOperator.SelectedItem) {
                    case CompareOperators.EqualTo:
                    case CompareOperators.GreaterThan:
                    case CompareOperators.LessThan:
                        txtFilterValue2.Text = "";
                        txtFilterValue2.Enabled = false; break;
                    default:
                        txtFilterValue2.Enabled = true; break;
                }
            };
            cboOperator.DataSource = Enum.GetValues(typeof(CompareOperators));
            // bind filter object to control
            cboOperator.DataBindings.Add("SelectedItem", filter, "op");
            txtFilterValue.DataBindings.Add("Text", filter, "operant1");
            txtFilterValue2.DataBindings.Add("Text", filter, "operant2");
            cboFilterValue.DataBindings.Add("SelectedValue", filter, "operant1");
        }
        private void DisplayWeatherData() {
            dgvResult.DataSource = DatabaseManager.GetWeatherData();
        }
        private void cboFilter_SelectedIndexChanged(object sender, EventArgs e) {
            filter.operant1 =
                filter.operant2 = null;
            ValidateControl();
            // show applicable controls depending on the selected filter
            switch((Filter.FilterBy)cboFilter.SelectedItem) {
                case Filter.FilterBy.City:
                    cboFilterValue.DataSource = DatabaseManager.GetEuropeanCities().Select(s=>new { Capital = s }).ToArray();
                    cboFilterValue.DisplayMember = "Capital";
                    cboFilterValue.ValueMember = "Capital";
                    break;
                case Filter.FilterBy.Country:
                    cboFilterValue.DataSource = DatabaseManager.GetEuropeanCountries();
                    cboFilterValue.DisplayMember = "Key";
                    cboFilterValue.ValueMember = "Value";
                    break;
                case Filter.FilterBy.WindDirection:
                    cboFilterValue.DataSource = DatabaseManager.GetAvailableWindDirections();
                    cboFilterValue.DisplayMember = "Key";
                    cboFilterValue.ValueMember = "Value";
                    break;
                case Filter.FilterBy.Precipitation: cboFilterValue.DataSource = new[] { "Yes", "No" }; break;
                case Filter.FilterBy.None: HideFilter(); break;
            }
        }

        private void btnFilter_Click(object sender, EventArgs e) {
            if (ValidateInput())
                dgvResult.DataSource = DatabaseManager.GetWeatherData((Filter.FilterBy)cboFilter.SelectedItem, filter);
            else MessageBox.Show("Invalid value! Please check input");
        }
        // determine controls' visibility
        private void ValidateControl() {
            switch ((Filter.FilterBy)cboFilter.SelectedItem) {
                case Filter.FilterBy.City:
                case Filter.FilterBy.Country:
                case Filter.FilterBy.WindDirection:
                case Filter.FilterBy.Precipitation:
                    ShowListControl(); break;
                case Filter.FilterBy.Humidity:
                case Filter.FilterBy.Pressure:
                case Filter.FilterBy.Visibility:
                case Filter.FilterBy.TemperatureCurrent:
                case Filter.FilterBy.TemperatureLow:
                case Filter.FilterBy.TemperatureHigh:
                case Filter.FilterBy.WindSpeed:
                case Filter.FilterBy.PrecipitationAmount:
                case Filter.FilterBy.Cloudiness:
                case Filter.FilterBy.Sunrise:
                case Filter.FilterBy.Sunset:
                case Filter.FilterBy.Latitude:
                case Filter.FilterBy.Longitude:
                    ShowQuantitativeControl(); break;

            }
        }
        private bool ValidateInput() {
            // involve user input
            if (!cboFilterValue.Visible) {
                switch ((Filter.FilterBy)cboFilter.SelectedItem) {
                    case Filter.FilterBy.Sunrise:
                    case Filter.FilterBy.Sunset:
                        return (DatabaseManager.ParseTime(txtFilterValue.Text) != null && DatabaseManager.ParseTime(txtFilterValue2.Text) != null);
                    default:
                        // input validation for numerical values
                        Regex r = new Regex(@"^\s*?(\d*?(?:.*\d*?))\s*?$");
                        return (r.Match(txtFilterValue.Text).Groups[1].Success &&
                            ((txtFilterValue2.Enabled && r.Match(txtFilterValue2.Text).Groups[1].Success) || !txtFilterValue2.Enabled));
                }
            }
            return true;
        }
        // show controls for filters that use a list selection
        private void ShowListControl() {
            cboFilterValue.Show();
            txtFilterValue.Visible =
                txtFilterValue2.Visible =
                cboOperator.Visible = false;
            label2.Show();
        }
        // show controls for filters that involve user input
        private void ShowQuantitativeControl() {
            cboFilterValue.Hide();
            txtFilterValue.Visible =
                txtFilterValue2.Visible =
                label2.Visible =
                cboOperator.Visible = true;
        }
        // hide all controls that is used for filtering
        private void HideFilter() {
            txtFilterValue.Hide();
            txtFilterValue2.Hide();
            cboFilterValue.Hide();
            cboOperator.Hide();
            label2.Hide();
        }
    }
}
