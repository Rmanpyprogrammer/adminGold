using System.ComponentModel.DataAnnotations;
using System.Text.Json;
namespace Core.API.Models;

public class Invoice
{
    public int Id { get; set; }
    public long DigikalaId { get; set; } 
    public DateTime CreatedAt { get; private set;} = DateTime.UtcNow;
    public double Amount { get; set;} = 0;
    
    public ICollection<InvoiceProduct> Products { get; set;} = new List<InvoiceProduct>();


}

public class InvoiceProduct
{

    [Key]
    public long DigikalaId { get; set; } 
    public  long DKPC { get; set;} 
    public DateTime CreatedAt { get; private set;} = DateTime.UtcNow;
    public string? desc { get; set;} = string.Empty;

    public double Weight { get; set;} 
    public long OrderId { get; set;} 
    public string? Serial { get; set;} = string.Empty;
    public string? Title { get; set;} = string.Empty;
    public string PayMethod { get; set;} = string.Empty;
    public int InvoiceId { get; set; }

    public Invoice Invoice{ get; set; } = null;

}

public class Packages
{
    
    [Key]
    public string Serial{ get; set; } = string.Empty;
    public long PackageId { get; set; } 
    public long dkpc{ get; set; }

    public DateTime RecievedAt{ get; set; }

    public string? Warehouse{ get; set; }
    public int? Status{ get; set; }



}



//--------------------------------------------------------------------------------
public class Invoice2
{
    public int Id { get; set; }
    public long DigikalaId { get; set; } 
    public DateTime CreatedAt { get; private set;} = DateTime.UtcNow;
    public double Amount { get; set;} = 0;
    
    public ICollection<InvoiceProduct2> Products { get; set;} = new List<InvoiceProduct2>();


}

public class InvoiceProduct2
{

    [Key]
    public long DigikalaId { get; set; } 
    public  long DKPC { get; set;} 
    public DateTime CreatedAt { get; private set;} = DateTime.UtcNow;
    public string? desc { get; set;} = string.Empty;

    public double Weight { get; set;} 
    public long OrderId { get; set;} 
    public string? Serial { get; set;} = string.Empty;
    public string? Title { get; set;} = string.Empty;
    public string PayMethod { get; set;} = string.Empty;
    public int InvoiceId { get; set; }

    public Invoice2 Invoice{ get; set; } = null;

}

public class Packages2
{
    
    [Key]
    public string Serial{ get; set; } = string.Empty;
    public long PackageId { get; set; } 
    public long dkpc{ get; set; }

    public DateTime RecievedAt{ get; set; }

    public string? Warehouse{ get; set; }
    public int? Status{ get; set; }



}