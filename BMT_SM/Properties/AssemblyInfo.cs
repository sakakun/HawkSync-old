﻿using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Babstats Server Manager")]
[assembly: AssemblyDescription("Stats Tracker and Server Manager for NovaLogic Delta Force series.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Babstats")]
[assembly: AssemblyProduct("Babstats")]
[assembly: AssemblyCopyright("")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("cbb3ea58-fc06-420e-b6d2-76e6b2e6bf65")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0")]
[assembly: AssemblyFileVersion("1.0.0")]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]
[assembly: NeutralResourcesLanguage("en")]
