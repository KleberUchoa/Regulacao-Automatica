using regulacao.core.models;
namespace listar_exames.helpers;

public static class Validacao
{
    public static bool ValidarProcedimentosSelecionados(List<Procedimento> procs, out string erro)
    {
        erro = string.Empty;
        if (procs == null)
        {
            erro = "lista de procedimentos inválida";
            return false;
        }
        if (procs.Count != 0)
        {
            for (int i = 0; i < procs.Count; i++)
            {
                if (procs[i] == null)
                {
                    erro = "há um procedimento inválido na lista";
                    return false;
                }
                if (string.IsNullOrEmpty(procs[i].Codigo))
                {
                    erro = "placeholder selecionado";
                    return false;
                }
                //verificar se há procedimentos repitidos
                for (int j = i + 1; j < procs.Count; j++)
                {
                    if (procs[i].Codigo == procs[j].Codigo)
                    {
                        erro = $"procedimento {procs[i].Nome} está repetido";
                        return false;

                    }
                }

            }
            return true;
        }
        erro = "nenhum procedimento selecionado";
        return false;
    }
    public static bool ValidarCampoObrigatorio(string nome)
    {
        return !string.IsNullOrEmpty(nome);
    }
    public static bool ValidarData(DateTime data)
    {
        var dataAtual = DateTime.Now.Date;
        var dataInserida = data;
        if (dataAtual >= dataInserida)
            return false;
        return true;
    }
    public static bool ValidarCNS(string cns)
    {
        if (string.IsNullOrWhiteSpace(cns)) return false;
        cns = cns.Trim();
        if (cns.Length != 15 || !cns.All(char.IsDigit)) return false;

        return cns[0] switch
        {
            '1' or '2' => ValidarCnsDefinitivo(cns),
            '7' or '8' or '9' => ValidarCnsProvisorio(cns),
            _ => false,
        };
    }
    private static bool ValidarCnsDefinitivo(string cns)
    {
        string pis = cns[..11];
        int soma = 0;
        for (int i = 0; i < 11; i++)
            soma += (pis[i] - '0') * (15 - i);

        int resto = soma % 11;
        int dv = 11 - resto;

        string resultado;
        if (dv == 11)
        {
            resultado = pis + "000" + "0";
        }
        else if (dv == 10)
        {
            soma = 0;
            for (int i = 0; i < 11; i++)
                soma += (pis[i] - '0') * (15 - i);
            soma += 2;
            resto = soma % 11;
            dv = 11 - resto;
            resultado = pis + "001" + dv;
        }
        else
        {
            resultado = pis + "000" + dv;
        }

        return resultado == cns;
    }
    private static bool ValidarCnsProvisorio(string cns)
    {
        int soma = 0;
        for (int i = 0; i < 15; i++)
            soma += (cns[i] - '0') * (15 - i);
        return soma % 11 == 0;
    }
    public static bool ValidarCpf(string cpf)
    {
        cpf = cpf.Trim().Replace(".", "").Replace("-", "");

        if (cpf.Length != 11)
            return false;

        if (cpf.All(c => c == cpf[0]))
            return false;

        int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        string tempCpf = cpf[..9];

        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += (tempCpf[i] - '0') * multiplicador1[i];

        int resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;

        tempCpf += resto;

        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += (tempCpf[i] - '0') * multiplicador2[i];

        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;

        return cpf.EndsWith(resto.ToString());
    }
    public static bool ValidarPossuiDocumento(string cpf, string cns)
    {
        return ValidarCampoObrigatorio(cpf) || ValidarCampoObrigatorio(cns);
    }
}