using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Safari;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Nodes;

namespace Ldd.YandexMusicAuthorization;

public static class YMAuthorization
{
    public static readonly string AuthPath = "https://oauth.yandex.ru/authorize?response_type=token&client_id=23cabbbdc6cd418abb4b39c32c41195d";
    private static readonly string _roamingFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
    private static readonly string _localFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

    public static bool TryGetAuthToken(SupportedBrowser selectedBrowser, [MaybeNullWhen(false)] out string authToken, TimeSpan? timeout = null)
    {
        TimeSpan innerTimeout = timeout ?? TimeSpan.FromMinutes(10);
        authToken = null;
        IWebDriver driver;
        switch (selectedBrowser)
        {
            case SupportedBrowser.Chrome:
                try
                {
                    ChromeOptions chromeOptions = GetDriverOptions<ChromeOptions>();
                    string chromeSettingPath = Path.Combine(_localFolderPath, "Google", "Chrome", "User Data");
                    DirectoryInfo profilesdir = Directory.CreateDirectory(chromeSettingPath);
                    string userDataArg = "user-data-dir=" + chromeSettingPath;
                    chromeOptions.AddArgument(userDataArg);
                    DirectoryInfo? profileDir = profilesdir.GetDirectories().Where(d => d.Name.StartsWith("profile", StringComparison.OrdinalIgnoreCase)).OrderByDescending(d => d.LastAccessTime).FirstOrDefault();
                    if (profileDir is not null)
                    {
                        string profileDataArg = "--profile-directory=" + profileDir.Name;
                        //string profileDataArg = "--profile-directory=" + Path.Combine(chromeSettingPath, profileDir.Name);
                        chromeOptions.AddArgument(profileDataArg);
                    }

                    chromeOptions.AddArguments("--headless=new", "--remote-debugging-port=9292", "--disable-dev-shm-usage", "--no-sandbox"); // Bypass OS security model
                    driver = new ChromeDriver(chromeOptions);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                    ChromeOptions co = GetDriverOptions<ChromeOptions>();
                    driver = new ChromeDriver(co);
                }
                break;
            case SupportedBrowser.Firefox:
                // Firefox logs not supported
                //FirefoxDriverService fs = FirefoxDriverService.CreateDefaultService("C:\\Program Files\\Mozilla Firefox\\firefox.exe");
                //FirefoxDriverService fs = FirefoxDriverService.CreateDefaultService();
                //fs.ConnectToRunningBrowser = true;                
                try
                {
                    FirefoxOptions firefoxOptions = GetDriverOptions<FirefoxOptions>();
                    string firefoxUserPath = Path.Combine(_roamingFolderPath, "Mozilla", "Firefox", "Profiles");
                    DirectoryInfo dir = Directory.CreateDirectory(firefoxUserPath);
                    //DirectoryInfo? lastUsedDir = dir.GetDirectories().OrderByDescending(d => d.LastAccessTime).FirstOrDefault();
                    DirectoryInfo? lastUsedDir = dir.GetDirectories().FirstOrDefault();
                    if (lastUsedDir is not null)
                    {
                        string profilePath = Path.Combine(firefoxUserPath, lastUsedDir.Name);
                        //firefoxOptions.AddArguments("-profile", profilePath);
                        firefoxOptions.Profile = new FirefoxProfile(profilePath);
                    }

                    driver = new FirefoxDriver(firefoxOptions);
                    //driver = new FirefoxDriver(fs, firefoxOptions);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.ToString());
                    FirefoxOptions fo = GetDriverOptions<FirefoxOptions>();
                    driver = new FirefoxDriver(fo);
                    //driver = new FirefoxDriver(fs, fo);
                }

                break;
            case SupportedBrowser.Edge:
                EdgeOptions edgeOptions = GetDriverOptions<EdgeOptions>();
                driver = new EdgeDriver(edgeOptions);
                break;
            case SupportedBrowser.Safari:
                SafariOptions safariOptions = GetDriverOptions<SafariOptions>();
                driver = new SafariDriver(safariOptions);
                break;
            case SupportedBrowser.IE:
                InternetExplorerOptions ieOptions = GetDriverOptions<InternetExplorerOptions>();
                driver = new InternetExplorerDriver(ieOptions);
                break;
            default:
                throw new ArgumentException("Selected browser not supported", nameof(selectedBrowser));
        };

        driver.Navigate().GoToUrl(AuthPath);
        driver.Manage().Window.Maximize();
        Stopwatch stopwatch = new();
        stopwatch.Start();
        while (string.IsNullOrEmpty(authToken)
            || stopwatch.ElapsedMilliseconds < innerTimeout.TotalMilliseconds)
        {
            try
            {
                ReadOnlyCollection<LogEntry> logs = driver.Manage().Logs.GetLog(LogType.Performance);
                foreach (LogEntry log in logs)
                {
                    if (TryParseAuthToken(log, out authToken))
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }

            Task.Delay(TimeSpan.FromMilliseconds(150));
        }

        if (stopwatch.ElapsedMilliseconds >= innerTimeout.TotalMilliseconds)
        {
            driver.Close();
            throw new TimeoutException("Receiving auth token timeout");
        }

        driver.Close();
        return !string.IsNullOrEmpty(authToken);
    }

    private static T GetDriverOptions<T>()
        where T : DriverOptions, new()
    {
        T result = new();
        result.SetLoggingPreference(LogType.Performance, LogLevel.All);
        return result;
    }

    private static bool TryParseAuthToken(LogEntry logEntry, [MaybeNullWhen(false)] out string authToken)
    {
        authToken = null;
        JsonNode? logMessage = JsonNode.Parse(logEntry.Message);
        if (logMessage is null
            || !logMessage.AsObject().TryGetPropertyValue("message", out JsonNode? message)
            || message is null
            || !message.AsObject().TryGetPropertyValue("params", out JsonNode? messageParams)
            || messageParams is null)
        {
            return false;
        }

        if (!messageParams.AsObject().TryGetPropertyValue("frame", out JsonNode? urlFragmentParent)
            && !messageParams.AsObject().TryGetPropertyValue("request", out urlFragmentParent))
        {
            return false;
        }

        if (urlFragmentParent is not null
            && urlFragmentParent.AsObject().TryGetPropertyValue("urlFragment", out JsonNode? urlFragment)
            && urlFragment is not null)
        {
            authToken = urlFragment.GetValue<string>();
        }

        return !string.IsNullOrEmpty(authToken);
    }
}
