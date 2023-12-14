#region System
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
#endregion

using SharpRambo.ExtensionsLib;

namespace SharpRambo.SystemInfoLib
{
    public static class SystemInfo
    {
        private static ConsoleX _cmd = null;
        public static ConsoleX CMD {
            get => _cmd ?? new ConsoleX();
            set => _cmd = value ?? new ConsoleX();
        }

        private static ManagementObjectCollection _processorsMOC => new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor").Get();
        private static ManagementObject _nic => (from ManagementObject mo in new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration where IPEnabled=true").Get()
                                                where Convert.ToString(mo["Description"]).IndexOf("vmware", StringComparison.CurrentCultureIgnoreCase) < 0 &&
                                                    Convert.ToString(mo["Description"]).IndexOf("virtual", StringComparison.CurrentCultureIgnoreCase) < 0 &&
                                                    Convert.ToString(mo["Description"]).IndexOf("vpn", StringComparison.CurrentCultureIgnoreCase) < 0
                                                select mo).FirstOrDefault();

        public static string MAC => Convert.ToString(_nic["MACAddress"]);
        public static string NicName => Convert.ToString(_nic["Caption"]);
        public static string NicDescription => Convert.ToString(_nic["Description"]);

        public static string Serial
        {
            get
            {
                string result = string.Empty;

                try {
#if NET5_0_OR_GREATER
                    using ManagementClass devs = new("Win32_SystemEnclosure");
#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
                    using ManagementClass devs = new ManagementClass("Win32_SystemEnclosure");
#else
                    using (ManagementClass devs = new ManagementClass("Win32_SystemEnclosure")) {
#endif
                    ManagementObjectCollection moc = devs.GetInstances();

                    foreach (ManagementObject mo in moc.Cast<ManagementObject>())
                    {
                        // SERIAL = "S" + (mo["SerialNumber"].ToString());
                        result = mo["SerialNumber"].ToString();
#if NET5_0_OR_GREATER
                        if (result.ToLower() is "chassis serial number" or "serial number" or "serial") result = string.Empty;
#else
                        if (string.Equals(result, "chassis serial number", StringComparison.CurrentCultureIgnoreCase) || string.Equals(result, "serial number", StringComparison.CurrentCultureIgnoreCase) || string.Equals(result, "serial", StringComparison.CurrentCultureIgnoreCase)) result = string.Empty;
#endif
                    }
#if !NET5_0_OR_GREATER && !NETCOREAPP3_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
                    }
#endif
                }
                catch (Exception ex) {
                    CMD.WriteException(ex);
                }

                return !result.IsNull() ? result : "No serial found!";
            }
        }

        public static List<Processor> ProcessorList
        {
            get
            {
                List<Processor> processors =
#if NET8_0_OR_GREATER
                [];
#elif NET5_0_OR_GREATER
                new();
#else
                new List<Processor>();
#endif

                if (_processorsMOC?.Count > 0)
                    foreach (ManagementObject mo in _processorsMOC.Cast<ManagementObject>())
                        processors.Add(new Processor(mo));

                return processors;
            }
        }
    }
}
