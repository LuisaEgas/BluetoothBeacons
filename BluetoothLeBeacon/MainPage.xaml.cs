using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace BluetoothLeBeacon
{
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        IAdapter adapter;
        IBluetoothLE bluetoothBLE;
        ObservableCollection<IDevice> list = new ObservableCollection<IDevice>();
        private IDevice device;

        public MainPage()
        {
            InitializeComponent();
            bluetoothBLE = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            list = new ObservableCollection<IDevice>();
            DevicesList.ItemsSource = list;
        }
        private void ButtonSearchDevice(object sender, EventArgs e)
        {
            Thread ThreadBeacon = new Thread(SearchDevice);
            //ThreadBeacon.Abort();
            ThreadBeacon.Start();
        }

        private async void SearchDevice()
        {
            if (bluetoothBLE.State == BluetoothState.Off)
            {
                await DisplayAlert("Información", "Bluetooth deshabilitado.", "OK");
            }
            else
            {
                list.Clear();
                adapter.ScanTimeout = 10000;
                adapter.ScanMode = ScanMode.Balanced;
                adapter.DeviceDiscovered += (obj, a) =>
                {
                    if (!list.Contains(a.Device))
                    {
                        list.Add(a.Device);
                    }
                };
                await adapter.StartScanningForDevicesAsync();
            }
            Thread.Sleep(3000);
            SearchDevice();
        }

        private async void DevicesList_OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            device = DevicesList.SelectedItem as IDevice;
            await DisplayAlert("RSSI", "Rssi: " + device.Rssi + " \n Distancia: " + calculateAccuracy(device.Rssi), "Aceptar");
            /**
            var result = await DisplayAlert("Información", "Desea conectar este dispositivo?", "Aceptar", "Cancelar");
            if (!result) return;           

            await adapter.StopScanningForDevicesAsync();
            try
            {
                await adapter.ConnectToDeviceAsync(device);
                await DisplayAlert("Conectado", "Status:" + device.State, "OK");
            }
            catch (DeviceConnectionException ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }**/
        }

        public double calculateAccuracy(double rssi)
        {
            var txPower = -59;
            if (rssi == 0)
            {
                return -1; // if we cannot determine accuracy, return -1.
            }
            double ratio = rssi * 1.0 / txPower;
            if (ratio < 1.0)
            {
                return Math.Pow(ratio, 10);
            }
            else
            {
                double accuracy = (0.89976) * Math.Pow(ratio, 7.7095) + 0.111;
                return accuracy;
            }
        }
    }
}
