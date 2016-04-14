/* https://github.com/jwaliszko/ExpressiveAnnotations
 * Copyright (c) 2014 Jarosław Waliszko
 * Licensed MIT: http://opensource.org/licenses/MIT */

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("ExpressiveAnnotations")]
[assembly: AssemblyCopyright("Copyright © Jarosław Waliszko 2014")]
[assembly: AssemblyProduct("ExpressiveAnnotations")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("4b679902-67d7-4463-a0f0-c4e75cb39ba6")]

#if !SIGNED
[assembly: InternalsVisibleTo("ExpressiveAnnotations.Tests")]
[assembly: InternalsVisibleTo("ExpressiveAnnotations.MvcUnobtrusive")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")] // for mocks in unit tests
#else
[assembly: InternalsVisibleTo("ExpressiveAnnotations.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001000ff42e23a8247bbd1c54c6f4428d3e592e505391131b6e28a381dadbb26a88f8407d96afa9993877cd71d3b541470d8ebef5a8dcd780fccf270b1846e14d70b68732f98d8ba9dada92d1f128885fe903011a2185a4be124f5b00618413229f73692638e3c7c22d81cf9365f207a954c6b522183280c07011c325168c148865a4")]
[assembly: InternalsVisibleTo("ExpressiveAnnotations.MvcUnobtrusive, PublicKey=00240000048000009400000006020000002400005253413100040000010001000ff42e23a8247bbd1c54c6f4428d3e592e505391131b6e28a381dadbb26a88f8407d96afa9993877cd71d3b541470d8ebef5a8dcd780fccf270b1846e14d70b68732f98d8ba9dada92d1f128885fe903011a2185a4be124f5b00618413229f73692638e3c7c22d81cf9365f207a954c6b522183280c07011c325168c148865a4")]
// https://github.com/moq/moq4/wiki/Quickstart#advanced-features:
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]
#endif

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
[assembly: AssemblyVersion("2.4.0.0")]
[assembly: AssemblyFileVersion("2.4.0.0")]
