namespace FitCheckWebApp.ViewModels
{
    public class ChangePasswordViewModel
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class VerifyCodeViewModel
    {
        public string Code { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}