using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace documents_handler.Models
{
    public class RawDocument
    {   
        public string Name { get; set;}
        public string Category { get; set; }
       public IFormFile Doc{ get; set; }
    }
}