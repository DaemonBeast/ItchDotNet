using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Avalonia.Themes.Fluent;
using Avalonia.Threading;
using ItchDotNet.Sandbox.Utilities;
using Xilium.CefGlue.Avalonia;
using Xilium.CefGlue.Common.Events;

namespace ItchDotNet.Sandbox;

public partial class App : Application
{
    public override void Initialize()
    {
        RequestedThemeVariant = ThemeVariant.Default;

        Styles.Add(new FluentTheme());
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.ShutdownMode = ShutdownMode.OnExplicitShutdown;
        }

        _ = Task.Run(AfterStartup)
            .ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Console.WriteLine(t.Exception);
                }

                Dispatcher.UIThread.Post(() => ((IClassicDesktopStyleApplicationLifetime) ApplicationLifetime!).Shutdown());
            });

        base.OnFrameworkInitializationCompleted();
    }

    private async Task AfterStartup()
    {
        var client = ItchClient.CreateWithDefaults();
        // client.ApiKey = "";

        /*await Console.Out.WriteAsync("Username: ");
        var username = (await Console.In.ReadLineAsync())!;

        await Console.Out.WriteAsync("Password: ");
        var password = ConsoleUtilities.ReadPassword();

        var response = await client.Login.WithPassword(
            username,
            password,
            HandleRecaptchaNeeded);*/

        // Console.WriteLine(response.Key.Key);

        /*await foreach (var downloadKey in client.Profile.OwnedKeys())
        {
            Console.WriteLine(
                JsonSerializer.Serialize(downloadKey, new JsonSerializerOptions { WriteIndented = true }));
        }*/

        /*var oldestBuild = client.Uploads.Builds(1047911).ToBlockingEnumerable().Last();
        Console.WriteLine(JsonSerializer.Serialize(oldestBuild, new JsonSerializerOptions { WriteIndented = true }));*/

        await foreach (var build in client.Uploads.Builds(1047911))
        {
            Console.WriteLine(JsonSerializer.Serialize(build, new JsonSerializerOptions { WriteIndented = true }));
        }

        Dispatcher.UIThread.Post(() => ((IClassicDesktopStyleApplicationLifetime) ApplicationLifetime!).Shutdown());
    }

    private static async Task<string> HandleRecaptchaNeeded(string recaptchaUrl)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            return await Dispatcher.UIThread.InvokeAsync(() => HandleRecaptchaNeeded(recaptchaUrl));
        }

        var browser = new AvaloniaCefBrowser
        {
            Address = recaptchaUrl
        };

        var captchaHandler = new CaptchaHandler();

        [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
        void HandleJavascriptContextCreated(object sender, JavascriptContextLifetimeEventArgs e)
        {
            const string objectName = "captchaHandler";
            const string methodName = nameof(CaptchaHandler.handle);

            browser.RegisterJavascriptObject(captchaHandler, objectName);
            browser.ExecuteJavaScript($"function onCaptcha(response) {{ {objectName}.{methodName}(response); }}");
        }

        browser.JavascriptContextCreated += HandleJavascriptContextCreated;

        var window = new Window
        {
            Content = browser
        };

        window.Show();

        var captchaResponse = await captchaHandler.Task;

        browser.JavascriptContextCreated -= HandleJavascriptContextCreated;
        browser.Dispose();
        window.Close();

        return captchaResponse;
    }

    private class CaptchaHandler
    {
        public Task<string> Task => _taskCompletionSource.Task;

        private readonly TaskCompletionSource<string> _taskCompletionSource = new();

        // ReSharper disable once InconsistentNaming
        public void handle(string response)
            => _taskCompletionSource.SetResult(response);
    }
}
