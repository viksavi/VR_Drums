using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;
using UnityEngine;

public class UDPForceReceiver : MonoBehaviour
{
    public int port = 5555;

    public static float CurrentForce { get; private set; }

    // Fired on the main thread whenever a new force value arrives (0-1 normalized).
    public static event Action<float> OnForceReceived;

    private UdpClient client;
    private Thread receiveThread;
    private volatile bool running;

    private volatile bool hasNewValue;
    private float pendingValue;

    void Start()
    {
        client = new UdpClient(port);
        running = true;
        receiveThread = new Thread(ReceiveLoop) { IsBackground = true };
        receiveThread.Start();
    }

    private void ReceiveLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        while (running)
        {
            try
            {
                byte[] data = client.Receive(ref remoteEP);
                string text = System.Text.Encoding.UTF8.GetString(data);
                if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float percent))
                {
                    float normalized = Mathf.Clamp01(percent / 100f);
                    CurrentForce = normalized;
                    pendingValue = normalized;
                    hasNewValue = true;
                }
            }
            catch (SocketException)
            {
                // Thrown when the socket is closed on stop; safe to ignore.
            }
        }
    }

    void Update()
    {
        // Dispatch on the main thread — Unity APIs (and most event subscribers)
        // aren't safe to call directly from the background receive thread.
        if (hasNewValue)
        {
            hasNewValue = false;
            OnForceReceived?.Invoke(pendingValue);
        }
    }

    void OnDestroy()
    {
        running = false;
        client?.Close();
        receiveThread?.Join(200);
    }
}