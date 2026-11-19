using System.Text.Json.Serialization;

namespace regulacao.core.models;

public class Atendimento
{
    public Atendimento() { }
    [JsonConstructor]
    public Atendimento
    (
        int id,
        string? nome,
        string? cpf,
        string? cns,
        DateTime dataAtendimento,
        string? historico,
        string? medicoSolicitante,
        List<Procedimento>? procedimentos
    )
    {
        Id = id;
        Nome = nome;
        Cpf = cpf;
        Cns = cns;
        DataAtendimento = dataAtendimento;
        Historico = historico;
        MedicoSolicitante = medicoSolicitante;
        Procedimentos = procedimentos;
    }
    public int Id { get; }
    public string? Nome { get; }
    public string? Cpf { get; }
    public string? Cns { get; }
    public DateTime DataAtendimento { get; }
    public string? Historico { get; }
    public string? MedicoSolicitante { get; }
    public List<Procedimento>? Procedimentos { get; }
}
public class AtendimentosData
{
    private string? id;
    private List<Atendimento>? data;
    public AtendimentosData()
    {
        id = DateTime.Now.ToString("yyyyMMddHHmmss");
    }
    public AtendimentosData(List<Atendimento> atendimentos)
    {
        Atendimentos = [.. atendimentos];
    }
    public string Aplicacao { get; private set; } = "ListarExames";
    public int Versao { get; private set; } = 1;
    public string Id
    {
        get => id ??= DateTime.Now.ToString("yyyyMMddHHmmss");
        set => id = string.IsNullOrWhiteSpace(value) ? DateTime.Now.ToString("yyyyMMddHHmmss") : value;
    }
    public List<Atendimento>? Atendimentos
    {
        get => data;
        set => data = value != null ? new(value) : null;
    }

    // Permite definir o Id a partir do nome do arquivo ao desserializar.
    // Método explícito para evitar exposição do setter público.
    public void SetIdFromFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return;

        // aceita tanto 'atendimento-YYYYMMDDHHMMSS' quanto apenas o timestamp
        id = fileName.Replace("atendimento-", "");
    }

}