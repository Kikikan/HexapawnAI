using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace HexapawnAI
{
    public partial class MainWindow : Window
    {
        private int defFontSize = 30;
        private int selFontSize = 40;

        internal string playerName = "You";
        internal string aiName = "AI";

        private bool end = false;

        public Button[,] Buttons = new Button[3, 3];
        public static Matrix CurrentMatrix;

        private int[,] board; 
        public int[,] Board {
            get
            {
                return board;
            }
            set
            {
                for (int i = 0; i < value.GetLength(0); i++)
                {
                    for (int j = 0; j < value.GetLength(1); j++)
                    {
                        if (value[i, j] == 1)
                            Buttons[i, j].Content = aiName;
                        else if (value[i, j] == -1)
                            Buttons[i, j].Content = playerName;
                        else
                            Buttons[i, j].Content = "";
                        Buttons[i, j].FontSize = defFontSize;
                    }
                }
                board = value;
            }
        }
        private Coordinate from;
        private Coordinate From
        {
            get { return from; }
            set
            {
                if (value != null)
                    Buttons[value.X, value.Y].FontSize = selFontSize;
                else if (from != null)
                    Buttons[from.X, from.Y].FontSize = defFontSize;
                from = value;
            }
        }

        private Move lastMove;

        public static List<Move> Forbidden = new List<Move>();
        private static ConsoleWindow CW = new ConsoleWindow();

        public MainWindow()
        {
            InitializeComponent();

            Loading();
        }

        private void Loading()
        {
            Buttons[0, 0] = A1;
            Buttons[0, 1] = A2;
            Buttons[0, 2] = A3;
            Buttons[1, 0] = B1;
            Buttons[1, 1] = B2;
            Buttons[1, 2] = B3;
            Buttons[2, 0] = C1;
            Buttons[2, 1] = C2;
            Buttons[2, 2] = C3;
            CW.Show();

            Start();
        }

        public static void Write(object data)
        {
            CW.Console.Text += data.ToString() + "\n";
        }

        public void Start()
        {
            end = false;
            lastMove = null;
            Board = new int[,] { { -1, 0, 1 }, { -1, 0, 1 }, { -1, 0, 1 } };
            From = null;
            CW.Console.Text = "";
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (end)
                return;
            for (int i = 0; i < Buttons.GetLength(0); i++)
            {
                for (int j = 0; j < Buttons.GetLength(1); j++)
                {
                    if (sender == Buttons[i,j])
                    {
                        if (From == null)
                        {
                            if (Board[i, j] == -1)
                            {
                                From = new Coordinate(i, j);
                            }
                        }
                        else if ((Board[i, j] == 1 && (i + 1 == From.X || i - 1 == From.X) && j == From.Y + 1) || (Board[i, j] == 0 && From.Y + 1 == j && From.X == i))
                        {
                            Move(i, j);
                            From = null;
                        }
                        else
                        {
                            From = null;
                        }
                            
                    }
                }
            }
            
        }

        private void Move(int x, int y)
        {
            Board[From.X, From.Y] = 0;
            Board[x, y] = -1;
            Buttons[From.X, From.Y].Content = "";
            Buttons[x, y].Content = playerName;

            int ai = 0;
            int p = 0;
            foreach (var i in Board)
            {
                if (i == 1)
                    ai++;
                else if (i == -1)
                    p++;
            }

            if (ai == 0)
                Win();
            else if (p == 0)
                Lose();
            else if (y == 2)
                Win();
            else
                AIMove();
                
        }

        private void AIMove()
        {
            CurrentMatrix = new Matrix(Board);
            if (CurrentMatrix.Possibilities.Count == 0)
            {
                Lose();
                return;
            }
            var p = CurrentMatrix.Move();
            lastMove = p;

            Board[p.From.X, p.From.Y] = 0;
            Board[p.To.X, p.To.Y] = 1;
            Buttons[p.From.X, p.From.Y].Content = "";
            Buttons[p.To.X, p.To.Y].Content = aiName;
            CurrentMatrix = new Matrix(Board);
            int pp = 0;
            foreach (var i in Board)
            {
                if (i == -1)
                    pp++;
            }
            if (p.To.Y == 0)
                Lose();
            else if (CurrentMatrix.Possibilities.Count == 0)
                Lose();
            else if (pp == 0)
                Lose();
        }

        private void Win()
        {
            end = true;
            Forbidden.Add(lastMove);
            Write($"Move added:\n{lastMove.Board[0, 2]} {lastMove.Board[1, 2]} {lastMove.Board[2, 2]}\n{lastMove.Board[0, 1]} {lastMove.Board[1, 1]} {lastMove.Board[2, 1]}\n{lastMove.Board[0, 0]} {lastMove.Board[1, 0]} {lastMove.Board[2, 0]}");
            CW.Status.Text += "W ";
            VictoryWindow w = new VictoryWindow(this);
            w.ResultText.Text = "Victory Royale!";
            w.Title = "You won!";
            w.Show();
        }

        internal void Lose()
        {
            end = true;
            CW.Status.Text += "L ";
            VictoryWindow w = new VictoryWindow(this);
            w.ResultText.Text = "Game Over!";
            w.Title = "You lost!";
            w.Show();
        }
    }

    public class Matrix
    {
        public int[,] Board = new int[3, 3];
        public List<Coordinate> Pawns = new List<Coordinate>();
        public List<Move> Possibilities = new List<Move>();

        public Matrix(int[,] b)
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Board[i, j] = b[i, j];
                }
            }
            for (int i = 0; i < b.GetLength(0); i++)
            {
                for (int j = 0; j < b.GetLength(1); j++)
                {
                    if (b[i,j] == 1)
                    {
                        Pawns.Add(new Coordinate(i, j));
                    }
                }
            }
            genMoves();
        }

        private void genMoves()
        {
            foreach (var pawn in Pawns)
            {
                if (pawn.Y == 0)
                    return;
                if (Board[pawn.X, pawn.Y - 1] == 0)
                    Possibilities.Add(new Move(Board, new Coordinate(pawn.X, pawn.Y), new Coordinate(pawn.X, pawn.Y - 1)));

                switch(pawn.X)
                {
                    case 0:
                        if (Board[pawn.X + 1, pawn.Y - 1] == -1)
                            Possibilities.Add(new Move(Board, new Coordinate(pawn.X, pawn.Y), new Coordinate(pawn.X + 1, pawn.Y - 1)));
                        break;
                    case 1:
                        if (Board[pawn.X + 1, pawn.Y - 1] == -1)
                            Possibilities.Add(new Move(Board, new Coordinate(pawn.X, pawn.Y), new Coordinate(pawn.X + 1, pawn.Y - 1)));

                        if (Board[pawn.X - 1, pawn.Y - 1] == -1)
                            Possibilities.Add(new Move(Board, new Coordinate(pawn.X, pawn.Y), new Coordinate(pawn.X - 1, pawn.Y - 1)));
                        break;
                    case 2:
                        if (Board[pawn.X - 1, pawn.Y - 1] == -1)
                            Possibilities.Add(new Move(Board, new Coordinate(pawn.X, pawn.Y), new Coordinate(pawn.X - 1, pawn.Y - 1)));
                        break;
                }
            }

            var temp = new List<Move>();
            foreach (var item in Possibilities)
            {
                temp.Add(item);
            }
            foreach (var move in temp)
            {
                foreach (var fmove in MainWindow.Forbidden)
                {
                    if (move.Equal(fmove))
                    {
                        Possibilities.Remove(move);
                    }
                        
                }
            }
        }

        public Move Move()
        {
            var r = new Random();
            return Possibilities[r.Next(Possibilities.Count)];
        }
    }

    public class Move
    {
        public int[,] Board;
        public Coordinate From;
        public Coordinate To;

        public Move(int[,] b, Coordinate f, Coordinate w)
        {
            Board = b;
            From = f;
            To = w;
        }

        public bool Equal(Move move)
        {
            if (From.Equal(move.From) && To.Equal(move.To))
            {
                int c = 0;
                for (int i = 0; i < 3; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (move.Board[i,j] == Board[i,j])
                        {
                            c++;
                        }
                    }
                }                

                if (c == 9)
                {
                    MainWindow.Write("Move detected in 'Forbidden' list. Move deleted to ensure winning.");
                    return true;
                }
                    
            }  
            return false;
                
        }
    }

    public class Coordinate
    {
        public int X;
        public int Y;

        public Coordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public bool Equal(Coordinate c)
        {
            if (X == c.X && Y == c.Y)
                return true;
            else
                return false;
        }
    }
}
