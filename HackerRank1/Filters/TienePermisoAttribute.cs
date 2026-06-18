using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace HackerRank1.Filters;

/// <summary>
/// Atributo que verifica si el usuario autenticado tiene un permiso específico.
/// Si no tiene el permiso, retorna 403 Forbidden.
/// </summary>
public class TienePermisoAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permisoRequerido;

    public TienePermisoAttribute(string permisoRequerido)
    {
        _permisoRequerido = permisoRequerido;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var usuario = context.HttpContext.User;

        // Verificar que el usuario está autenticado
        if (!usuario.Identity?.IsAuthenticated ?? false)
        {
            context.Result = new UnauthorizedObjectResult(new { mensaje = "Usuario no autenticado" });
            return;
        }

        // Verificar que el usuario tiene el permiso requerido
        var tienePermiso = usuario.FindAll("permiso")
            .Any(c => c.Value == _permisoRequerido);

        if (!tienePermiso)
        {
            context.Result = new ForbidResult();
        }
    }
}
