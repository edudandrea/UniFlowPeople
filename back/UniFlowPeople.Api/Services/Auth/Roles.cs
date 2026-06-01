namespace UniFlowPeople.Api.Services.Auth;

public static class Roles
{
    public const string SistemaAdmin = "SistemaAdmin";
    public const string EmpresaAdmin = "EmpresaAdmin";
    public const string RH = "RH";
    public const string Gestor = "Gestor";
    public const string Colaborador = "Colaborador";

    public const string EmpresaRoles = EmpresaAdmin + "," + RH + "," + Gestor + "," + Colaborador;
    public const string BackofficeRoles = SistemaAdmin;
    public const string AdminOrRh = EmpresaAdmin + "," + RH;
    public const string GestaoEmpresa = EmpresaAdmin + "," + RH + "," + Gestor;
}
