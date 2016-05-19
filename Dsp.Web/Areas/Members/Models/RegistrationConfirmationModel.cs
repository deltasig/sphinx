namespace Dsp.Web.Areas.Members.Models
{
    public class RegistrationConfirmationModel
    {
        public string UserName { get; set; }
        public string TemporaryPassword { get; set; }
        public string CallbackUrl { get; set; }
    }
}