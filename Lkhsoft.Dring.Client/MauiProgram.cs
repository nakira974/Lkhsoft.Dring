#region

using CommunityToolkit.Maui;
using Lkhsoft.Dring.Client.Services.Authentication;
using Lkhsoft.Dring.Client.Services.Multimedia;
using Lkhsoft.Dring.Client.Services.SIP;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;

#endregion

namespace Lkhsoft.Dring.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureSyncfusionToolkit()
            .ConfigureMauiHandlers(handlers => { })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
            });

#if DEBUG
        builder.Logging.AddDebug();
        builder.Services.AddLogging(configure => configure.AddDebug());
#endif

        #region PAGE MODELS

        builder.Services.AddSingleton<MainPageModel>();
        builder.Services.AddSingleton<ProjectListPageModel>();
        builder.Services.AddSingleton<ManageMetaPageModel>();
        builder.Services.AddSingleton<AudioStreamPageModel>();
        builder.Services.AddSingletonWithShellRoute<LoginPage, LoginPageModel>("login");
        builder.Services.AddTransientWithShellRoute<ProjectDetailPage, ProjectDetailPageModel>("project");
        builder.Services.AddTransientWithShellRoute<TaskDetailPage, TaskDetailPageModel>("task");

        #endregion

        #region SERVICES

        builder.Services.AddSingleton<ProjectRepository>();
        builder.Services.AddSingleton<TaskRepository>();
        builder.Services.AddSingleton<CategoryRepository>();
        builder.Services.AddSingleton<TagRepository>();
        builder.Services.AddSingleton<SeedDataService>();
        builder.Services.AddSingleton<ModalErrorHandler>();

        builder.Services.AddSingleton<INativeLibrariesImports, NativeLibrariesImports>();
        builder.Services.AddScoped<IAudioService, AudioService>();
        builder.Services.AddScoped<IVideoService, VideoService>();
        
        builder.Services.AddSingleton<ISessionService, SessionService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();
        builder.Services.AddSingleton<SipServer>();

        builder.Services.AddHostedService<SipBackgroundService>();

        #endregion


        return builder.Build();
    }
}