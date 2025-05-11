using System.Diagnostics.CodeAnalysis;
using System.Security.Authentication;
using Bitwarden.AppModels.Views.DialogsView;
using Bitwarden.AppModels.Views.DialogsView.System;
using Bitwarden.AppModels.Views.DialogsView.System.Android;

namespace Bitwarden.AppModels.Views;

public static class Dialogs
{
    public static AppKeepsStoppingDialog AndroidAppKeepsStopping => new();
    public static AllowNotificationsDialog AllowNotifications => new();
    public static LogOutDialog LogOut => new();
    public static ErrorDialog Error => new();
    public static InvalidCredentialErrorDialog InvalidCredentialError => new();
    public static LoggingInOnDialog LoggingInOn => new();
}