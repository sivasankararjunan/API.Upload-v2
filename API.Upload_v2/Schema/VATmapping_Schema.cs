namespace FileUploadService.Schema
{
    public class VATMapping
    {
        [Required]
        public string[] DoNotHandleVAT { get; set; }
    }
}