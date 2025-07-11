using System;
using System.Collections.Generic;

namespace SalidaAutomaticaQR.Models;

public partial class Boletum
{
    public int NumTicket { get; set; }

    public int NumBoleta { get; set; }

    public int? Monto { get; set; }

    public DateOnly Fechaentrada { get; set; }

    public TimeOnly Horaentrada { get; set; }

    public DateOnly? Fechapago { get; set; }

    public TimeOnly? Horapago { get; set; }

    public DateOnly? Fechasalida { get; set; }

    public TimeOnly? Horasalida { get; set; }

    public string? Patente { get; set; }

    public int CodTurno { get; set; }

    public byte CodUsuario { get; set; }

    public byte CodTipoUsuario { get; set; }

    public int NumCaja { get; set; }

    public string NombreAcceso { get; set; } = null!;

    public int? Estado { get; set; }

    public int? TipoLiberado { get; set; }

    public int? IncidenceId { get; set; }

    public int? IncidenceIdSalida { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public DateTime? FechaEdicion { get; set; }
}
