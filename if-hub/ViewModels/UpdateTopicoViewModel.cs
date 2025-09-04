using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class UpdateTopicViewModel
{
    [Required]
    [StringLength(100, MinimumLength = 5)]
    public string Titulo { get; set; }

    [Required]
    [StringLength(10000, MinimumLength = 10)]
    public string Conteudo { get; set; }

    public List<IFormFile>? Imagens { get; set; }

    public List<IFormFile>? OutrosAnexos { get; set; }
}