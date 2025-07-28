using Transbank.POSAutoservicio;
using Transbank.Responses.CommonResponses;
using Microsoft.Extensions.Configuration;
using System.Drawing.Printing;
using System.Text.Json;
using SalidaAutomaticaQR.Models;
using Microsoft.EntityFrameworkCore;
using System.Text;
using NAudio.Wave;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Globalization;
using TestingTransbank.Services;
using TestingTransbank.Helper;
using TestingTransbank.Managers;
using TestingTransbank.Helpers;

public enum ReceiptType
{

    Sale,

    Close

}

public class App
{

    private PrinterManager _printerManager;

    private DatabaseManager _databaseManager;

    private POSManager _posManager;

    public App(PrinterManager printerManager, DatabaseManager databaseManager, CAFManager cafService, VariableManager variableManager, POSManager posManager)
    {

        _printerManager = printerManager;

        _databaseManager = databaseManager;

        _posManager = posManager;
    
    }

    public async Task Run()
    {

        _printerManager.SelectPrinter();

        _printerManager.SelectPort();

        await _databaseManager.SelectDatabase();

        await _posManager.SelectOperationLoop();

    }

}