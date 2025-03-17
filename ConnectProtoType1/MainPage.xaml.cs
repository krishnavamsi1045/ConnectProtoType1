using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.ApplicationModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System;
using System.Diagnostics;


namespace ConnectProtoType1
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly string GoogleApiKey = "AlzaSy1optn8yo8tadsWLdtrwgmkvoETsafcTzl";


        public MainPage()
        {
            InitializeComponent();
          
        }

        public async Task GetLocationAsync()
        {
            try
            {
                // ✅ Check if permission is already granted
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    // ✅ Properly request permission if not granted
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        await DisplayAlert("Permission Denied", "Cannot fetch location without permission.", "OK");
                        return;
                    }
                }

                // ✅ Get last known location first
                var location = await Geolocation.GetLastKnownLocationAsync();
                if (location == null)
                {
                    location = await Geolocation.GetLocationAsync(new GeolocationRequest
                    {
                        DesiredAccuracy = GeolocationAccuracy.Medium,
                        Timeout = TimeSpan.FromSeconds(18)
                    });
                }

                if (location != null)
                {
                    // ✅ Update UI on MainThread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LatitudeLabel.Text = $"Latitude: {location.Latitude}";
                        LongitudeLabel.Text = $"Longitude: {location.Longitude}";
                    });

                    // ✅ Fetch and display address
                    string address = await GetAddressFromCoordinates(location.Latitude, location.Longitude);
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        AddressLabel.Text = address;
                    });
                }
                else
                {
                    await DisplayAlert("Error", "Unable to retrieve location.", "OK");
                }
            }
            catch (FeatureNotSupportedException)
            {
                await DisplayAlert("Error", "Location is not supported on this device.", "OK");
            }
            catch (FeatureNotEnabledException)
            {
                await DisplayAlert("Error", "Please enable location services.", "OK");
            }
            catch (PermissionException)
            {
                await DisplayAlert("Error", "Location permission is denied.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Unexpected error: {ex.Message}", "OK");
            }
        }

        private async Task<string> GetAddressFromCoordinates(double latitude, double longitude)
        {
            try
            {
                string requestUrl = $"https://maps.gomaps.pro/maps/api/geocode/json?latlng={latitude},{longitude}&key=AlzaSy1optn8yo8tadsWLdtrwgmkvoETsafcTzl";

                using (HttpClient client = new HttpClient())
                {
                    string response = await client.GetStringAsync(requestUrl);
                    var json = JObject.Parse(response);
                    Debug.WriteLine(json);

                    if (json["status"].ToString() == "OK")
                    {
                        return json["results"][0]["formatted_address"].ToString();
                    }
                    else
                    {
                        return "Address not found";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error fetching address: {ex.Message}";
            }
        }

        private async void Button_Clicked(object sender, EventArgs e)
        {
            await GetLocationAsync();
        }
    }

}
