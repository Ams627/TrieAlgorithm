using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace TrieAlgorithm
{

    class PathDir
    {
        public string Directory { get; set; }
        public string Sha1Sum { get; set; }
        public string Prefix { get; set; }
        public bool isSystem { get; set; }
    }
    class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var sysPaths0 = RegLib.GetValues(@"HKLM\SYSTEM\CURRENTCONTROLSET\CONTROL\SESSION MANAGER\ENVIRONMENT",
                    RegistryView.Registry64,
                    RegistryValueOptions.DoNotExpandEnvironmentNames)
                    .Where(x => x.key.Equals("Path", StringComparison.OrdinalIgnoreCase)).First().value;

                var sysPaths = sysPaths0
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => new PathDir { Directory = x, isSystem = true, Sha1Sum = Sha1SumString($"{x}S") });

                var userPaths0 = RegLib.GetValues(@"HKCU\Environment",
                                                    RegistryView.Registry64,
                                                    RegistryValueOptions.DoNotExpandEnvironmentNames)
                                                    .Where(x => x.key.Equals("Path", StringComparison.OrdinalIgnoreCase))
                                                    .First().value;

                var userPaths = userPaths0
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => new PathDir { Directory = x, isSystem = false, Sha1Sum = Sha1SumString($"{x}U") });

                var paths = sysPaths.Concat(userPaths);

                var group = paths.ToLookup(x => x.Directory);
                foreach (var dup in group.Where(x=>x.Count() > 1))
                {
                    Console.WriteLine($"Directory {dup.Key} occurs twice in system Path");
                }


                var t1 = new ShaTrie();
                var prefixes = t1.GetShortestPrefix(paths.Select(x=>x.Sha1Sum));
                paths = paths.Zip(prefixes, (a, b) => new PathDir { Directory = a.Directory, isSystem = a.isSystem, Sha1Sum = a.Sha1Sum, Prefix = b });

                foreach (var p in paths)
                {
                    Console.WriteLine($"{p.Prefix,7} {p.Directory}");
                }
            }
            catch (Exception ex)
            {
                var fullname = System.Reflection.Assembly.GetEntryAssembly().Location;
                var progname = Path.GetFileNameWithoutExtension(fullname);
                Console.Error.WriteLine($"{progname} Error: {ex.Message}");
            }

        }

        private static string Sha1SumString(string str)
        {
            var data = Encoding.ASCII.GetBytes(str);
            var hashData = new SHA1Managed().ComputeHash(data);
            return string.Concat(hashData.Select(x => $"{x:x2}"));
        }
    }
}
