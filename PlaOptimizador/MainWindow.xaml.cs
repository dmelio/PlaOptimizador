using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ServiceProcess;
using Microsoft.Win32;
using System.Management;
using System.Linq.Expressions;
using System.Drawing;
using System.Diagnostics;
using System.IO;

namespace PlaOptimizador
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private readonly string[] serviceNames = {"SysMain","wuauserv",""}
        public MainWindow()
        {
            InitializeComponent();
        }

        //Desactiva y deshabilita servicio SysMain
        private void btnSuperfetch_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "SysMain"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType (serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            } 
            catch (InvalidOperationException ex)
            { 
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void SetServiceStartupType(string serviceName, ServiceStartMode mode)
        {
            using (RegistryKey Key = Registry.LocalMachine.OpenSubKey($@"SYSTEM\CurrentControlSet\Services\{serviceName}", true))
            {
                if(Key!= null)
                {
                    Key.SetValue("start", (int)mode, RegistryValueKind.DWord);
                }
                else
                {
                    throw new InvalidOperationException("No se pudo encontrar la clave del registro del servicio.");
                }
            }
        }

        public enum ServiceStartMode
        {
            Boot= 0,
            System = 1,
            Automatic = 2,
            Manual = 3,
            Disabled = 4
        }

        private void btnWupdate_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "wuauserv"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnHWinfo_Click(object sender, RoutedEventArgs e)
        {
            txtHardwareInfo.Text = GetHardwareInfo();
        }

        private string GetHardwareInfo()
        {
           StringBuilder sb = new StringBuilder();

            //informacion de bios
            sb.AppendLine("Bios info");
            sb.AppendLine(GetBIOSInfo());

            //Informacion del CPU
            sb.AppendLine("CPU info:");
            sb.AppendLine(GetCPUInfo());

            //Informacion de la ram
            sb.AppendLine("Ram Info");
            sb.AppendLine(GetRAMInfo());

            //Informacion de la tarjeta de video
            sb.AppendLine("Video Info");
            sb.AppendLine(GetVideoInfo());

            //informacion del disco duro
            sb.AppendLine("Disk Drive Info:");
            sb.AppendLine(GetDiskInfo());

            //informacion del adaptador de red
            sb.AppendLine("Network Adapter Info:");
            sb.AppendLine(GetInfo("Win32_NetworkAdapter","Name","MACAddress"));
            return sb.ToString();
        }

        private string? GetVideoInfo()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
                foreach (ManagementObject obj in searcher.Get())
                {

                    sb.AppendLine($"Descripcion:{obj["Description"]}");
                    sb.AppendLine($"Nombre: {obj["Name"]}");
                    //ulong ARAM = (ulong)obj["AdapterRAM"];
                    //sb.AppendLine($"Capacidad: {ARAM / (1024 * 1024 * 1024)}GB");
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"ERROR retrieving Video information: {ex.Message}");
            }

            return sb.ToString();
        }

        private string? GetCPUInfo()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {

                    sb.AppendLine($"Nombre:{obj["Name"]}");
                    sb.AppendLine($"Cantidad de nucleos: {obj["NumberOfCores"]}");
                    sb.AppendLine($"Numero de procesadores: {obj["NumberOfLogicalProcessors"]}");
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"ERROR retrieving CPU information: {ex.Message}");
            }

            return sb.ToString();
        }

        private string? GetBIOSInfo()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                foreach (ManagementObject obj in searcher.Get())
                {
                    
                    sb.AppendLine($"Bios Version:{obj["SMBIOSBIOSVersion"]}");
                    sb.AppendLine($"Manufacturer: {obj["Manufacturer"]}");
                    sb.AppendLine($"Release Date: {ManagementDateTimeConverter.ToDateTime(obj["ReleaseDate"].ToString())}");
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"ERROR retrieving BIOS information: {ex.Message}");
            }

            return sb.ToString();
        }

        private string? GetDiskInfo()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                foreach (ManagementObject obj in searcher.Get())
                {
                    ulong size = (ulong)obj["Size"];
                    sb.AppendLine($"Size: {size / (1024 * 1024 * 1024)}GB");

                    sb.AppendLine($"Model: {obj["Model"]}");
                    sb.AppendLine();
                }
            }
            catch (Exception ex) 
            {
                sb.AppendLine($"ERROR retrieving Disk information: {ex.Message}");
            }
            return sb.ToString();
        }

        private string? GetRAMInfo()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
                foreach (ManagementObject obj in searcher.Get())
                {
                    //Capacidad en GB
                    ulong capacity = (ulong)obj["Capacity"];
                    sb.AppendLine($"Capacity:{capacity / (1024 * 1024 * 1024)}GB");
                    //modelo
                    sb.AppendLine($"Model: {obj["Manufacturer"]} {obj["PartNumber"]}");
                    //frecuencia
                    sb.AppendLine($"Speed: {obj["Speed"]}MHz");
                    sb.AppendLine();
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"ERROR retrieving RAM information: {ex.Message}");
            }

            return sb.ToString();
        }
      

        private string? GetInfo(string wmiClass, params string[] properties)
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT * FROM {wmiClass}");
                foreach(ManagementObject obj in searcher.Get())
                {
                    foreach(string property in properties)
                    {
                        sb.AppendLine($"{property}: {obj[property]}");
                    }
                    sb.AppendLine();
                }
            }
            catch(Exception ex)
            {
                sb.AppendLine($"ERROR retrieving{wmiClass} information: {ex.Message}");
            }
            return sb.ToString();
        }

        private void btnAutXbox_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "XblAuthManager"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnMapas_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "MapsBroker"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnColaImpresion_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "Spooler"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnDiagExServ_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "diagsvc"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnTarjetaintel_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "SCPolicySvc"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnTelemetria_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "DiagTrack"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnNotImpresora_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "PrintNotify"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnFax_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "Fax"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnInterfazhyperv_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "vmicguestinterface"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnJuegosXbox_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "XblGameSave"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        /// <summary>
        /// listo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCompatibilidadBT_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "bthserv"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnServicioBiometrico_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "WbioSrvc"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnCifradoBitlocker_Click(object sender, RoutedEventArgs e)
        {

            string serviceName = "BDESVC"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnDirectivaDiag_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "DPS"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnGeolocalizacion_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "lfsvc"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnHostHyperv_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "HvHost"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnEscrituraMano_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "TabletInputService"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnPruebaComercial_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "RetailDemo"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnEnlaceBT_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "BTAGService"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnConexionMobile_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "icssvc"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnRedXbox_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "XboxNetApiSvc"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnServicioFrameserver_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "FrameServer"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnInformeErrores_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "WerSvc"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnServicioTelefonico_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "PhoneSvc"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnVolumetricAudio_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "VacSvc"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnTarjetaInteligente_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "SCardSvr"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnWalletService_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "WalletService"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnWindowsMixedReality_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "MixedRealityOpenXRSvc"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnXboxAccesory_Click(object sender, RoutedEventArgs e)
        {
            string serviceName = "XboxGipSvc"; //nombre del servicio

            try
            {
                ServiceController sc = new ServiceController(serviceName);
                if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
                {
                    //Parar el servicio
                    sc.Stop();
                    sc.WaitForStatus(ServiceControllerStatus.Stopped);
                    //Deshabilitar servicio
                    SetServiceStartupType(serviceName, ServiceStartMode.Disabled);
                    MessageBox.Show("Servicio desactivado y deshabilitado.");
                }
                else if (sc.Status == ServiceControllerStatus.Stopped)
                {
                    SetServiceStartupType(serviceName, ServiceStartMode.Automatic);
                    sc.Start();
                    sc.WaitForStatus(ServiceControllerStatus.Running);
                    MessageBox.Show("Servicio habilitado y activado.");
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                MessageBox.Show($"Error:{ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}");
            }
        }

        private void btnLimpiaDisco_Click(object sender, RoutedEventArgs e)
        {
            RunDiskCleanup();
        }

        private void RunDiskCleanup()
        {
            try
            {
                Process.Start("cleanmgr.exe");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error running Disk Cleanup: {ex.Message}");
            }
        }
        private void ClearTempFolder()
        {
            try
            {
                string tempPath = System.IO.Path.GetTempPath();
                DirectoryInfo di = new DirectoryInfo(tempPath);
                
                foreach (FileInfo file in di.GetFiles())
                {
                    try
                    {
                        file.Delete();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar archivos: {ex.Message}");
                    }
                }
                foreach(DirectoryInfo dir in di.GetDirectories())
                {
                    try
                    {
                        dir.Delete(true);
                    }catch(Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar carpetas: {ex.Message}");
                    }
                }
                MessageBox.Show("Archivos temporales eliminados.");
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error clearing temp folder: {ex.Message}");
            }
        }

        private void btnTemp_Click(object sender, RoutedEventArgs e)
        {
            ClearTempFolder();
        }
    }
}