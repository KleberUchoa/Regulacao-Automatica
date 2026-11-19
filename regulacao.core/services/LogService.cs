using System.Text;

namespace regulacao.core.services
{
    public class LogService
    {
        readonly string caminhoDownloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        readonly DateTime dataHora = DateTime.Now;
        readonly StringBuilder sb;
        public LogService()
        {
            sb = new StringBuilder();
        }
        public void AdicionarLinha(string mensagem)
        {
            sb.AppendLine($"[{dataHora:dd:MM:yyyy HH:mm:ss}] {mensagem}");
        }
        public void SalvarLog()
        {
            try
            {
                File.WriteAllText($"{caminhoDownloads}/logErros-{dataHora:yyyyMMddHHmmss}.txt", sb.ToString());
            }
            catch (Exception ex)
            {
               Console.WriteLine($"Falha ao salvar o log: {ex.Message}");
                // Handle the exception (e.g., log to console, display message)
            }

        }
    }
}
