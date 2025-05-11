using System.Xml;
using Bitwarden.Models.Enums;
using PuffinDom.Infrastructure;

namespace Bitwarden;

public class BitwardenConstants
{
    public const string RelativePathToRootDirectory = "..";
    private const string TestArtefacts = "TestArtefacts";

    public static string RelativePathToAllureResultsFolder =>
        Path.Combine(RelativePathToResultsFolder, "allure-results");

    private const string UITestsRunSettingsFilename = "uitests.runsettings";
    public const string ProductName = "bitwarden";
    public const string AppCrashed = "App crashed";
    public const string BundleId = "com.x8bit.bitwarden";
    public const Host DefaultHost = Host.BitwardenCom;

    public const string GmsModuleWasUpdatedOnAndroidSoTheAppWasClosed =
        "com.google.android.gms was updated and OS closed the app";

    public const string BugreportFileName = "bugreport.zip";
    public const int MinFreeSpaceOnHardDriveMb = 2000;
    public const string NotEnoughFreeSpaceOnHardDrive = "Not enough free space on hard drive";

    // ReSharper disable once InconsistentNaming
    public const string iOSTurnedOff = "iOS is turned off";
    public const string AndroidTurnedOff = "Android is turned off";
    public const string TestInDevelopment = "Test in development. ";
    public const string TestFullDisplayDateFormat = "dddd, MMMM dd, yyyy h:mm:ss tt";

    public static string BitwardenSamplesPath => Path.Combine(
        CoreConstants.GetBaseProjectPath());
    public static string BitwardenTestArtefactsPath => Path.Combine($"{BitwardenSamplesPath}", TestArtefacts);

    public static string RelativePathToResultsFolder
    {
        get
        {
            var xmlDocumentAfterRunningTests = new XmlDocument();
            xmlDocumentAfterRunningTests.Load(
                Path.Combine(
                    BitwardenSamplesPath, 
                    TestArtefacts, 
                    UITestsRunSettingsFilename));
            var folderOfTestsResultsXml =
                xmlDocumentAfterRunningTests.SelectSingleNode("//RunSettings/RunConfiguration/ResultsDirectory")!
                    .InnerText;

            return Path.Combine($"{BitwardenTestArtefactsPath}", folderOfTestsResultsXml);
        }
    }
    
        
}