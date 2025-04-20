using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Spech_to_Text_Crist_Aza_2023_0731.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpechToTextController : ControllerBase
    {
        //Datos de entorno: para obtener la dirección de la carpeta del proyecto y guardar los audios para transcribir
        private readonly IWebHostEnvironment _env;

        public SpechToTextController(IWebHostEnvironment env)
        {
            _env = env;
        }

        //Endpoint para transcribir a partir del archivo subido
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile audio)
        {
            //Si el audio es nulo o esta vacio, se retorna un error 400 para notificar una mala solicitud
            if (audio == null || audio.Length == 0)
                return BadRequest("Archivo inválido.");

            //Obtenemoa la dirección de la carpeta Uploads en los archivos del proyecto, o la creamos en caso de no existir
            var uploads = Path.Combine(_env.ContentRootPath, "Uploads");
            Directory.CreateDirectory(uploads); 

            //Creamos la ruta donde se escribira el archivo enviado
            var filePath = Path.Combine(uploads, audio.FileName);

            //En un Using (para cerrar todos los recursos iniciados en el Steam de forma intuitiva) guardamos el archivo enviado
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await audio.CopyToAsync(stream);
            }

            //Llamamos al metodo que ejecuta la transcripcion
            var result = await RunWhisper(filePath);

            //Borrar el archivo temporal una vez realizada la transcripción.
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            return Ok(new { texto = result });
        }

        //Metodo que ejecuta la transcripcion
        private async Task<string> RunWhisper(string audioPath)
        {
            //Definimos un proceso, un comando que nuestro Backend correra a nivel de consola para pedirle al proceso Whisper, de OpenAI, en local
            //que transcriba nuestro audio a texto
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"-m whisper \"{audioPath}\" --model tiny --language Spanish",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            //Ejecutamos el proceso en un Using
            using var process = new Process { StartInfo = psi };
            process.Start();

            //Extraemos la salida del proceso, y sus posibles errores
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            //Retornamos los errores en caso de ser necesario
            if (process.ExitCode != 0)
                return $"Error: {error}";

            //Retornamos la salida del proceso
            return output;
        }
    }
}
