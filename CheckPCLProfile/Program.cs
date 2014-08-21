using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace CheckPCLProfile
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("CheckPLCProfile [dllname]");
                Console.WriteLine("Found proifle name ex. 'Profile7'");
                return;
            }
            // var path = @"D:\git\CheckPCLProfile\LibFsharpPCL\bin\Debug\LibFsharpPCL.dll";
            var path = args[0];

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var pname = PCLProfile.GetProfile(path);
            Console.WriteLine("profile: {0}", pname);
            var lst = PCLProfile.GetSupportedFrameworks(pname);
            foreach (var it in lst)
            {
                Console.WriteLine("- {0}", it);
            }
        }

        /// <summary>
        /// Dynamic loading when run short dll.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var dir  = System.IO.Path.GetDirectoryName( args.RequestingAssembly.Location );
            var name = new System.Reflection.AssemblyName( args.Name ).Name ;
            var path = string.Format("{0}\\{1}.dll", dir, name);
            var asm = Assembly.LoadFile(path);
            return asm;
        }
    }

    /// <summary>
    /// Find profile name 
    /// ex. 'Profile7' 
    /// </summary>
    public class PCLProfile
    {
        public static string GetProfile(string path)
        {
            var asm = System.Reflection.Assembly.LoadFile(path);
            var attr = asm.CustomAttributes.First(
                    a => a.AttributeType == typeof(System.Runtime.Versioning.TargetFrameworkAttribute));
            var va = attr.ConstructorArguments[0];
            var pname = va.Value as string;
            // ex. ".NETPortable,Version=v4.5,Profile=Profile7"
            if (pname.StartsWith(".NETPortable") == false )
            {
                return "error";
            } 
            int i = pname.LastIndexOf("=");
            return pname.Substring(i + 1);
        }

        protected static List<Tuple<string, string>> profiles = null;

        /// <summary>
        /// プロファイル一覧を取得
        /// </summary>
        public static void InitSupportedFrameworks()
        {
            var dir1 = @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable";
            var dir2 = @"C:\Program Files\Reference Assemblies\Microsoft\Framework\.NETPortable";

            var files = new List<string>();
            if ( System.IO.Directory.Exists( dir1 ) == true ) 
               files.AddRange( System.IO.Directory.EnumerateFiles(dir1, "*.xml", System.IO.SearchOption.AllDirectories));
            if ( System.IO.Directory.Exists(dir2 ) == true )
               files.AddRange( System.IO.Directory.EnumerateFiles(dir2, "*.xml", System.IO.SearchOption.AllDirectories));

            // C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETPortable\v4.5\Profile\Profile7\SupportedFrameworks\.NET Framework 4.5.xml
            var lst = new List<Tuple<string, string>>();
            foreach (var it in files)
            {
                if ( it.IndexOf("SupportedFrameworks") > 0 ) {
                    var ws = it.Split(new char[] { '\\' });
                    var framework = ws[ws.Length - 1].Replace(".xml", "");
                    var profile = ws[ws.Length - 3];
                    lst.Add(new Tuple<string, string>(profile, framework));
                }
            }
            PCLProfile.profiles = lst;
        }
        /// <summary>
        /// 対応フレームワークを取得
        /// </summary>
        /// <param name="pname"></param>
        public static List<string> GetSupportedFrameworks( string pname )
        {
            if (PCLProfile.profiles == null)
                PCLProfile.InitSupportedFrameworks();

            var lst = new List<string>();

            foreach (var it in PCLProfile.profiles)
            {
                if ( it.Item1 == pname )
                    lst.Add(it.Item2);
            }
            return lst;
        }
    }
}
