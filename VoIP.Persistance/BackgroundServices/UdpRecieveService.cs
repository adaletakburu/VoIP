using NAudio.Wave;
using System.Net;
using System.Net.Sockets;

namespace VoIP.Persistance.BackgroundServices
{
    public class UdpRecieveService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            int listenPort = 5000;

            using (UdpClient udpClient = new UdpClient(listenPort))
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, listenPort);
                Console.WriteLine($"UDP sunucusu {listenPort} portunda dinliyor...");

                // Ses verisini hoparlörde çalmak için WaveOut ve BufferedWaveProvider kullanıyoruz
                using (var waveOut = new WaveOutEvent())
                {
                    var bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(8000, 16, 1)); // 8kHz, 16bit, mono
                    waveOut.Init(bufferedWaveProvider);
                    waveOut.Play();

                    while (!stoppingToken.IsCancellationRequested)
                    {
                        // UDP paketini asenkron olarak al
                        UdpReceiveResult result = await udpClient.ReceiveAsync(stoppingToken);
                        byte[] receivedBytes = result.Buffer;
                        await Console.Out.WriteLineAsync($"Veri alındı: {receivedBytes.Length} byte");

                        // Alınan ses verisini hoparlörde oynat
                        bufferedWaveProvider.AddSamples(receivedBytes, 0, receivedBytes.Length);
                    }

                    waveOut.Stop(); // Servis durdurulduğunda çalmayı durdur
                }
            }
        }
    }
}
