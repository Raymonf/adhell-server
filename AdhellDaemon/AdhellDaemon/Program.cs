using JsonFx.Json;
using Nancy;
using Nancy.Hosting.Self;
using Nancy.Responses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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
        public static string BaseDirectory = @"/root/adhell";

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

        public static string smaliHack = @".class public final Lcom/fiendfyre/AdHell2/BuildConfig;
.super Ljava/lang/Object;
.source ""BuildConfig.java""


# static fields
.field public static final APPLICATION_ID:Ljava/lang/String; = ""[!!!!!][0]""

.field public static final BUILD_TYPE:Ljava/lang/String; = ""release""

.field public static final DEBUG:Z = false

.field public static final FLAVOR:Ljava/lang/String; = """"

.field public static final VERSION_CODE:I = 0x1

.field public static final VERSION_NAME:Ljava/lang/String; = ""1.0.0""


# direct methods
.method public constructor <init>()V
    .locals 0

    .prologue
    .line 6
    invoke-direct {p0}, Ljava/lang/Object;-><init>()V

    return-void
.end method
";

        public static string AndroidManifest = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""no""?><manifest xmlns:android=""http://schemas.android.com/apk/res/android"" package=""[!!!!!][0]"">
    <uses-permission android:name=""android.permission.sec.MDM_FIREWALL""/>
    <uses-permission android:name=""android.permission.sec.MDM_APP_MGMT""/>
    <uses-permission android:name=""android.permission.sec.MDM_APP_PERMISSION_MGMT""/>
    <uses-permission android:name=""com.samsung.android.providers.context.permission.WRITE_USE_APP_FEATURE_SURVEY""/>
    <uses-permission android:name=""android.permission.ACCESS_NETWORK_STATE""/>
    <uses-permission android:name=""android.permission.INTERNET""/>
    <uses-permission android:name=""android.permission.RECEIVE_BOOT_COMPLETED""/>
    <uses-permission android:name=""android.permission.WAKE_LOCK""/>
    <uses-permission android:name=""android.permission.WRITE_EXTERNAL_STORAGE""/>
    <uses-permission android:name=""android.permission.READ_EXTERNAL_STORAGE""/>
    <uses-permission android:name=""com.android.vending.BILLING""/>
    <application android:allowBackup=""true"" android:icon=""@mipmap/ic_launcher"" android:label=""@string/app_name"" android:name=""com.fiendfyre.AdHell2.App"" android:supportsRtl=""true"" android:theme=""@style/AppTheme"">
        <activity android:name=""com.fiendfyre.AdHell2.MainActivity"" android:windowSoftInputMode=""adjustPan"">
            <intent-filter>
                <action android:name=""android.intent.action.MAIN""/>
                <category android:name=""android.intent.category.LAUNCHER""/>
            </intent-filter>
        </activity>
        <service android:exported=""false"" android:name=""com.fiendfyre.AdHell2.service.BlockedDomainService""/>
        <service android:exported=""false"" android:name=""com.fiendfyre.AdHell2.service.HeartbeatIntentService""/>
        <receiver android:description=""@string/app_name"" android:label=""@string/app_name"" android:name=""com.fiendfyre.AdHell2.receiver.CustomDeviceAdminReceiver"" android:permission=""android.permission.BIND_DEVICE_ADMIN"">
            <meta-data android:name=""android.app.device_admin"" android:resource=""@xml/enterprise_device_admin""/>
            <intent-filter>
                <action android:name=""android.app.action.DEVICE_ADMIN_ENABLED""/>
            </intent-filter>
        </receiver>
        <receiver android:name=""com.fiendfyre.AdHell2.receiver.BlockedDomainAlarmReceiver"" android:process="":remote""/>
        <receiver android:name=""com.fiendfyre.AdHell2.receiver.BootBroadcastReceiver"">
            <intent-filter>
                <action android:name=""android.intent.action.BOOT_COMPLETED""/>
            </intent-filter>
        </receiver>
        <receiver android:name=""com.fiendfyre.AdHell2.receiver.AdhellDownloadBroadcastReceiver"">
            <intent-filter>
                <action android:name=""android.intent.action.DOWNLOAD_COMPLETE""/>
            </intent-filter>
        </receiver>
        <receiver android:name=""com.fiendfyre.AdHell2.receiver.ApplicationsListChangedReceiver"">
            <intent-filter>
                <action android:name=""android.intent.action.PACKAGE_ADDED""/>
                <action android:name=""android.intent.action.PACKAGE_REMOVED""/>
                <data android:scheme=""package""/>
            </intent-filter>
        </receiver>
        <meta-data android:name=""android.support.VERSION"" android:value=""26.0.1""/>
        <provider android:authorities=""[!!!!!][0].lifecycle-trojan"" android:exported=""false"" android:multiprocess=""true"" android:name=""android.arch.lifecycle.LifecycleRuntimeTrojanProvider""/>
        <activity android:name=""com.android.billingclient.util.ProxyBillingActivity"" android:theme=""@android:style/Theme.Translucent.NoTitleBar""/>
    </application>
</manifest>";

        public static string Errors = "";
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
                    try
                    {
                        // Modify apktool.yml
                        var appId = "com." + GetUniqueKey(new Random().Next(5, 11)) + "." + GetUniqueKey(new Random().Next(6, 12));

                        var newYml = string.Format(Settings.apkToolYml, appId);
                        File.WriteAllText(Settings.BaseDirectory + "/Adhell/apktool.yml", newYml);

                        var newSmali = Settings.smaliHack.Replace("[!!!!!][0]", appId);
                        File.WriteAllText(Settings.BaseDirectory + "/Adhell/smali/com/fiendfyre/AdHell2/BuildConfig.smali", newSmali);

                        var androidManifest = Settings.AndroidManifest.Replace("[!!!!!][0]", appId);
                        File.WriteAllText(Settings.BaseDirectory + "/Adhell/AndroidManifest.xml", androidManifest);

                        var appSettingsFragPath = Settings.BaseDirectory + "/Adhell/smali/com/fiendfyre/AdHell2/fragments/AppSettingsFragment.smali";

                        if(!File.Exists(appSettingsFragPath + ".bak"))
                        {
                            // Make it!
                            File.Copy(appSettingsFragPath, appSettingsFragPath + ".bak", true);
                        }

                        var appSettingsFrag = File.ReadAllText(appSettingsFragPath + ".bak").Replace("com.fiendfyre.AdHell5", appId);

                        File.WriteAllText(appSettingsFragPath, appSettingsFrag);

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
                    }
                    catch (Exception ex)
                    {
                        Settings.Errors += "<pre>" + ex + "</pre><br>";
                    }
                });

                return "working";
            };

            Get["/errors"] = parameters =>
            {
                return Settings.Errors;
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

            Get["/grablatest"] = parameters =>
            {
                if (Settings.Building) return "There's a build in progress right now, hold on.";

                Settings.Building = true;

                try
                {
                    // https://api.github.com/repos/MilanParikh/Adhell2/releases/latest
                    var reader = new JsonReader();
                    var wc = new WebClient();

                    dynamic output = null;

                    try
                    {
                        wc.Headers.Add("User-Agent", "AdhellDaemon/1.0");
                        output = reader.Read(wc.DownloadString("https://api.github.com/repos/MilanParikh/Adhell2/releases/latest"));
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Unable to get the latest version data. " + ex);
                    }

                    var link = (string) output.assets[0].browser_download_url;

                    var outPath = Settings.BaseDirectory + "/Adhell.apk";

                    if (File.Exists(outPath))
                    {
                        File.Delete(outPath);
                    }

                    try
                    {
                        wc.Headers.Add("User-Agent", "AdhellDaemon/1.0");
                        wc.DownloadFile(link, outPath);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Unable to download the latest file. " + ex);
                    }

                if (Directory.Exists(Settings.BaseDirectory + "/Adhell"))
                    {
                        Directory.Delete(Settings.BaseDirectory + "/Adhell", true);
                    }

                    var apkToolStartInfo = new ProcessStartInfo();
                    apkToolStartInfo.FileName = Settings.BaseDirectory + "/apktool";
                    apkToolStartInfo.Arguments = "d Adhell.apk";
                    apkToolStartInfo.WorkingDirectory = Settings.BaseDirectory;
                    apkToolStartInfo.CreateNoWindow = true;

                    var proc = Process.Start(apkToolStartInfo);
                    proc.WaitForExit();

                    Settings.Building = false;

                    return "Seems to be OK...? Try to rebuild!";
                }
                catch(Exception ex)
                {
                    Settings.Building = false;
                    return "Something happened. RIP :(<br><pre>" + ex + "</pre>";
                }

                return "done";
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
