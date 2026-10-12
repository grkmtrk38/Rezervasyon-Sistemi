using System.ComponentModel.DataAnnotations;

namespace RezervasyonSistemi.Models.ViewModels
{
    public class PersonelCreateViewModel
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
        [Display(Name = "Sistem Giriş E-postası")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, ErrorMessage = "{0} en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "İlk Giriş Şifresi")]
        public string Password { get; set; } = string.Empty;
    }
}
