using System.Drawing.Printing;


public class PrinterRepository
{

    public List<string> GetPrinters()
    {

        var printers = new List<string>();

        foreach (string installedPrinter in PrinterSettings.InstalledPrinters)
        {

            printers.Add(installedPrinter);

        }

        return printers;

    }

}
