namespace AutoTraderApp.Core.Constants;

public static class Messages
{
    public static class Auth
    {
        public const string UserNotFound = "User not found";
        public const string PasswordError = "Password is incorrect";
        public const string SuccessfulLogin = "Successfully logged in";
        public const string UserAlreadyExists = "User already exists";
        public const string AccessTokenCreated = "Access token created";
    }

    public static class General
    {
        public const string DataNotFound = "Data not found";
        public const string ValidationError = "Validation error";
        public const string SystemError = "System error occurred";
    }
}