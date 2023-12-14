// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Windows only library!
[assembly: SuppressMessage("Interoperability", "CA1416:Plattformkompatibilität überprüfen")]

[assembly: SuppressMessage("Style", "IDE0022:Ausdruckskörper für Methode verwenden", Justification = "<Ausstehend>", Scope = "member", Target = "~M:SharpRambo.SystemInfoLib.Processor.parseStatus(System.UInt16)~System.String")]
[assembly: SuppressMessage("Style", "IDE0022:Ausdruckskörper für Methode verwenden", Justification = "<Ausstehend>", Scope = "member", Target = "~M:SharpRambo.SystemInfoLib.Processor.parseType(System.UInt16)~System.String")]
[assembly: SuppressMessage("Style", "IDE0022:Ausdruckskörper für Methode verwenden", Justification = "<Ausstehend>", Scope = "member", Target = "~M:SharpRambo.SystemInfoLib.Processor.parseArch(System.UInt16)~System.String")]
