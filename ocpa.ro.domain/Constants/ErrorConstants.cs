namespace ocpa.ro.domain.Constants;

public static class AuthenticationErrors
{
    public const string NoToken = "ERR_NO_TOKEN";
    public const string BadCredentials = "ERR_BAD_CREDENTIALS";
    public const string AccountDisabled = "ERR_ACCOUNT_DISABLED";
    public const string BadOtp = "ERR_BAD_OTP";
}

public static class AccessManagementServiceErrors
{
    public const string CannotEditBuiltinApp = "ERR_EDIT_BUILT_IN_APP";
    public const string CannotDeleteBuiltinApp = "ERR_DELETE_BUILTIN_APP";
    public const string CannotDeleteAppMenuAssoc = "ERR_DELETE_APP_MENU_ASSOCIATIONS";
    public const string CannotDeleteAppUserAssoc = "ERR_DELETE_APP_USER_ASSOCIATIONS";
    public const string CannotEditBuiltinMenu = "ERR_EDIT_BUILTIN_MENU";
    public const string CannotDeleteBultinMenu = "ERR_DELETE_BUILTIN_MENU";
    public const string CannotDeleteMenuAppAssoc = "ERR_DELETE_MENU_APP_ASSOCIATIONS";
    public const string FailSaveAppsUser = "ERR_FAIL_SAVE_APPS_FOR_USER";
    public const string FailDeleteAppsUser = "ERR_FAIL_DELETE_APPS_FOR_USER";
    public const string FailClearAppsUser = "ERR_FAIL_CLEAR_APPS_FOR_USER";
    public const string AppNotFound = "ERR_APP_NOT_FOUND";
    public const string MenuNotFound = "ERR_MENU_NOT_FOUND";
    public const string UserNotFound = "ERR_USER_NOT_FOUND";
}

public static class GeographyServiceErrors
{
    public const string CannotDeleteDefaultCity = "ERR_DELETE_DEFAULT_CITY";
}