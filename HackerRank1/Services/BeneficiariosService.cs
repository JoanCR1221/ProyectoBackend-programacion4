using HackerRank1.Data;
using HackerRank1.DTO;
using HackerRank1.Entities;
using Microsoft.EntityFrameworkCore;

namespace HackerRank1.Services;

public interface IBeneficiariosService
{
    Task<IEnumerable<BeneficiarioDto>> GetAll();
    Task<BeneficiarioDto> Create(BeneficiarioForm form);
    Task<BeneficiarioDto?> Update(string id, BeneficiarioForm form);
    Task<BeneficiarioDto?> ToggleStatus(string id);
}

public class BeneficiariosService : IBeneficiariosService
{
    private readonly BeneficiariosContext _context;

    public BeneficiariosService(BeneficiariosContext context)
    {
        _context = context;
    }

    private static string FormatId(int id) => $"b{id:D3}";

    private static int ParseId(string id) => int.Parse(id.Substring(1));

    private static BeneficiarioDto ToDto(Beneficiario b) => new BeneficiarioDto
    {
        Id = FormatId(b.Id),
        NombreCompleto = b.NombreCompleto,
        Cedula = b.Cedula,
        FechaNacimiento = b.FechaNacimiento,
        TipoBeneficiario = b.TipoBeneficiario,
        Telefono = b.Telefono,
        Direccion = b.Direccion,
        PersonasACargo = b.PersonasACargo,
        Observaciones = b.Observaciones,
        Activo = b.Activo,
        FechaRegistro = b.FechaRegistro
    };

    public async Task<IEnumerable<BeneficiarioDto>> GetAll()
    {
        var lista = await _context.Beneficiarios.ToListAsync();
        return lista.Select(ToDto);
    }

    public async Task<BeneficiarioDto> Create(BeneficiarioForm form)
    {
        var beneficiario = new Beneficiario
        {
            NombreCompleto = form.NombreCompleto,
            Cedula = form.Cedula,
            FechaNacimiento = form.FechaNacimiento,
            TipoBeneficiario = form.TipoBeneficiario,
            Telefono = form.Telefono,
            Direccion = form.Direccion,
            PersonasACargo = form.PersonasACargo,
            Observaciones = form.Observaciones,
            Activo = true,
            FechaRegistro = DateTime.Now.ToString("yyyy-MM-dd")
        };

        _context.Beneficiarios.Add(beneficiario);
        await _context.SaveChangesAsync();

        return ToDto(beneficiario);
    }

    public async Task<BeneficiarioDto?> Update(string id, BeneficiarioForm form)
    {
        var numericId = ParseId(id);
        var beneficiario = await _context.Beneficiarios.FindAsync(numericId);
        if (beneficiario is null) return null;

        beneficiario.NombreCompleto = form.NombreCompleto;
        beneficiario.Cedula = form.Cedula;
        beneficiario.FechaNacimiento = form.FechaNacimiento;
        beneficiario.TipoBeneficiario = form.TipoBeneficiario;
        beneficiario.Telefono = form.Telefono;
        beneficiario.Direccion = form.Direccion;
        beneficiario.PersonasACargo = form.PersonasACargo;
        beneficiario.Observaciones = form.Observaciones;

        await _context.SaveChangesAsync();

        return ToDto(beneficiario);
    }

    public async Task<BeneficiarioDto?> ToggleStatus(string id)
    {
        var numericId = ParseId(id);
        var beneficiario = await _context.Beneficiarios.FindAsync(numericId);
        if (beneficiario is null) return null;

        beneficiario.Activo = !beneficiario.Activo;
        await _context.SaveChangesAsync();

        return ToDto(beneficiario);
    }
}
