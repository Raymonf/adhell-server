using Nancy;
using Nancy.Hosting.Self;
using Nancy.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AdhellDaemon
{
    public class Settings
    {
        public static DateTime LatestBuild = new DateTime(2017, 1, 1, 0, 0, 0);
        public static int Port = 54236;
        public static bool Building = false;
        public static string BaseDirectory = @"/root/adhell/";

        public static string LastBuildName = "";

        public static string apkToolYml = @"!!brut.androlib.meta.MetaInfo
apkFileName: Adhell.apk
compressionType: false
doNotCompress:
- arsc
isFrameworkApk: false
packageInfo:
  forcedPackageId: '127'
  renameManifestPackage: {0}
sdkInfo:
  minSdkVersion: '16'
  targetSdkVersion: '26'
sharedLibrary: false
unknownFiles:
  publicsuffixes.gz: '0'
  fabric/com.crashlytics.sdk.android.answers.properties: '8'
  fabric/com.crashlytics.sdk.android.beta.properties: '8'
  fabric/com.crashlytics.sdk.android.crashlytics-core.properties: '8'
  fabric/com.crashlytics.sdk.android.crashlytics.properties: '8'
  fabric/io.fabric.sdk.android.fabric.properties: '8'
usesFramework:
  ids:
  - 1
  tag: null
version: 2.2.4
versionInfo:
  versionCode: '1'
  versionName: 1.0.0
";
    }

    public class MainModule : NancyModule
    {
        public static string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[26];
            chars = "abcdefghijklmnopqrstuvwxyz".ToCharArray();

            byte[] data = new byte[1];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }
            StringBuilder result = new StringBuilder(maxSize);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public MainModule()
        {
            Get["/newbuild"] = parameters =>
            {
                if (Settings.Building) return "alreadybuilding";

                if((DateTime.Now - Settings.LatestBuild).TotalMinutes < 2)
                {
                    return "notoldenough";
                }

                Settings.Building = true;
                
                Task.Run(() =>
                {
                    // Modify apktool.yml
                    var newYml = string.Format(Settings.apkToolYml, "com." + GetUniqueKey(new Random().Next(5, 11)) + "." + GetUniqueKey(new Random().Next(6, 12)));
                    
                    File.WriteAllText(Settings.BaseDirectory + "/Adhell/apktool.yml", newYml);

                    var lastBuild = DateTime.Now;
                    var outFilename = "Adhell-" + lastBuild.Month + lastBuild.Day + "-" + lastBuild.Hour + lastBuild.Minute + lastBuild.Second + ".apk";

                    Settings.LatestBuild = lastBuild;

                    // Rebuild apk
                    var apkToolStartInfo = new ProcessStartInfo();
                    apkToolStartInfo.FileName = Settings.BaseDirectory + "/apktool";
                    apkToolStartInfo.Arguments = "b Adhell -o " + outFilename;
                    apkToolStartInfo.WorkingDirectory = Settings.BaseDirectory;
                    apkToolStartInfo.CreateNoWindow = true;

                    var apkTool = Process.Start(apkToolStartInfo);
                    apkTool.WaitForExit();

                    // Resign apk
                    var jarSignStartInfo = new ProcessStartInfo();
                    jarSignStartInfo.FileName = @"jarsigner"; // wtf.
                    jarSignStartInfo.Arguments = "-keystore my.keystore -storepass android -keypass android -sigalg SHA1withRSA -digestalg SHA1 " + outFilename + " app";
                    jarSignStartInfo.WorkingDirectory = Settings.BaseDirectory;
                    jarSignStartInfo.CreateNoWindow = true;

                    var jarSign = Process.Start(jarSignStartInfo);
                    jarSign.WaitForExit();

                    try
                    {
                        File.Delete(Settings.BaseDirectory + "/" + Settings.LastBuildName);
                    }
                    catch { }

                    Settings.Building = false;
                    Settings.LastBuildName = outFilename;
                });

                return "working";
            };

            Get["/getlatest"] = parameters =>
            {
                var file = new FileStream(Settings.BaseDirectory + "/" + Settings.LastBuildName, FileMode.Open);
                string fileName = Settings.LastBuildName;

                var response = new StreamResponse(() => file, MimeTypes.GetMimeType(fileName));
                return response.AsAttachment(fileName);
            };

            Get["/isbuilding"] = parameters =>
            {
                return Settings.Building ? "true" : "false";
            };

            After += ctx =>
            {
                if (ctx.Response.ContentType == "text/html")
                {
                    ctx.Response.ContentType = "text/html; charset=utf-8";
                }
            };
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("Starting...");
            
            var url = "http://127.0.0.1:" + Settings.Port;

            using (var host = new NancyHost(new Uri(url)))
            {
                host.Start();

                Console.WriteLine("Started at {0}", url);

                while (true)
                {
                    Console.ReadLine();
                    Console.ReadKey();
                }
            }
        }
    }
}
