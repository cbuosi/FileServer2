using FileServer2.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;
using static FileServer2.Models.DadosArquivo;

namespace FileServer2.Controllers
{

    public class ArquivoController : Controller
    {

        public string? CaminhoInicial { get; set; }
        public string? BearerAPI { get; set; }


        IConfiguration? configuration;

        private Stopwatch? oReloginho = null;


        //public override void OnActionExecuting(ActionExecutingContext context)
        //{
        //
        //    ////LogaErro("OnActionExecuting");
        //    //oReloginho = new Stopwatch();
        //    //oReloginho.Start();
        //    //base.OnActionExecuting(context);
        //    //_context = context;
        //
        //    LogaErro("INICIO! (OnActionExecuting)");
        //    //User.Identity.
        //
        //}
        //
        //public override void OnActionExecuted(ActionExecutedContext context)
        //{
        //    LogaErro("FIM! "); // + tRend.ToString("F4") + " segs. (OnActionExecuted)\n");
        //}


        public ArquivoController(IConfiguration _config)
        {

            try
            {

                oReloginho = new Stopwatch();
                oReloginho.Start();

                LogaErro("-- ArquivoController --------------------------------------------------------");
                configuration = _config;
                //configuration = x.configuration;
                CaminhoInicial = configuration.GetValue<string>("FileServer:CaminhoInicial");
                BearerAPI = configuration.GetValue<string>("FileServer:BearerAPI");
                LogaErro(">>>>>>>>>> BearerAPI..........: " + BearerAPI);
                LogaErro(">>>>>>>>>> CaminhoInicial.....: " + CaminhoInicial);

            }
            catch (Exception ex)
            {
                LogaErro("Erro em ArquivoController: " + ex.Message);
            }

        }

        //public IActionResult Index()
        //{
        //    return View();
        //}

        [HttpPost]
        public JsonResult ObterConteudoArquivo([FromBody] DadosArquivo dadosArquivo)
        {

            int tam;
            Stopwatch sw;

            try
            {
                sw = Stopwatch.StartNew();
                LogaErro(">>>>>>>>>> Inicio: ObterConteudoArquivo");

                Request.Headers.TryGetValue("BearerAPI", out var varBearerAPI);

                if (varBearerAPI.Count != 1)
                {
                    return Json(new DadosArquivo(eCodErro.NAO_AUTORIZADO, "Nao autorizado (1): BearerAPI Zerado"));
                }

                if (BearerAPI is not null)
                {
                    if (varBearerAPI.ToString().Trim() != BearerAPI.Trim())
                    {
                        return Json(new DadosArquivo(eCodErro.NAO_AUTORIZADO, "Nao autorizado (2): BearerAPI Inválido"));
                    }
                }

                LogaErro("BearerAPI OK: " + varBearerAPI.ToString());

                if (dadosArquivo == null)
                {
                    return Json(new DadosArquivo(eCodErro.REQUISICAO_INCORRETA, "Erro no ObterConteudoArquivo: Dados Nulos"));
                }

                // -------------------------------------------------------------------------------------------
                LogaErro("caminhoCompletoArquivo...: " + dadosArquivo.caminhoCompletoArquivo);
                LogaErro("CaminhoArquivo...........: " + dadosArquivo.caminhoArquivo);
                LogaErro("NomeArquivo..............: " + dadosArquivo.nomeArquivo);
                // -------------------------------------------------------------------------------------------

                if ((dadosArquivo.caminhoArquivo.Trim().Length > 0) && (dadosArquivo.nomeArquivo.Trim().Length > 0))
                {
                    dadosArquivo.caminhoCompletoArquivo = Path.Combine(dadosArquivo.caminhoArquivo, dadosArquivo.nomeArquivo);
                    dadosArquivo.caminhoCompletoArquivo = Path.Combine(CaminhoInicial!, dadosArquivo.caminhoCompletoArquivo);
                }
                else if (dadosArquivo.caminhoCompletoArquivo.Trim().Length > 0)
                {
                    dadosArquivo.caminhoCompletoArquivo = Path.Combine(CaminhoInicial!, dadosArquivo.caminhoCompletoArquivo);
                }
                else
                {
                    return Json(new DadosArquivo(eCodErro.REQUISICAO_INCORRETA, "ObterConteudoArquivo: Erro: caminhoArquivo+nomeArquivo OU caminhoCompletoArquivo Obrigatórios!"));
                }

                if (System.IO.File.Exists(dadosArquivo.caminhoCompletoArquivo) == false)
                {
                    return Json(new DadosArquivo(eCodErro.ARQUIVO_NAO_ENCONTRADO, "ObterConteudoArquivo: Erro: caminho ou arquivo inexistente!"));
                }

                dadosArquivo.conteudoBase64 = ObterBase64Arquivo(dadosArquivo.caminhoCompletoArquivo, out tam);

                dadosArquivo.codErro = eCodErro.OK;
                dadosArquivo.msgErro = "ObterConteudoArquivo: OK! Tempo: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("#,##0.000") + " Tamanho: " + tam.ToString("#,##0") + " Tamanho64: " + dadosArquivo.conteudoBase64.Length.ToString("#,##0");
                return Json(dadosArquivo);
            }
            catch (Exception ex)
            {
                return Json(new DadosArquivo(eCodErro.ERRO_GENERICO, "Erro em ObterConteudoArquivo: " + ex.Message));
            }
            finally
            {
                LogaErro(">>>>>>>>>> Fim: ObterConteudoArquivo");
            }

        }

        [HttpPost]
        public JsonResult ObterConteudoZipDiretorio([FromBody] DadosArquivo dadosArquivo)
        {

            int tam;
            Stopwatch sw;
            FileInfo[] oFileInfo;
            DirectoryInfo oDirectoryInfo;
            //
            string fileNameZip;
            string fileNameZipCompleto;
            byte[] compressedBytes;
            Stream entryStream;
            MemoryStream outStream;
            MemoryStream fileToCompressStream;
            ZipArchive archive;
            ZipArchiveEntry fileInArchive0;

            try
            {
                sw = Stopwatch.StartNew();
                LogaErro(">>>>>>>>>> Inicio: ObterConteudoZipDiretorio");

                Request.Headers.TryGetValue("BearerAPI", out var varBearerAPI);

                if (varBearerAPI.Count != 1)
                {
                    return Json(new DadosArquivo(eCodErro.NAO_AUTORIZADO, "Nao autorizado (1): BearerAPI Zerado"));
                }

                if (BearerAPI is not null)
                {
                    if (varBearerAPI.ToString().Trim() != BearerAPI.Trim())
                    {
                        return Json(new DadosArquivo(eCodErro.NAO_AUTORIZADO, "Nao autorizado (2): BearerAPI Inválido"));
                    }
                }

                LogaErro("BearerAPI OK: " + varBearerAPI.ToString());

                if (dadosArquivo == null)
                {
                    return Json(new DadosArquivo(eCodErro.REQUISICAO_INCORRETA, "Erro no ObterConteudoArquivo: Dados Nulos"));
                }

                // -------------------------------------------------------------------------------------------
                LogaErro("CaminhoArquivo...........: " + dadosArquivo.caminhoArquivo);
                // -------------------------------------------------------------------------------------------

                if (dadosArquivo.caminhoArquivo.Trim().Length == 0)
                {
                    return Json(new DadosArquivo(eCodErro.REQUISICAO_INCORRETA, "ObterConteudoArquivo: Erro: caminhoArquivo obrigatórios!"));
                }

                if (CaminhoInicial is not null)
                {
                    dadosArquivo.caminhoArquivo = Path.Combine(CaminhoInicial, dadosArquivo.caminhoArquivo);
                }

                if (System.IO.Directory.Exists(dadosArquivo.caminhoArquivo) == false)
                {
                    return Json(new DadosArquivo(eCodErro.DIRETORIO_NAO_ENCONTRADO, "ObterConteudoArquivo: Erro: caminho inexistente!"));
                }

                // ###############################################################################################

                dadosArquivo.arquivos = new List<DadosArquivo.Arquivo>();
                oDirectoryInfo = new DirectoryInfo(dadosArquivo.caminhoArquivo);
                oFileInfo = oDirectoryInfo.GetFiles("*.*"); //Getting Text files

                foreach (FileInfo arquivo in oFileInfo)
                {
                    dadosArquivo.arquivos.Add(new DadosArquivo.Arquivo(arquivo.Name, System.IO.File.ReadAllBytes(arquivo.FullName)));
                }

                fileNameZip = "Donload_" + DateTime.Now.ToString("yyyy.MM.dd.hhmmss") + ".zip";
                fileNameZipCompleto = Path.Combine(dadosArquivo.caminhoArquivo, fileNameZip);

                outStream = new MemoryStream();

                archive = new ZipArchive(outStream, ZipArchiveMode.Create, true);

                foreach (DadosArquivo.Arquivo arquivo in dadosArquivo.arquivos)
                {
                    fileInArchive0 = archive.CreateEntry(arquivo.nome, CompressionLevel.SmallestSize);
                    entryStream = fileInArchive0.Open();
                    fileToCompressStream = new MemoryStream(arquivo.ObterConteudo());
                    fileToCompressStream.CopyTo(entryStream);
                    fileToCompressStream.Close();
                    entryStream.Close();
                }


                //fileBytes0 = System.IO.File.ReadAllBytes(@"C:\temp\fs\TESTE\Cemu.exe");
                //fileInArchive0 = archive.CreateEntry(fileName0, CompressionLevel.SmallestSize);
                //entryStream = fileInArchive0.Open();
                //fileToCompressStream = new MemoryStream(fileBytes0);
                //fileToCompressStream.CopyTo(entryStream);
                //fileToCompressStream.Close();
                //entryStream.Close();
                //
                //fileBytes0 = System.IO.File.ReadAllBytes(@"C:\temp\fs\TESTE\nomedopai.mp4");
                //fileInArchive0 = archive.CreateEntry(fileName1, CompressionLevel.SmallestSize);
                //entryStream = fileInArchive0.Open();
                //fileToCompressStream = new MemoryStream(fileBytes0);
                //fileToCompressStream.CopyTo(entryStream);
                //fileToCompressStream.Close();
                //entryStream.Close();

                archive.Dispose(); //efetiva as bagaças.

                compressedBytes = outStream.ToArray();

                //tirar
                System.IO.File.WriteAllBytes(fileNameZipCompleto, compressedBytes);

                dadosArquivo.nomeArquivo = fileNameZip;
                dadosArquivo.conteudoBase64 = Convert.ToBase64String(compressedBytes);

                //dadosArquivo.conteudoBase64 = ObterBase64Arquivo(dadosArquivo.caminhoCompletoArquivo, out tam);
                tam = compressedBytes.Length;

                dadosArquivo.codErro = eCodErro.OK;
                dadosArquivo.msgErro = "ObterConteudoZipDiretorio: OK! Tempo: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("#,##0.000") +
                                       " Qtd Arquivos Zip: " + dadosArquivo.arquivos.Count().ToString() +
                                       " Tamanho: " + tam.ToString("#,##0") +
                                       " Tamanho64: " + dadosArquivo.conteudoBase64.Length.ToString("#,##0");

                return Json(dadosArquivo);
            }
            catch (Exception ex)
            {
                return Json(new DadosArquivo(eCodErro.ERRO_GENERICO, "Erro em ObterConteudoArquivo: " + ex.Message));
            }
            finally
            {
                LogaErro(">>>>>>>>>> Fim: ObterConteudoZipDiretorio");
            }

        }

        public void LogaErro(string _msg, bool bHora = true)
        {
            //
            if (bHora)
            {
                _msg = DateTime.Now.ToString("HH:mm:ss FFFFF") + ": " + _msg;
            }
            //
            System.Diagnostics.Debug.WriteLine(_msg);
            Console.WriteLine(_msg);
            //a
        }

        [HttpPost]
        public JsonResult GravarArquivo([FromBody] DadosArquivo dadosArquivo)
        {

            int tam = 0;
            Stopwatch sw;
            bool bSobrescreveu = false;
            string ret = "";

            try
            {
                sw = Stopwatch.StartNew();

                LogaErro(">>>>>>>>>> Inicio: GravarArquivo");

                Request.Headers.TryGetValue("BearerAPI", out var varBearerAPI);

                if (varBearerAPI.Count != 1)
                {
                    return Json(new DadosArquivo(eCodErro.NAO_AUTORIZADO, "Nao autorizado (1): BearerAPI Zerado"));
                }

                if (BearerAPI is not null)
                {
                    if (varBearerAPI.ToString().Trim() != BearerAPI.Trim())
                    {
                        return Json(new DadosArquivo(eCodErro.NAO_AUTORIZADO, "Nao autorizado (2): BearerAPI Inválido"));
                    }
                }

                LogaErro("BearerAPI OK: " + varBearerAPI.ToString());

                if (dadosArquivo == null)
                {
                    return Json(new DadosArquivo(eCodErro.ERRO_GENERICO, "Erro no GravarArquivo: Dados Nulos"));
                }

                // -------------------------------------------------------------------------------------------
                LogaErro("caminhoCompletoArquivo...: " + dadosArquivo.caminhoCompletoArquivo);
                LogaErro("CaminhoArquivo...........: " + dadosArquivo.caminhoArquivo);
                LogaErro("NomeArquivo..............: " + dadosArquivo.nomeArquivo);
                // -------------------------------------------------------------------------------------------

                if ((dadosArquivo.caminhoArquivo.Trim().Length > 0) && (dadosArquivo.nomeArquivo.Trim().Length > 0))
                {
                    dadosArquivo.caminhoCompletoArquivo = Path.Combine(dadosArquivo.caminhoArquivo, dadosArquivo.nomeArquivo);
                    dadosArquivo.caminhoCompletoArquivo = Path.Combine(CaminhoInicial!, dadosArquivo.caminhoCompletoArquivo);
                }
                else if (dadosArquivo.caminhoCompletoArquivo.Trim().Length > 0)
                {
                    dadosArquivo.caminhoCompletoArquivo = Path.Combine(CaminhoInicial!, dadosArquivo.caminhoCompletoArquivo);
                }
                else
                {
                    return Json(new DadosArquivo(eCodErro.REQUISICAO_INCORRETA, "GravarArquivo: Erro: caminhoArquivo+nomeArquivo OU caminhoCompletoArquivo Obrigatórios!"));
                }

                //se ja existir, sobrescreve
                if (System.IO.File.Exists(dadosArquivo.caminhoCompletoArquivo) == true)
                {
                    bSobrescreveu = true;
                    System.IO.File.Delete(dadosArquivo.caminhoCompletoArquivo);
                }

                ret = SalvarArquivoBase64(dadosArquivo.caminhoCompletoArquivo, dadosArquivo.conteudoBase64, out tam);

                if (ret != "")
                {
                    return Json(new DadosArquivo(eCodErro.ERRO_GRAVACAO, "Erro em GravarArquivo: " + ret));
                }

                dadosArquivo.msgErro = "GravarArquivo: OK! Tempo: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("#,##0.000") + " Tamanho: " + tam.ToString("#,##0") + " Tamanho64: " + dadosArquivo.conteudoBase64.Length.ToString("#,##0");

                if (bSobrescreveu == true)
                {
                    dadosArquivo.msgErro = dadosArquivo.msgErro + " (Obs: arquivo ja existente sobrescrito!)";
                }

                //nao precisa voltar a bagaça toda....
                dadosArquivo.conteudoBase64 = "";

                return Json(dadosArquivo);
                // -------------------------------------------------------------------------------------------
            }
            catch (Exception ex)
            {
                return Json(new DadosArquivo(eCodErro.ERRO_GENERICO, "Erro em GravarArquivo: " + ex.Message));
            }
            finally
            {
                LogaErro(">>>>>>>>>> Fim: GravarArquivo");
            }

        }

        [HttpPost]
        public JsonResult ListarArquivos([FromBody] DadosArquivo dadosArquivo)
        {

            Stopwatch sw;
            FileInfo[] oFileInfo;
            DirectoryInfo oDirectoryInfo;

            try
            {
                sw = Stopwatch.StartNew();

                LogaErro(">>>>>>>>>> Inicio: ListarArquivos");

                Request.Headers.TryGetValue("BearerAPI", out var varBearerAPI);

                //BEARER não veio
                if (varBearerAPI.Count != 1)
                {
                    return Json(new DadosArquivo(eCodErro.NAO_AUTORIZADO, "Nao autorizado (1): BearerAPI Zerado"));
                }

                //BEARER NAO BATE
                if (BearerAPI is not null)
                {
                    if (varBearerAPI.ToString().Trim() != BearerAPI.Trim())
                    {
                        return Json(new DadosArquivo(eCodErro.NAO_AUTORIZADO, "Nao autorizado (2): BearerAPI Inválido"));
                    }
                }

                LogaErro("BearerAPI OK: " + varBearerAPI.ToString());

                //Body vazio ou incorreto...
                if (dadosArquivo == null)
                {
                    return Json(new DadosArquivo(eCodErro.ERRO_GENERICO, "Erro no ExisteDiretorio: Dados Nulos"));
                }

                // -------------------------------------------------------------------------------------------
                LogaErro("CaminhoArquivo...........: " + dadosArquivo.caminhoArquivo);
                // -------------------------------------------------------------------------------------------

                if (dadosArquivo.caminhoArquivo.Trim().Length == 0)
                {
                    return Json(new DadosArquivo(eCodErro.REQUISICAO_INCORRETA, "ListarArquivos: Erro: caminhoArquivo Obrigatório!"));
                }

                dadosArquivo.caminhoArquivo = Path.Combine(CaminhoInicial!, dadosArquivo.caminhoArquivo);

                //se ja existir, sobrescreve
                if (System.IO.Directory.Exists(dadosArquivo.caminhoArquivo) == false)
                {
                    return Json(new DadosArquivo(eCodErro.DIRETORIO_NAO_ENCONTRADO, "ListarArquivos: Erro: caminhoArquivo inexistente"));
                }

                dadosArquivo.arquivos = new List<DadosArquivo.Arquivo>();
                oDirectoryInfo = new DirectoryInfo(dadosArquivo.caminhoArquivo);
                oFileInfo = oDirectoryInfo.GetFiles("*.*"); //Getting Text files
                foreach (FileInfo arquivo in oFileInfo)
                {
                    dadosArquivo.arquivos.Add(new DadosArquivo.Arquivo(arquivo.Name));
                }


                dadosArquivo.msgErro = "ListarArquivos: OK! Tempo: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("#,##0.000") + " Arquivos encontrados: " + oFileInfo.Count().ToString();


                //dadosArquivo.caminhoArquivo = "";
                //limpa bagaças
                dadosArquivo.caminhoCompletoArquivo = "";
                dadosArquivo.nomeArquivo = "";
                dadosArquivo.conteudoBase64 = "";


                return Json(dadosArquivo);
                // -------------------------------------------------------------------------------------------

            }
            catch (Exception ex)
            {
                return Json(new DadosArquivo(eCodErro.ERRO_GENERICO, "Erro em ListarArquivos: " + ex.Message));
            }
            finally
            {
                LogaErro(">>>>>>>>>> Fim: ListarArquivos");
            }

        }

        [HttpPost]
        public JsonResult CriarDiretorio([FromBody] DadosArquivo dadosArquivo)
        {

            Stopwatch sw;
            bool bJaExiste = false;

            try
            {
                sw = Stopwatch.StartNew();

                LogaErro(">>>>>>>>>> Inicio: CriarDiretorio");

                Request.Headers.TryGetValue("BearerAPI", out var varBearerAPI);

                //BEARER não veio
                if (varBearerAPI.Count != 1)
                {
                    return Json(new DadosArquivo(eCodErro.NAO_AUTORIZADO, "Nao autorizado (1): BearerAPI Zerado"));
                }

                //BEARER NAO BATE
                if (BearerAPI is not null)
                {
                    if (varBearerAPI.ToString().Trim() != BearerAPI.Trim())
                    {
                        return Json(new DadosArquivo(eCodErro.NAO_AUTORIZADO, "Nao autorizado (2): BearerAPI Inválido"));
                    }
                }

                LogaErro("BearerAPI OK: " + varBearerAPI.ToString());

                //Body vazio ou incorreto...
                if (dadosArquivo == null)
                {
                    return Json(new DadosArquivo(eCodErro.ERRO_GENERICO, "Erro no CriarDiretorio: Dados Nulos"));
                }

                // -------------------------------------------------------------------------------------------
                LogaErro("CaminhoArquivo...........: " + dadosArquivo.caminhoArquivo);
                // -------------------------------------------------------------------------------------------

                if (dadosArquivo.caminhoArquivo.Trim().Length == 0)
                {
                    return Json(new DadosArquivo(eCodErro.REQUISICAO_INCORRETA, "CriarDiretorio: Erro: caminhoArquivo Obrigatório!"));
                }

                dadosArquivo.caminhoArquivo = Path.Combine(CaminhoInicial!, dadosArquivo.caminhoArquivo);

                //se ja existir, sobrescreve
                if (System.IO.Directory.Exists(dadosArquivo.caminhoArquivo) == true)
                {
                    bJaExiste = true;
                }
                else
                {
                    System.IO.Directory.CreateDirectory(dadosArquivo.caminhoArquivo);
                }

                dadosArquivo.msgErro = "CriarDiretorio: OK! Tempo: " + ((decimal)sw.ElapsedMilliseconds / 1000).ToString("#,##0.000");

                if (bJaExiste == true)
                {
                    dadosArquivo.msgErro = dadosArquivo.msgErro + " (Obs: diretório ja existente)";
                }

                //dadosArquivo.caminhoArquivo = "";
                //limpa bagaças
                dadosArquivo.caminhoCompletoArquivo = "";
                dadosArquivo.nomeArquivo = "";
                dadosArquivo.conteudoBase64 = "";
                dadosArquivo.arquivos = new List<DadosArquivo.Arquivo>();

                return Json(dadosArquivo);
                // -------------------------------------------------------------------------------------------

            }
            catch (Exception ex)
            {
                return Json(new DadosArquivo(eCodErro.ERRO_GENERICO, "Erro em CriarDiretorio: " + ex.Message));
            }
            finally
            {
                LogaErro(">>>>>>>>>> Fim: CriarDiretorio");
            }

        }

        //[HttpPost]
        //public JsonResult ApagarArquivo([FromBody] DadosArquivo dadosArquivo)
        //{
        //
        //    try
        //    {
        //
        //        if (dadosArquivo == null)
        //        {
        //            return Json(new DadosArquivo(eCodErro.ERRO_GENERICO, "Erro no Apagar: Dados Nulos"));
        //        }
        //
        //        // -------------------------------------------------------------------------------------------
        //        LogaErro(DateTime.Now.ToLongTimeString());
        //        //LogaErro("MsgErro..........: " + dadosArquivo.msgErro);
        //        //LogaErro("CaminhoArquivo...: " + dadosArquivo.caminhoArquivo);
        //        //LogaErro("NomeArquivo......: " + dadosArquivo.nomeArqui2vo);
        //        // ---------------------------------------------------------------------------------------2----
        //        dadosArquivo.msgErro = "Apagar: OK!";
        //        return Json(dadosArquivo);
        //        // -------------------------------------------------------------------------------------------
        //
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new DadosArquivo(eCodErro.ERRO_GENERICO, "Erro em Apagar: " + ex.Message));
        //    }
        //
        //}

        public string ObterBase64Arquivo(string _CaminhoArquivo, out int Tamanho)
        {
            Byte[] bytes;
            try
            {
                bytes = System.IO.File.ReadAllBytes(_CaminhoArquivo);
                Tamanho = bytes.Length;
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                Tamanho = -1;
                LogaErro("Erro em ObterBase64Arquivo: " + ex.Message);
                return "";
            }
        }

        public string SalvarArquivoBase64(string _CaminhoArquivo, string strBase64, out int Tamanho)
        {
            Byte[] bytes;
            try
            {
                bytes = Convert.FromBase64String(strBase64);
                Tamanho = bytes.Length;

                System.IO.File.WriteAllBytes(_CaminhoArquivo, bytes);
                Tamanho = bytes.Length;

                return "";

            }
            catch (Exception ex)
            {
                Tamanho = -1;
                LogaErro("Erro em SalvarArquivoBase64: " + ex.Message);
                return "Erro em SalvarArquivoBase64: " + ex.Message;
            }
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {


            }

            if (oReloginho is not null)
            {
                LogaErro("------------------------------------------------------------ " + Decimal.Divide(oReloginho.ElapsedMilliseconds, 1000).ToString("F4") + " Seg(s).");
                oReloginho = null;
            }

            base.Dispose(disposing);
        }

    }


}
