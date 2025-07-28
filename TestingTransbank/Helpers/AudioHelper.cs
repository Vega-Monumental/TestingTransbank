using NAudio.Wave;

namespace TestingTransbank.Helper
{

    public static class AudioPaths
    {

        public const string Error = "Audios/ERROR.mp3";

        public const string TicketAprobado = "Audios/TICKET_APROBADO.mp3";

        public const string TicketNoPagado = "Audios/TICKET_NO_PAGADO.mp3";

        public const string TicketNoValido = "Audios/TICKET_NO_VALIDO.mp3";

        public const string TicketVencido = "Audios/TICKET_VENCIDO.mp3";

    }

    public static class AudioHelper
    {

        public static void PlayAudio(string audioPath)
        {

            using (var audioFile = new AudioFileReader(audioPath))

            using (var outputDevice = new WaveOutEvent())
            {

                outputDevice.Init(audioFile);

                outputDevice.Play();

                // Esperar hasta que termine la reproducción
                while (outputDevice.PlaybackState == PlaybackState.Playing)
                {

                    System.Threading.Thread.Sleep(1000);

                }

            }

        }

    }

}
