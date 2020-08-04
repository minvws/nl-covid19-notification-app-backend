using System;
using System.Security.Principal;

namespace elevated
{
    class Program
    {
        static void Main(string[] args)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            var result = principal.IsInRole(WindowsBuiltInRole.Administrator);

            Console.WriteLine(result);
        }
    }
}
