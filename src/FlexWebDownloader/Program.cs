using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace SiteRippers
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine($"Gerbuik: {Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName)} [gebruikersnaam] [wachtwoord] [doelmap]");
                return 1;
            }
            string user = args[0];
            string password = args[1];
            string path = args[2];
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var ripper = new Flexweb(user, password);
            try
            {
                await ripper.RipTo(path, new DateTime(2000, 1, 1), DateTime.Today);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 2;
            }
            return 0;
        }
    }
}