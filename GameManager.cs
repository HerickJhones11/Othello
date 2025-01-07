using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Multiplayer.Center.Common;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEditor;
using Newtonsoft.Json;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    ScrollRect scrollView;

    [SerializeField]
    GameObject chatPanel, textObject;

    [SerializeField]
    public InputField chatBox;

    [SerializeField]
    List<Message> messageList = new List<Message>();

    [SerializeField]
    private Camera cam;

    [SerializeField]
    private Disc discBlackUp;

    [SerializeField]
    private Disc discWhiteUp;

    [SerializeField]
    private GameObject highLightPrefab;

    [SerializeField]
    private UIManager uiManager;

    [SerializeField]
    private GameNetworkManager gameNetworkManager;

    [SerializeField]
    public Color playerMessage, info;

    [SerializeField]
    public string username;

    private Dictionary<Player, Disc> discPrefabs = new Dictionary<Player, Disc>();
    private GameState gameState = new GameState();
    private Disc[,] discs = new Disc[8, 8];
    private List<GameObject> highlights = new List<GameObject>();

    PlayerData response = null;

    bool myTurn = false;
    public Player color;
    public bool isClient;

    private void Start()
    {
        isClient = gameNetworkManager.TryConnectAsClient();
        if (!isClient)
        {
            gameNetworkManager.TryConnectAsClient();
        }

        gameNetworkManager.OnMessageReceived += HandleServerMessage;

        //SendMessageToChat("aqui1", Message.MessageType.info);


        gameNetworkManager.SendMessageToServerAsync(new PlayerData { request = "user_registry" });

        // Processa a resposta
        Debug.Log("Received response: " + response);


        discPrefabs[Player.Black] = discBlackUp;
        discPrefabs[Player.White] = discWhiteUp;

        AddStartsDiscs();
        uiManager.SetPlayerText(gameState.CurrentPlayer);
        //StartCoroutine(ShowBlackScreen(moveInfo));
        if (!isClient)
            StartCoroutine(ShowLobbyText("Aguarde o pr�ximo jogador \n entrarr"));


    }
    private void HandleServerMessage(PlayerData message)
    {
        response = message;
    }
    private void UserRegistry(PlayerData message)
    {
        color = JsonConvert.DeserializeObject<Player>(message.response);
        if (color == Player.Black)
        {
            myTurn = true;
        }
        else
        {
            gameNetworkManager.SendMessageToServerAsync(new PlayerData { request = "start_game" });
            gameStarted = true;
        }
        uiManager.SetPlayerColor(color);
    }
    private void UpdateOpponentBoard(PlayerData message)
    {
        if (color != message.Color)
        {
            OnBoardClicked(message.boardPos);
            ShowLegalMoves();
            myTurn = true;
        }
    }
    private void StartGame(PlayerData message)
    {
        gameStarted = true;
        if (!isClient)
        {
            StartCoroutine(uiManager.HideBlackScreen());
            ShowLegalMoves();
        }
    }
    private void UpdateOpponentChat(PlayerData message)
    {
        if (color != message.Color)
        {
            SendMessageToChat(message.Color.StringColor() + ": " + message.message, Message.MessageType.playerMessage);
        }
    }
    private void HandleResponse()
    {
        //SendMessageToChat("aqui2", Message.MessageType.info);

        switch (response.request)
        {
            case "user_registry":
                UserRegistry(response);
                break;
            case "player_move":
                UpdateOpponentBoard(response);
                break;
            case "player_message":
                UpdateOpponentChat(response);
                break;
            case "start_game":
                StartGame(response);
                break;
        }
    }
    private float timer = 0f; // Temporizador
    private bool gameStarted;

    private void Update()
    {
        if (!myTurn)
        {
            timer += Time.deltaTime; // Incrementa o temporizador com o tempo do frame
        }
        if (timer >= 15f && gameStarted)
        {
            try
            {
                gameNetworkManager.SendMessageToServerAsync(new PlayerData { request = "check_connection" });
            }
            catch (Exception e)
            {
                StartCoroutine(ShowLobbyText("O outro jogador  se \ndesconectou"));
            }
            timer = 0f;
        }

        if (response != null)
        {
            if (response.response == "disconnected")
            {
                StartCoroutine(ShowLobbyText("O outro jogador  se \ndesconectou"));
            }
            HandleResponse();
            response = null;
        }

        if (chatBox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {

                PlayerData message = new PlayerData()
                {
                    request = "player_message",
                    message = chatBox.text,
                    Color = color
                };

                gameNetworkManager.SendMessageToServerAsync(message);
                SendMessageToChat(color.StringColor() + ": " + chatBox.text, Message.MessageType.playerMessage);
                chatBox.text = "";
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Vector3 impact = hitInfo.point;
                Position boardPos = SceneToBoardPos(impact);
                OnBoardClicked(boardPos);
                PlayerData move = new PlayerData()
                {
                    Color = color,
                    request = "player_move",
                    boardPos = boardPos
                };

                myTurn = false;
                gameNetworkManager.SendMessageToServerAsync(move);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            SendMessageToChat("You pressed the space bar", Message.MessageType.info);
            Debug.Log("Space");
        }

    }
    private IEnumerator WaitTime(float time)
    {
        yield return new WaitForSeconds(time);
    }





    private void OnDestroy()
    {
        if (gameNetworkManager != null)
        {
            gameNetworkManager.OnMessageReceived -= HandleServerMessage;
        }
    }

    private void ShowLegalMoves()
    {
        foreach (Position boardPos in gameState.LegalMoves.Keys)
        {
            Vector3 scenePos = BoardToScenePos(boardPos) + Vector3.up * 0.01f;
            GameObject highLight = Instantiate(highLightPrefab, scenePos, Quaternion.identity);
            highlights.Add(highLight);
        }
    }

    private void HideLegalMoves()
    {
        highlights.ForEach(Destroy);
        highlights.Clear();
    }
    private void UpdateBoardWithOpponentMove(PlayerData moveData)
    {
        // Atualiza o estado do tabuleiro com a jogada recebida
    }
    private void ProcessOpponentMove(PlayerData opponentMove)
    {
        Debug.Log("Received opponent's move: " + opponentMove);
        // Atualiza o tabuleiro com a jogada do oponente
        UpdateBoardWithOpponentMove(opponentMove);
    }
    private async void OnBoardClicked(Position boardPos)
    {
        if (gameState.MakeMove(boardPos, out MoveInfo moveInfo))
        {
            HideLegalMoves();
            StartCoroutine(OnMoveMade(moveInfo));
            //ShowLegalMoves();
        }
    }

    //private IEnumerator SetPlayer()
    //{
    //    gameNetworkManager.WaitForConnection();
    //}
    private IEnumerator OnMoveMade(MoveInfo moveInfo)
    {
        HideLegalMoves();
        yield return ShowMove(moveInfo);
        yield return ShowTurnOutcome(moveInfo);
        //ShowLegalMoves();
    }

    private Position SceneToBoardPos(Vector3 scenePos)
    {
        int col = (int)(scenePos.x - 0.25f);
        int row = 7 - (int)(scenePos.z - 0.25f);

        return new Position(row, col);
    }
    private Vector3 BoardToScenePos(Position boardPos)
    {
        //var row = boardPos.Row;
        //var col = boardPos.Col;
        //var borda = 0.25f;
        //var centro = 0.5f;

        //var topoTabuleiro = 8 + 2 * borda;

        //var x = borda + col + centro;
        //var z = topoTabuleiro - borda - row - centro;
        //return new Vector3(x,0, z);
        return new Vector3(boardPos.Col + 0.75f, 0, 7 - boardPos.Row + 0.75f);
    }

    private void SpawnDisc(Disc prefab, Position boardPos)
    {
        Vector3 scenePos = BoardToScenePos(boardPos) + Vector3.up * 0.1f;
        discs[boardPos.Row, boardPos.Col] = Instantiate(prefab, scenePos, Quaternion.identity);
    }
    private void AddStartsDiscs()
    {
        foreach (Position boardPos in gameState.OccupiedPositions())
        {
            Player player = gameState.Board[boardPos.Row, boardPos.Col];
            SpawnDisc(discPrefabs[player], boardPos);
        }
    }
    private void FlipDiscs(List<Position> positions)
    {
        foreach (Position boardPos in positions)
        {
            discs[boardPos.Row, boardPos.Col].Flip();
        }
    }
    private IEnumerator ShowMove(MoveInfo moveInfo)
    {
        SpawnDisc(discPrefabs[moveInfo.Player], moveInfo.Position);
        yield return new WaitForSeconds(0.33f);
        FlipDiscs(moveInfo.OutFlanked);
        yield return new WaitForSeconds(0.83f);
    }
    private IEnumerator ShowTurnSkipped(Player skippedPlayer)
    {
        uiManager.SetSkippedText(skippedPlayer);
        yield return uiManager.AnimateTopText();
    }
    private IEnumerator ShowGameOver(Player winner)
    {
        uiManager.SetTopText("Nenhum jogador pode se mover");
        yield return uiManager.AnimateTopText();

        yield return uiManager.ShowScoreText();
        yield return new WaitForSeconds(0.5f);

        yield return ShowCounting();
        uiManager.SetWinnerText(winner);
        yield return uiManager.ShowEndScreen();
    }
    private IEnumerator ShowLobbyText(string text)
    {
        yield return uiManager.ShowLobbyText(text);
    }
    private IEnumerator ShowBlackScreen()
    {
        yield return uiManager.ShowBlackScreen();
    }
    private IEnumerator ShowTurnOutcome(MoveInfo moveInfo)
    {
        if (gameState.GameOver)
        {
            yield return ShowGameOver(gameState.Winner);
            yield break;
        }
        Player currentPlayer = gameState.CurrentPlayer;
        if (currentPlayer == moveInfo.Player)
        {
            yield return ShowTurnSkipped(currentPlayer.Opponent());
        }
        uiManager.SetPlayerText(currentPlayer);
    }
    private IEnumerator ShowCounting()
    {
        int black = 0, white = 0;
        foreach (Position pos in gameState.OccupiedPositions())
        {
            Player player = gameState.Board[pos.Row, pos.Col];
            if (player == Player.Black)
            {
                black++;
                uiManager.SetBlackScoreText(black);
            }
            else
            {
                white++;
                uiManager.SetWhiteScoreText(white);
            }
            discs[pos.Row, pos.Col].Twitch();
            yield return new WaitForSeconds(0.05f);
        }
    }
    private IEnumerator RestartGame()
    {
        yield return uiManager.HideEndScreen();
        Scene activeScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(activeScene.name);
    }
    public void OnPlayAgainClicked()
    {
        StartCoroutine(RestartGame());
    }
    Color MessageTypeColor(Message.MessageType messageType)
    {
        Color color = info;
        switch (messageType)
        {
            case Message.MessageType.playerMessage:
                color = playerMessage;
                break;
        }
        return color;
    }
    public void SendMessageToChat(string message, Message.MessageType messageType)
    {
        Message newMessage = new Message();
        newMessage.text = message;

        GameObject newText = Instantiate(textObject, chatPanel.transform);

        newMessage.textObject = newText.GetComponent<Text>();

        newMessage.textObject.text = newMessage.text;
        newMessage.textObject.color = MessageTypeColor(messageType);

        messageList.Add(newMessage);
        ScrollToBottom();
    }
    public void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollView.verticalNormalizedPosition = 0f;
    }
}

