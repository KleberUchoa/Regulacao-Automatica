using System.Text.Json.Serialization;

namespace regulacao.core.models;

public class Procedimento
{
    public string? Codigo { get; }
    public string? Nome { get; }
    [JsonConstructor]
    public Procedimento(string codigo, string nome)
    {
        Nome = nome;
        Codigo = codigo;
    }
}
public class ProcedimentosData
{
    //carrega se procedimentos.json não existe
    public Procedimento[] Procedimentos =
    {
        new("0202010120","ACIDO ÚRICO"),
        new("0202030636","ANTI-HBS"),
        new("0202030679","ANTI-HCV"),
        new("0202030300","ANTI-HIV (ELISA)"),
        new("0202010201","BILIRRIBULINAS TOTAIS E FRAÇÕES"),
        new("0202010210","CÁLCIO"),
        new("0202010228","CÁLCIO IONIZÁVEL"),
        new("0202010295","COLESTEROL TOTA"),
        new("0202010317","CREATININA"),
        new("0202010384","FERRITINA"),
        new("0202010392","FERRO SÉRICO"),
        new("0202040127","FEZES (OVOS E CISTOS PARASITAS)"),
        new("0202010422","FOSFATASE ALCALINA"),
        new("0202010430","FÓSFORO"),
        new("0202010465","GAMA GT"),
        new("0202010473","GLICOSE"),
        new("0202010279","HDL"),
        new("0202010503","HEMOGLOBINA GLICOSILADA"),
        new("0202020380","HEMOGRAMA COMPLETO"),
        new("0202060268","INSULINA"),
        new("0202010287","LDL"),
        new("0202010600","POTÁSSIO"),
        new("0202030105","PSA"),
        new("0202031110","SIFILIS (TREPONÊMICO)"),
        new("0202031179","SIFILIS EM GESTANTE (NÃO TREPONÊMICO)"),
        new("0202010635","SÓDIO"),
        new("0202060373","T4"),
        new("0202020142","TAP"),
        new("0202010643","TGO"),
        new("0202010651","TGP"),
        new("0202010660","TRANSFERRINA"),
        new("0202010678","TRIGLICERÍDEOS"),
        new("0202060250","TSH"),
        new("0202010694","UREIA"),
        new("0202050017","URINA"),
    };
}