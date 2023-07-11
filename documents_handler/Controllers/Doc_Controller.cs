using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using documents_handler.Models;

using System.IO;

namespace documents_handler.Controllers;

public class DocumentController : Controller
{

    // private string _path = System.IO.Directory.GetCurrentDirectory();
    private string cd_Path = System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).FullName;
    
    /// <summary>
    /// Controller which returns Home/Main page that display list of documents
    /// </summary>
    /// <returns> Main View with list of document</returns>
    [HttpGet]
    public IActionResult Main()
    {
        var docList =  GetDocuments();

        return View(docList);
    }

    /// <summary>
    /// Controller that return Page to uplaod document 
    /// </summary>
    /// <returns>Upload View</returns>
    [HttpGet]
    public IActionResult Upload(){
        
        return View();
    }

    /// <summary>
    /// Controller that returns file to display in the browser
    /// </summary>
    /// <param name="path"></param>
    /// <returns>fileStream to display</returns>
    [HttpGet]
    public IActionResult Display(string path){
        Console.WriteLine(" Display  ");
        Console.WriteLine(path);

        string p = path.Replace("\\", "/");
        p = cd_Path + "/" + p;
        Console.WriteLine(p);
        if(System.IO.File.Exists(p)){
            var fileStream = new FileStream(p,
            FileMode.Open,
            FileAccess.Read
            );

            var FsResult = new FileStreamResult(fileStream,  "application/pdf");
            return FsResult;
        }
        return null;
    }

    /// <summary>
    /// Controller to upload file
    /// </summary>
    /// <param name="newDoc"></param>
    /// <returns>Return to Main View</returns>
    [HttpPost]
    public IActionResult Uplaod_Doc(RawDocument newDoc){

        Console.WriteLine( newDoc.Doc.FileName);

        bool res = upload_file(newDoc);

        if(res){
            bool r = update_csv(newDoc);
        }else{
            Console.WriteLine("Failed");
            TempData["Failed"] = "Upload Failed";
            return RedirectToAction("Main");
        }
        TempData["Success"] = "Document uploaded successfully";
        return RedirectToAction("Main");
    }


    /// <summary>
    /// Controller to Delete a file
    /// </summary>
    /// <param name="doc"></param>
    /// <returns>Return to Main View</returns>
    [HttpPost]
    public IActionResult Delete(Document doc){
        Console.WriteLine( doc.Name);
        Console.WriteLine(cd_Path );
        var path = cd_Path + "/" + doc.Path;
        bool res = deleteDoc(path);

        if(res){

            bool r = removeDocfromCSV(doc.Path);
            if(!r){
                TempData["Failed"] = "Unable to delete this document!";
                return RedirectToAction("Main");
            }
            TempData["Success"] = "The doucment deleted successfully";
            return RedirectToAction("Main");
            
        }
        else{
            TempData["Failed"] = "Unable to delete this document!";
            return RedirectToAction("Main");
        }
    }


    /// <summary>
    /// Function to delete a document
    /// </summary>
    /// <param name="p"></param>
    /// <returns>Boolean</returns>
    private bool deleteDoc(string p){

        var path = p.Replace("\\", "/");

         Console.WriteLine( path);

        if(System.IO.File.Exists(path)){
            System.IO.File.Delete(path);
        }else{
            Console.WriteLine("file not found");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Function to remove document info from csv file (list)
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Boolean</returns>
    private bool removeDocfromCSV(string path){
        List<Document> docList = new List<Document>();      

        using(var reader = new StreamReader(cd_Path+"/Documents.csv"))
        {
            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                string[] data = line.Split(',');
                

                if (data.Length == 3)
                {
                    Document doc = new Document
                    {
                        Name = data[0],
                        Path = data[1],
                        Category = data[2]
                    };
                    Console.WriteLine(doc.Name + " " + doc.Path);

                    docList.Add(doc);
                }

            }
        }
        Document doc_to_remove = docList.FirstOrDefault(d => d.Path == path);
        if(doc_to_remove != null){
            docList.Remove(doc_to_remove);

            // Write the updated data back to the CSV file
            using (StreamWriter writer = new StreamWriter(cd_Path+"/Documents.csv"))
            {
                foreach (Document d in docList)
                {
                    writer.WriteLine($"{d.Name},{d.Path},{d.Category}");
                }
            }
            return true;

        }else{
            return false;
        }
    }

    /// <summary>
    /// Function to get all the documents
    /// </summary>
    /// <returns>List of Documents </returns>
    private List<Document> GetDocuments()
    {
        List<Document> docList = new List<Document>();      

        using(var reader = new StreamReader(cd_Path+"/Documents.csv"))
        {
            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                string[] data = line.Split(',');
                if(data[0] == "Name" && data[1] == "Path"){
                    continue;
                }

                if (data.Length == 3)
                {
                    Document doc = new Document
                    {
                        Name = data[0],
                        Path = data[1],
                        Category = data[2]
                    };
                    Console.WriteLine(doc.Name + " " + doc.Path);

                    docList.Add(doc);
                }
            }
        }

        return docList;
    }

    /// <summary>
    /// Uplaod a document in the folder
    /// </summary>
    /// <param name="doc"></param>
    /// <returns>boolen</returns>
    private bool upload_file(RawDocument doc){
        if(doc.Doc.Length > 0){
            string ext = System.IO.Path.GetExtension(doc.Doc.FileName);
            string docPath = cd_Path + "/Docs/" + doc.Category + "/" + doc.Name + ext;
            using (Stream fileStream = new FileStream(docPath, FileMode.Create)) {
                doc.Doc.CopyToAsync(fileStream);
            }
        }
        
        return true;
    }

    /// <summary>
    /// Function to update document info in the csv file 
    /// </summary>
    /// <param name="doc"></param>
    /// <returns>boolena</returns>
    private bool update_csv(RawDocument doc){

        string ext = doc.Doc.FileName.Split(".")[1];
        string category = "";

        if(doc.Category == "SIG"){
            category = "Signatures";
        }
        else if(doc.Category == "SD"){
            category = "Supporting Documents";
        }
        Document newDoc = new Document(){
            Name = doc.Name,
            Path = "Docs\\" + doc.Category +"\\" + doc.Name + "." + ext,
            Category = category
        };

        List<Document> docList = new List<Document>();  
    

        using(var reader = new StreamReader(cd_Path+"/Documents.csv"))
        {
            
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                string[] data = line.Split(',');
                

                if (data.Length == 3)
                {
                    Document mydoc = new Document
                    {
                        Name = data[0],
                        Path = data[1],
                        Category = data[2]
                    };
                    Console.WriteLine(mydoc.Name + " " + mydoc.Path);

                    docList.Add(mydoc);
                }

            }
        }
        docList.Add(newDoc);

        using (StreamWriter writer = new StreamWriter(cd_Path+"/Documents.csv"))
        {
            foreach (Document d in docList)
            {
                writer.WriteLine($"{d.Name},{d.Path},{d.Category}");
            }
        }
        
        return true;
    }
}