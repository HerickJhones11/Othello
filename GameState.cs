using System.Collections.Generic;

public class GameState
{
    public const int Rows = 8;
    public const int Cols = 8;

    public Player[,] Board { get; }
    public Dictionary<Player, int> DiscCount { get; }
    public Player CurrentPlayer { get; private set; }
    public bool GameOver { get; private set; }
    public Player Winner { get; private set; }
    public Dictionary<Position, List<Position>> LegalMoves { get; private set; }
    public GameState()
    {
        Board = new Player[Rows, Cols];
        Board[3, 3] = Player.White;
        Board[3, 4] = Player.Black;
        Board[4, 3] = Player.Black;
        Board[4, 4] = Player.Black;

        DiscCount = new Dictionary<Player, int>()
        {
            { Player.Black, 2 },
            { Player.White, 2 }
        };
        CurrentPlayer = Player.Black;
        LegalMoves = FindLegalMoves(CurrentPlayer);
    } 
    public bool MakeMove(Position pos, out MoveInfo moveInfo)
    {
        if (!LegalMoves.ContainsKey(pos))
        {
            moveInfo = null;
            return false;
        }
        Player movePlayer = CurrentPlayer;
        List<Position> outFlanked = LegalMoves[pos];

        Board[pos.Row, pos.Col] = movePlayer;
        FlipDiscs(outFlanked);
        UpdateDiscCount(movePlayer, outFlanked.Count);
        PassTurn();

        moveInfo = new MoveInfo { Player = movePlayer, Position = pos, OutFlanked = outFlanked };
        return true;
    }
    public IEnumerable<Position> OccupiedPositions()
    {
        for(int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                if (Board[r, c] != Player.None)
                {
                    yield return new Position(r, c);
                }
            }
        }
    }
    private void FlipDiscs(List<Position> positions)
    {
        foreach (var pos in positions)
        {
            Board[pos.Row, pos.Col] = Board[pos.Row, pos.Col].Opponent();
        }
    }
    private void UpdateDiscCount(Player movePlayer, int outFlankedCount)
    {
        DiscCount[movePlayer] += outFlankedCount + 1;
        DiscCount[movePlayer] += outFlankedCount - 1;
    }
    private void ChangePlayer()
    {
        CurrentPlayer = CurrentPlayer.Opponent();
        LegalMoves = FindLegalMoves(CurrentPlayer);
    }
    private Player FindWinner()
    {
        if (DiscCount[Player.Black] > DiscCount[Player.White])
        {
            return Player.Black;
        }
        if (DiscCount[Player.White] > DiscCount[Player.Black])
        {
            return Player.White;
        }
        return Player.None;
    }
    private void PassTurn()
    {
        ChangePlayer();
        if (LegalMoves.Count > 0)
        {
            return;
        }
        if (LegalMoves.Count == 0)
        {
            CurrentPlayer = Player.None;
            GameOver = true;
            Winner = FindWinner();
        }
    }
    private bool IsInsideBoard(int r,int c)
    {
        return r >= 0 &&  r < Rows && c >= 0 && c < Cols;
    }
    private List<Position> OutFlankedInDir(Position pos, Player player, int rDelta, int cDelta)
    {
        List<Position> outFlanked = new List<Position>();
        int r = pos.Row + rDelta;
        int c = pos.Col + cDelta;

        while(IsInsideBoard(r, c) && Board[r, c] != Player.None) 
        {
            if (Board[r, c] == player.Opponent())
            {
                outFlanked.Add(new Position(r, c));
                r += rDelta;
                c += cDelta;
            }
            else 
            {
                return outFlanked;
            }
        }
        return new List<Position>();
    }
    private List<Position> OutFlanked(Position pos, Player player)
    {
        List<Position> outFlanked = new List<Position>();
        
        for (int rDelta = -1; rDelta <= 1; rDelta++) 
        {
            for(int cDelta = -1; cDelta <= 1; cDelta++)
            {
                if(rDelta == 0 && cDelta == 0)
                {
                    continue;
                }
                outFlanked.AddRange(OutFlankedInDir(pos, player, rDelta, cDelta));
            }
        }
        return outFlanked;
    }
    private bool IsMoveLegal(Player player, Position pos, out List<Position> outFlanked)
    {
        if (Board[pos.Row, pos.Col] != Player.None)
        {
            outFlanked = null;
            return false;
        }
        outFlanked = OutFlanked(pos, player);
        return outFlanked.Count > 0;
    }
    private Dictionary<Position, List<Position>> FindLegalMoves(Player player)
    {
        Dictionary<Position, List<Position>> legalMoves = new Dictionary<Position, List<Position>>();

        for(int r = 0; r < Rows; r++)
        {
            for (int c = 0; c < Cols; c++)
            {
                Position pos = new Position(r, c);
                if(IsMoveLegal(player, pos, out List<Position> outFlanked))
                {
                    legalMoves[pos] = outFlanked;
                }
            }
        }
        return legalMoves;
    }
}
