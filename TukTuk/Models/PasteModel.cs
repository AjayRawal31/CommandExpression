using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace TukTuk.Models
{
    public class PasteModel
    {
        [Required(ErrorMessage ="Textarea can't be empty")]
        public string Paste { get; set; }

        [Required(ErrorMessage ="Please enter your commands")]        
        public string Command { get; set; }



    }
}