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
        private static ConsoleX _CMD = null;
        public static ConsoleX CMD {
            get => _CMD ?? new ConsoleX();
            set => _CMD = value ?? new ConsoleX();
        }

        private static ManagementObjectCollection ProcessorsMOC => new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor").Get();
        private static ManagementObject NIC => (from ManagementObject mo in new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapterConfiguration where IPEnabled=true").Get()
                                                where !Convert.ToString(mo["Description"]).ToLower().Contains("vmware") &&
                                                      !Convert.ToString(mo["Description"]).ToLower().Contains("virtual") &&
                                                      !Convert.ToString(mo["Description"]).ToLower().Contains("vpn")
                                                select mo).FirstOrDefault();

        public static string MAC => Convert.ToString(NIC["MACAddress"]);
        public static string NicName => Convert.ToString(NIC["Caption"]);
        public static string NicDescription => Convert.ToString(NIC["Description"]);

        public static string Serial
        {
            get
            {
                string result = string.Empty;

                try {
#if NET5_0_OR_GREATER
                    using ManagementClass devs = new(@"Win32_SystemEnclosure");
#elif NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
                    using ManagementClass devs = new ManagementClass(@"Win32_SystemEnclosure");
#else
                    using (ManagementClass devs = new ManagementClass(@"Win32_SystemEnclosure")) {
#endif
                    ManagementObjectCollection moc = devs.GetInstances();

                    foreach (ManagementObject mo in moc)
                    {
                        // Prefix für Hallstadt
                        // SERIAL = "S" + (mo["SerialNumber"].ToString());
                        result = mo["SerialNumber"].ToString();
#if NET5_0_OR_GREATER
                        if (result.ToLower() is "chassis serial number" or "serial number" or "serial") result = string.Empty;
#else
                        if (result.ToLower() == "chassis serial number" || result.ToLower() == "serial number" || result.ToLower() == "serial") result = string.Empty;
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
#if NET5_0_OR_GREATER
                List<Processor> processors = new();
#else
                List<Processor> processors = new List<Processor>();
#endif

                if (
                    ProcessorsMOC != null &&
                    ProcessorsMOC.Count > 0
                )
                    foreach (ManagementObject mo in ProcessorsMOC)
                        processors.Add(new Processor(mo));

                return processors;
            }
        }
    }

    public class Processor
    {
        public string Name { get; }
        public string Architecture { get; }
        public uint Cores { get; }
        public uint LogicalCores { get; }
        public string Socket { get; }
        public string Cache { get => L2Cache.ToString() + "M [L2], " + L3Cache.ToString() + "M [L3]"; }
        public uint L2Cache { get; }
        public uint L3Cache { get; }
        public string Manufacturer { get; }
        public string Family { get; }
        public string Type { get; }
        public string Status { get; }

        public Processor(ManagementObject processorMO)
        {
            Name = Convert.ToString(processorMO[nameof(Name)]);
            Architecture = parseArch(Convert.ToUInt16(processorMO[nameof(Architecture)]));
            Cores = Convert.ToUInt32(processorMO["NumberOfCores"]);
            LogicalCores = Convert.ToUInt32(processorMO["NumberOfLogicalProcessors"]);
            Socket = Convert.ToString(processorMO["SocketDesignation"]);
            L2Cache = Convert.ToUInt32(processorMO["L2CacheSize"]);
            L3Cache = Convert.ToUInt32(processorMO["L3CacheSize"]);
            Manufacturer = Convert.ToString(processorMO[nameof(Manufacturer)]);
            Family = Convert.ToString(processorMO["Caption"]);
            Type = parseType(Convert.ToUInt16(processorMO["ProcessorType"]));
            Status = parseStatus(Convert.ToUInt16(processorMO["CpuStatus"]));
        }

        private static string parseStatus(ushort statusID)
        {
#if NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            return statusID switch
            {
                1 => "Enabled",
                2 => "Disabled by User via BIOS Setup",
                3 => "Disabled by BIOS (POST Error)",
                4 => "Idle",
#if NET5_0_OR_GREATER
                5 or 6 => "Reserved",
#else
                5 => "Reserved",
                6 => "Reserved",
#endif
                7 => "Other",
                _ => "Unknown",
            };
#else
            switch (statusID)
            {
                case 1:
                    return "Enabled";
                case 2:
                    return "Disabled by User via BIOS Setup";
                case 3:
                    return "Disabled by BIOS (POST Error)";
                case 4:
                    return "Idle";
                case 5:
                case 6:
                    return "Reserved";
                case 7:
                    return "Other";
                default:
                    return "Unknown";
            }
#endif
            }

        private static string parseType(ushort typeID)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            return typeID switch
            {
                1 => "Other",
                3 => "Central Processor",
                4 => "Math Processor",
                5 => "DSP Processor",
                6 => "Video Processor",
                _ => "Unknown",
            };
#else
            switch (typeID)
            {
                case 1:
                    return "Other";
                case 3:
                    return "Central Processor";
                case 4:
                    return "Math Processor";
                case 5:
                    return "DSP Processor";
                case 6:
                    return "Video Processor";
                default:
                    return "Unknown";
            }
#endif
        }

        private static string parseArch(ushort archID)
        {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER || NET5_0_OR_GREATER
            return archID switch
            {
                0 => "x86",
                1 => "MIPS",
                2 => "Alpha",
                3 => "PowerPC",
                5 => "ARM",
                6 => "ia64",
                9 => "x64",
                _ => "Unknown",
            };
#else
            switch (archID)
            {
                case 0:
                    return "x86";
                case 1:
                    return "MIPS";
                case 2:
                    return "Alpha";
                case 3:
                    return "PowerPC";
                case 5:
                    return "ARM";
                case 6:
                    return "ia64";
                case 9:
                    return "x64";
                default:
                    return "Unknown";
            }
#endif
        }
    }
}
