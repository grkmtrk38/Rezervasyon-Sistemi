using System.ComponentModel.DataAnnotations;

namespace RezervasyonSistemi.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [Display(Name = "Birim / Müdürlük")]
        public string? Birim { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        [Compare("Password", ErrorMessage = "Şifreler birbiriyle uyuşmuyor.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
