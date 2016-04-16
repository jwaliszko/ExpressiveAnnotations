using System.Linq;

namespace ExpressiveAnnotations.MvcWebSample.Misc
{
    public static class CustomToolchain
    {
        public static void Register()
        {
            // Toolchain.Instance.Recharge(new CustomFunctionsProvider()); // load complately new set of functions...
            // ...or simply add some new ones to existing toolchain:
            Toolchain.Instance.AddFunction<int[], int>("ArrayLength", array => array.Length);
            Toolchain.Instance.AddFunction<int?, int[], bool>("ArrayContains", (value, array) => value != null && array.Contains((int)value));
        }
    }
}
