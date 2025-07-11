using System;
using System.Collections.Generic;

namespace SalidaAutomaticaQR.Models;

public partial class Parametro
{

    public bool? LecturaPatente { get; set; }

    public string? MsjCajero { get; set; }

    public string? CamEntrada { get; set; }

    public string? CamSalida { get; set; }

    public int? AwaitLecturaPatente { get; set; }

    public string? DirAnpr { get; set; }

    public string? CameraMask { get; set; }

    public string? NomImpresora { get; set; }

    public int? COMRele { get; set; }

    public int? StringRele { get; set; }

    public int? StringRele2 { get; set; }

    public string? CameraMaskSalida { get; set; }


}
