using NAudio.Wave;
using System.Net;
using System.Net.Sockets;

public class UdpSenderService : BackgroundService
{
    private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();
    public bool IsRunning { get; private set; } = true;

    private UdpClient _udpClient;
    private WaveInEvent _waveIn;
    private IPEndPoint _endPoint;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string serverIP = "127.0.0.1"; // Sunucunun IP'si
        int serverPort = 5000;

        while (!stoppingToken.IsCancellationRequested)
        {
            if (IsRunning)
            {
                // UDP istemcisi ve mikrofonu başlat
                if (_udpClient == null)
                {
                    _udpClient = new UdpClient();
                    _endPoint = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);

                    // Mikrofon girişini başlat
                    _waveIn = new WaveInEvent
                    {
                        WaveFormat = new WaveFormat(8000, 16, 1) // 8 kHz, 16 bit, mono
                    };

                    // Mikrofon verisini okuduğumuzda bu veriyi UDP üzerinden sunucuya gönderiyoruz
                    _waveIn.DataAvailable += (sender, e) =>
                    {
                        _udpClient.Send(e.Buffer, e.BytesRecorded, _endPoint);
                        Console.WriteLine($"Veri gönderildi: {e.BytesRecorded} byte");
                    };

                    _waveIn.StartRecording();
                    await Console.Out.WriteLineAsync("UDP istemcisi başlatıldı. Mikrofon verisi gönderiliyor...");
                }
            }
            else
            {
                // Kaydı durdur ve kaynakları serbest bırak
                if (_waveIn != null)
                {
                    _waveIn.StopRecording();
                    _waveIn.Dispose();
                    _waveIn = null;
                }

                if (_udpClient != null)
                {
                    _udpClient.Dispose();
                    _udpClient = null;
                }

                await Console.Out.WriteLineAsync("Background service paused.");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    public void StopService()
    {
        IsRunning = false;
    }

    public void StartService()
    {
        IsRunning = true;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Console.Out.WriteLineAsync("Background service is stopping.");

        if (_waveIn != null)
        {
            _waveIn.StopRecording();
            _waveIn.Dispose();
            _waveIn = null;
        }

        if (_udpClient != null)
        {
            _udpClient.Dispose();
            _udpClient = null;
        }

        _stoppingCts.Cancel();
        return base.StopAsync(cancellationToken);
    }
}
