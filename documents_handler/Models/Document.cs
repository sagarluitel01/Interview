using System.ComponentModel.DataAnnotations;

namespace documents_handler.Models;

public class Document{

    public string Name {get; set;}
    public string Path {get; set;}
    public string Category { get; set; }

}