using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;

[assembly: ComVisible(false)]
// only include security rules stuff if in 4.0
#if _NET_L_T_4_0
#else
[assembly: SecurityRules(SecurityRuleSet.Level1)]
#endif
// [assembly: AllowPartiallyTrustedCallers]
[assembly: AssemblyTitle("One.Net.BLL")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Renderspace d.o.o.")]
[assembly: AssemblyProduct("One.Net")]
[assembly: AssemblyCopyright("Copyright 2016")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyVersion("3.3.53")]
[assembly: AssemblyKeyName("")]