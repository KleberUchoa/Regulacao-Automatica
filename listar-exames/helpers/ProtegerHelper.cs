using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace listar_exames.helpers;

public static class ProtegerHelper
{
    public static string Proteger(string senha)
    {
        var bytes = Encoding.UTF8.GetBytes(senha);
        var protegido = ProtectedData.Protect(
            bytes, null, DataProtectionScope.CurrentUser);

        return Convert.ToBase64String(protegido);
    }

    public static string Desproteger(string senhaProtegida)
    {
        var protegido = Convert.FromBase64String(senhaProtegida);
        var bytes = ProtectedData.Unprotect(
            protegido, null, DataProtectionScope.CurrentUser);

        return Encoding.UTF8.GetString(bytes);
    }
}
