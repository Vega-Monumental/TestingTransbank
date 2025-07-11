using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace SalidaAutomaticaQR.Models;

public partial class EstacionamientoContext : DbContext
{

    private readonly string _connectionString;

    public EstacionamientoContext()
    {

        // Configuración
        var config = new ConfigurationBuilder()

            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)

            .Build();

        var conexionSeleccionada = config["ConexionSeleccionada"];

        // Obtener la sección "Conexiones"
        var conexiones = config.GetSection("Conexiones");

        // Obtener la cadena de conexión correspondiente
        var cadenaConexion = conexiones[conexionSeleccionada];

        _connectionString = cadenaConexion;

    }

    public EstacionamientoContext(DbContextOptions<EstacionamientoContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Boletum> Boleta { get; set; }

    public virtual DbSet<Parametro> Parametros { get; set; }

    public virtual DbSet<Variable> Variables { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

        if (!optionsBuilder.IsConfigured)
        {

            optionsBuilder.UseSqlServer(_connectionString);

        }

    } 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Boletum>(entity =>
        {
            entity.HasKey(e => new { e.NumTicket, e.NumBoleta, e.NumCaja })
                .HasName("PK_boleta_1")
                .IsClustered(false);

            entity.ToTable("boleta");

            entity.Property(e => e.NumTicket)
                .ValueGeneratedOnAdd()
                .HasColumnName("num_ticket");
            entity.Property(e => e.NumBoleta).HasColumnName("num_boleta");
            entity.Property(e => e.NumCaja).HasColumnName("num_caja");
            entity.Property(e => e.CodTipoUsuario).HasColumnName("cod_tipo_usuario");
            entity.Property(e => e.CodTurno).HasColumnName("cod_turno");
            entity.Property(e => e.CodUsuario).HasColumnName("cod_usuario");
            entity.Property(e => e.Estado)
                .HasDefaultValue(1)
                .HasColumnName("estado");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.FechaEdicion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Fechaentrada)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("fechaentrada");
            entity.Property(e => e.Fechapago).HasColumnName("fechapago");
            entity.Property(e => e.Fechasalida).HasColumnName("fechasalida");
            entity.Property(e => e.Horaentrada)
                .HasDefaultValueSql("(getdate())")
                .HasColumnName("horaentrada");
            entity.Property(e => e.Horapago).HasColumnName("horapago");
            entity.Property(e => e.Horasalida).HasColumnName("horasalida");
            entity.Property(e => e.IncidenceId).HasColumnName("incidenceID");
            entity.Property(e => e.IncidenceIdSalida).HasColumnName("incidenceID_salida");

            entity.Property(e => e.Monto)
                .HasDefaultValue(0)
                .HasColumnName("monto");
            entity.Property(e => e.NombreAcceso)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre_acceso");
            entity.Property(e => e.Patente)
                .HasMaxLength(12)
                .IsUnicode(false)
                .HasColumnName("patente");
            entity.Property(e => e.TipoLiberado)
                .HasDefaultValue(4)
                .HasColumnName("tipo_liberado");
        });

        modelBuilder.Entity<Parametro>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("parametros");

            entity.Property(e => e.AwaitLecturaPatente).HasColumnName("await_LecturaPatente");
            entity.Property(e => e.CamEntrada)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("camEntrada");
            entity.Property(e => e.CamSalida)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("camSalida");
            entity.Property(e => e.CameraMask)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("cameraMask");
            entity.Property(e => e.DirAnpr)
                .HasMaxLength(18)
                .IsUnicode(false)
                .HasColumnName("dir_ANPR");
            entity.Property(e => e.LecturaPatente).HasColumnName("lecturaPatente");
            entity.Property(e => e.MsjCajero)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("msjCajero");
            entity.Property(e => e.NomImpresora)
                .HasMaxLength(165)
                .IsUnicode(false)
                .HasColumnName("nom_impresora");
            entity.Property(e => e.COMRele).HasColumnName("COM_rele");
            entity.Property(e => e.StringRele).HasColumnName("string_rele");
            entity.Property(e => e.StringRele2).HasColumnName("string_rele2");
            entity.Property(e => e.CameraMaskSalida)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("cameraMaskSalida");

        });

        modelBuilder.Entity<Variable>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("variables");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
