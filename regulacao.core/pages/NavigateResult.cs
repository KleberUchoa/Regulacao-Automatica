namespace regulacao.core.pages;

public class NavigateResult
{
    public bool Sucesso { get; set; }

    public string? Mensagem { get; set; }

    public Exception? Exception { get; set; }
}
