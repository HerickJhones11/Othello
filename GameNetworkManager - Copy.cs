//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//using UnityEngine;
//using Newtonsoft.Json;
//using System.Security.Cryptography;

//public class GameNetworkManager2 : MonoBehaviour
//{
//    public static GameNetworkManager2 Instance { get; private set; } // Singleton

//    private TcpListener server;
//    private TcpClient client;
//    private NetworkStream stream;
//    private List<TcpClient> connectedClients = new List<TcpClient>(); // Lista de clientes conectados
//    public bool isServer = false;
//    public bool isClient = false;
//    public Player color;

//    public string ipAddress = "127.0.0.1"; // IP do servidor (localhost)
//    public int port = 12345;              // Porta de comunicação
//    private bool acceptingClients = true; // Controle de execução do loop de aceitação de clientes

//    public delegate void MessageReceivedEventHandler(PlayerData message);
//    public event MessageReceivedEventHandler OnMessageReceived;

//    private int requestIdCounter = 0;
//    private Dictionary<int, TaskCompletionSource<PlayerData>> responseTasks = new Dictionary<int, TaskCompletionSource<PlayerData>>();

//    // Método assíncrono para enviar uma mensagem e aguardar a resposta
//    public void SendMessageToServerAsync(PlayerData request)
//    {
//        int requestId = requestIdCounter++;
//        request.requestId = requestId;

//        // Cria uma TaskCompletionSource para esperar a resposta
//        var taskCompletionSource = new TaskCompletionSource<PlayerData>();
//        responseTasks[requestId] = taskCompletionSource;

//        // Serializa e envia a requisição
//        string jsonData = JsonConvert.SerializeObject(request);
//        byte[] data = Encoding.UTF8.GetBytes(jsonData);

//        if (stream != null && stream.CanWrite)
//        {
//            stream.Write(data, 0, data.Length);
//            Debug.Log("Sent request to server with requestId: " + requestId);
//        }
//    }


//    // Chamado quando os dados são recebidos


//    private void Awake()
//    {
//        if (Instance == null)
//        {
//            Instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else
//        {
//            Destroy(gameObject);
//            return;
//        }
//    }

//    public bool TryConnectAsClient()
//    {
//        try
//        {
//            client = new TcpClient();
//            client.Connect(ipAddress, port);
//            stream = client.GetStream();
//            Debug.Log("Successfully connected to server");
//            StartReceivingData(); // Inicia a recepção de dados
//            isClient = true;
//            color = Player.White;
//            if (isServer)
//            {
//                color = Player.Black;
//            }
//            return true;
//        }
//        catch (Exception)
//        {
//            Debug.Log("No server found, initializing as server");
//            StartServer();
//            isServer = true;
//            return false;
//        }
//    }

//    private void StartServer()
//    {
//        try
//        {
//            server = new TcpListener(IPAddress.Parse(ipAddress), port);
//            server.Start();
//            Debug.Log("Server started");
//            AcceptClients();
//        }
//        catch (Exception e)
//        {
//            Debug.LogError("Server error: " + e.Message);
//        }
//    }
//    private void AcceptClients()
//    {
//        Thread acceptThread = new Thread(() =>
//        {
//            while (acceptingClients)
//            {
//                try
//                {
//                    TcpClient newClient = server.AcceptTcpClient();
//                    Debug.Log("Client connected");
//                    connectedClients.Add(newClient);
//                    NetworkStream clientStream = newClient.GetStream();
//                    Thread clientThread = new Thread(() => HandleClient(newClient, clientStream));
//                    clientThread.Start();
//                }
//                catch (SocketException e)
//                {
//                    if (acceptingClients)
//                        Debug.LogError("Socket exception: " + e.Message);
//                }
//            }
//        });
//        acceptThread.Start();
//    }

//    public List<string> HandleJson(string receivedData)
//    {
//        List<string> jsonObjects = new List<string>();
//        int bracketCount = 0;
//        int startIndex = 0;

//        for (int i = 0; i < receivedData.Length; i++)
//        {
//            if (i == 1)
//            {
//                Console.WriteLine(receivedData);
//            }
//            if (receivedData[i] == '{')
//                bracketCount++;
//            else if (receivedData[i] == '}')
//                bracketCount--;

//            if (bracketCount == 0)
//            {
//                jsonObjects.Add(receivedData.Substring(startIndex, i - startIndex + 1));
//                startIndex = i + 1;
//            }
//        }
//        return jsonObjects;
//    }
//    private void HandleReceivedData(byte[] buffer, int bytesRead)
//    {
//        string jsonData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
//        var jsonList = HandleJson(jsonData).Last();
//        PlayerData response = JsonConvert.DeserializeObject<PlayerData>(jsonData);

//        if (response != null)
//        {
//            switch (response.request)
//            {
//                case "user_registry":
//                    UserRegistry(response);
//                    OnMessageReceived?.Invoke(response);
//                    break;
//                default:
//                    OnMessageReceived?.Invoke(response);
//                    break;
//            }
//        }

//        // Completa a Task associada ao requestId
//        if (responseTasks.TryGetValue(response.requestId, out var taskCompletionSource))
//        {
//            taskCompletionSource.SetResult(response); // Completa a Task com a resposta
//            responseTasks.Remove(response.requestId);  // Remove a Task do dicionário
//        }
//        else
//        {
//            Debug.LogWarning("No task found for requestId: " + response.requestId);
//        }
//    }

//    //
//    private void HandleClient(TcpClient client, NetworkStream clientStream)
//    {
//        byte[] buffer = new byte[1024];
//        int bytesRead;

//        while ((bytesRead = clientStream.Read(buffer, 0, buffer.Length)) != 0)
//        {
//            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
//            Debug.Log("Received from client: " + message);

//            PlayerData res = JsonConvert.DeserializeObject<PlayerData>(message);
//            if (res != null)
//            {
//                switch (res.request)
//                {
//                    case "user_registry":
//                        UserRegistry(res);
//                        OnMessageReceived?.Invoke(res);
//                        break;
//                    default:
//                        SendToOpponent(client, res);
//                        break;
//                }
//            }


//            string responseMessage = message; // Message received by server
//            byte[] response = Encoding.UTF8.GetBytes(responseMessage);
//            clientStream.Write(response, 0, response.Length);
//            Debug.Log("Response sent to client: " + responseMessage);
//        }

//        client.Close();
//        connectedClients.Remove(client);
//        Debug.Log("Client disconnected");
//    }
//    private void SendToOpponent(TcpClient senderClient, PlayerData message)
//    {
//        TcpClient opponentClient = GetOpponentClient(senderClient);
//        if (opponentClient != null && opponentClient.Connected)
//        {
//            // Serializa a mensagem para enviar ao oponente
//            string responseMessage = JsonConvert.SerializeObject(message);
//            byte[] data = Encoding.UTF8.GetBytes(responseMessage);

//            // Envia a mensagem para o cliente oponente
//            NetworkStream opponentStream = opponentClient.GetStream();
//            opponentStream.Write(data, 0, data.Length);
//            Debug.Log("Message forwarded to opponent: " + responseMessage);
//        }
//        else
//        {
//            var response = new PlayerData()
//            {
//                response = "disconnected"
//            };
//            OnMessageReceived?.Invoke(response);
//        }
//    }
//    private TcpClient GetOpponentClient(TcpClient client)
//    {
//        // Procura pelo cliente oponente (assumindo que você tem apenas dois clientes)
//        return connectedClients.FirstOrDefault(c => c != client);
//    }

//    public void SendPlayerData(PlayerData playerData)
//    {
//        // Converte o objeto para JSON
//        string jsonData = JsonConvert.SerializeObject(playerData);
//        byte[] data = Encoding.UTF8.GetBytes(jsonData);

//        // Envia os dados pela rede (supondo que você tenha uma conexão de rede estabelecida)
//        if (stream != null && stream.CanWrite)
//        {
//            stream.Write(data, 0, data.Length);
//            Debug.Log("Sent JSON data to server: " + jsonData);
//        }
//    }

//    private void UserRegistry(PlayerData playerData)
//    {
//        playerData.response = JsonConvert.SerializeObject(color);
//    }

//    private void StartReceivingData()
//    {
//        Thread receiveThread = new Thread(() =>
//        {
//            byte[] buffer = new byte[1024];
//            int bytesRead;

//            try
//            {
//                while (client != null && client.Connected)
//                {
//                    if (stream != null && stream.DataAvailable)
//                    {
//                        bytesRead = stream.Read(buffer, 0, buffer.Length);
//                        if (bytesRead > 0)
//                        {
//                            HandleReceivedData(buffer, bytesRead);
//                        }
//                    }
//                    Thread.Sleep(100);
//                }
//            }
//            catch (Exception e)
//            {
//                Debug.LogError("Error receiving data: " + e.Message);
//            }
//        });
//        receiveThread.Start();
//    }

//    private void OnApplicationQuit()
//    {
//        if (isServer)
//        {
//            acceptingClients = false;
//            server.Stop();
//            Debug.Log("Server stopped");
//        }

//        if (isClient && client != null)
//        {
//            stream.Close();
//            client.Close();
//            Debug.Log("Disconnected from server");
//        }
//    }
//}
