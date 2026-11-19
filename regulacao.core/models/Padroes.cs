namespace regulacao.core.models;

public class Padroes
{
    public string Site { get; private set; }
    public string Usuario { get; private set; }
    public string Senha { get; private set; }
    public string EstabelecimentoRequisitado { get; private set; }
    public List<string>? Medicos { get; private set; }
    public string Cid { get; private set; }
    public string Prioridade { get; private set; }
    public string NivelDeUrgencia { get; private set; }
    public Padroes(string site, string usuario, string senha, string estabelRequisitado, List<string>? medicos,
        string cid, string prioridade, string nivelUrgencia)
    {
        Site = site;
        Usuario = usuario;
        Senha = senha;
        EstabelecimentoRequisitado = estabelRequisitado;
        Medicos = medicos;
        Cid = cid;
        Prioridade = prioridade;
        NivelDeUrgencia = nivelUrgencia;
    }
}
public class PadroesData()
{
    public Padroes padroes = new(
        "https://regulapiaui.sesapi.pi.gov.br/",
        "",
        "",
        "",
        [""],
        "Z000",
        "Não se aplica",
        "P3");
}
