using FileServer2.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace FileServer2.Models
{
    public class DadosArquivo
    {

        public class Arquivo
        {
            public string nome { get; set; }
            private byte[]? conteudo { get; set; }

            public Arquivo(string _nome)
            {
                nome = _nome;
            }

            public Arquivo(string _nome,
                           byte[] _conteudo)
            {
                nome = _nome;
                conteudo = _conteudo;
            }

            public byte[] ObterConteudo()
            {
                if (conteudo is not null)
                {
                    return conteudo;
                }
                else
                {
                    return new byte[0];
                }
            }


        }

        public enum eCodErro
        {
            DESCONHECIDO = -1,
            OK = 0,
            NAO_AUTORIZADO = 1,
            REQUISICAO_INCORRETA = 2,
            ARQUIVO_NAO_ENCONTRADO = 3,
            DIRETORIO_NAO_ENCONTRADO = 4,
            ERRO_GRAVACAO = 5,
            ERRO_GENERICO = 99
        }

        public eCodErro codErro { get; set; }
        public string msgErro { get; set; }
        public string caminhoCompletoArquivo { get; set; }
        public string caminhoArquivo { get; set; }
        public string nomeArquivo { get; set; }
        public string conteudoBase64 { get; set; }
        private byte[] conteudoBin { get; set; }
        //public List<string> listaArquivos { get; set; }
        public List<Arquivo> arquivos { get; set; }

        public DadosArquivo()
        {
            codErro = 0;
            msgErro = "-";
            caminhoCompletoArquivo = "";
            caminhoArquivo = "";
            nomeArquivo = "";
            conteudoBase64 = "";
            conteudoBin = new byte[] { };
            arquivos = new List<Arquivo>();
        }

        public DadosArquivo(eCodErro _codErro, string _msgErro)
        {
            //Startup.configuration.Get() = "";
            codErro = _codErro;
            msgErro = _msgErro;
            caminhoCompletoArquivo = "";
            caminhoArquivo = "";
            nomeArquivo = "";
            conteudoBase64 = "";
            conteudoBin = new byte[] { };
            arquivos = new List<Arquivo>();
        }

    }

}
