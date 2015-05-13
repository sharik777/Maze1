using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;


namespace Labyrinth
{
    public partial class Form1 : Form
    {
        private String MazesPath;
        private string TilesetsPath;
        private string MusicPath;
        static int W, H, startX, startY, endX, endY;
        private int[,] LeeGrid;
        static int[,] Grid;
        int[] Px;
        int[] Py;
        static int Steps;
        int Len;
        Player human;
        bool InGame = false;
        
        int TileSize;
        Image WallTile;
        Image FloorTile;
        Image PlayerTile;
        Image ScreenView;
        Point ScreenViewPosition = new Point(0,0);

        System.Media.SoundPlayer MusicPlayer;
        String MenuMusic;
        String GameMusic;
        String VictoryMusic;
        static bool SoundOn = true;

        public class Player
        {
            public int X;
            public int Y;
            public PictureBox PlayerImage;
            public Player()
            {

                PlayerImage = new PictureBox();
                PlayerImage.Size = new Size(50, 50);
                PlayerImage.Location = new Point(0, 0);
                PlayerImage.BackColor = Color.BlueViolet;

                X = 0;
                Y = 0;
            }

            public void Move(String Arrow, Form1 obj)
            {
                switch (Arrow)
                {
                    case "Left":
                        if (X > 0 && Grid[Y, X - 1] != 1)
                        {
                            Steps++;
                            X -= 1;
                        }
                        break;
                    case "Right":
                        if (X < W - 1 && Grid[Y, X + 1] != 1)
                        {
                            Steps++;
                            X += 1;
                        }
                        break;
                    case "Up":
                        if (Y > 0 && Grid[Y - 1, X] != 1)
                        {
                            Steps++;
                            Y -= 1;
                        }
                        break;
                    case "Down":
                        if (Y < H - 1 && Grid[Y + 1, X] != 1)
                        {
                            Steps++;
                            Y += 1;
                        }
                        break;
                }
                if (X == endX && Y == endY)
                {
                    obj.ShowEndMessage();
                }
            }
        }

        public Form1()
        {
            InitializeComponent();
            MazesPath = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - 14) + @"\Mazes\";
            TilesetsPath = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - 14) + @"\Tilesets\";
            MusicPath = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - 14) + @"\Music\";
            RefreshMazesList();
            RefreshTilesetsList();
            RefreshMusicList();
        }

        public void RefreshMusicList()
        {
            MenuMusic = MusicPath + "Menu.wav";
            GameMusic = MusicPath + "BGM.wav";
            VictoryMusic = MusicPath + "Victory.wav";
        }

        public void RefreshMazesList()
        {
            if (Directory.Exists(MazesPath))
            {
                DirectoryInfo dir = new DirectoryInfo(MazesPath);
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo temp in files)
                {
                    if (temp.Name.StartsWith("maze_"))
                    {
                        MazesComboBox.Items.Add(temp.Name);
                    }
                }
            }
        }
            

        public void RefreshTilesetsList()
        {
            if (Directory.Exists(TilesetsPath))
            {
                DirectoryInfo dir = new DirectoryInfo(TilesetsPath);
                DirectoryInfo[] sets = dir.GetDirectories();
                foreach (DirectoryInfo temp in sets)
                {
                    TilesetsBox.Items.Add(temp.Name);
                }
            }
            TilesetsBox.SelectedIndex = 0;
        }

        private void LoadTileSet(string Path)
        {
            WallTile = new Bitmap(Path + "Wall.png");
            FloorTile = new Bitmap(Path + "Floor.png");
            PlayerTile = new Bitmap(Path + "Player.png");
            TileSize = WallTile.Height;
        }

        private bool Lee(int Ax, int Ay, int Bx, int By)
        {
            LeeGrid = new int[H, W];
            const int Wall = -1;
            const int Blank = -2;
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    if (Grid[i, j] == 1)
                    {
                        LeeGrid[i, j] = Wall;
                    }
                    if (Grid[i, j] == 0)
                    {
                        LeeGrid[i, j] = Blank;
                    }
                }
            }
            Px = new int[W * H];
            Py = new int[W * H];
            int[] dx = { 1, 0, -1, 0 };
            int[] dy = { 0, 1, 0, -1 };
            int d, x, y, k;
            bool Stop;
            d = 0;
            LeeGrid[Ay, Ax] = 0;
            do 
            {
                Stop = true;
                for (y = 0; y < H; ++y)
                {
                    for (x = 0; x < W; ++x)
                    {
                        if (LeeGrid[y, x] == d)
                        {
                            for (k = 0; k < 4; ++k)
                            {
                                try
                                {
                                    if (LeeGrid[y + dy[k], x + dx[k]] == Blank)
                                    {
                                        Stop = false;
                                        LeeGrid[y + dy[k], x + dx[k]] = d + 1;
                                    }
                                }
                                catch (Exception e)
                                {
                                    String temp = e.Message;
                                }
                            }
                        }
                    }
                }
                d++;
            } while (!Stop && LeeGrid[By,Bx] == Blank);
            if (LeeGrid[By, Bx] == Blank)
            {
                return false;
            }
            Len = LeeGrid[By, Bx];
            x = Bx;
            y = By;
            d = Len;
            while (d > 0)
            {
                Px[d] = x;
                Py[d] = y;
                d--;
                for (k = 0; k < 4; ++k)
                {
                    try 
                    {
                        if (LeeGrid[y + dy[k], x + dx[k]] == d)
                        {
                            x = x + dx[k];
                            y = y + dy[k];
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        String temp = e.Message;
                    }
                }
            }
            Px[0] = Ax;
            Py[0] = Ay;
            return true;
            
        }

        bool Scaled = true;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (InGame)
            {
                if (Scaled)
                {
                    e.Graphics.DrawImage(RefreshView(), (this.Width - this.Height * W / H) / 2, 0, this.Height * W / H, this.Height);
                }
                else
                {
                    e.Graphics.DrawImage(RefreshView(), ScreenViewPosition.X, ScreenViewPosition.Y);
                }
            }
        }

        public Image RefreshView()
        {
            ScreenView = new Bitmap(W * TileSize, H * TileSize);
            var canvas = Graphics.FromImage(ScreenView);
            canvas.Clear(Color.Black);
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    if (Grid[i, j] == 0)
                    {
                        canvas.DrawImage(FloorTile, j * TileSize, i * TileSize, TileSize, TileSize);
                    }
                    if (Grid[i, j] == 1)
                    {
                        canvas.DrawImage(WallTile, j * TileSize, i * TileSize, TileSize, TileSize);
                    }
                }
            }
            canvas.DrawImage(PlayerTile, human.X * TileSize, human.Y * TileSize, TileSize, TileSize);
            canvas.Save();
            return ScreenView;
        }

        public void DrawMaze()
        {
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    if (Grid[i, j] == 1)
                    {
                        Label temp = new Label();
                        temp.AutoSize = false;
                        temp.Size = new Size(50, 50);
                        temp.BackColor = Color.Brown;
                        temp.Location = new Point(j * 50, i * 50);
                        Controls.Add(temp);
                    }
                }
            }
            this.Size = new Size(10+50*W, 35+50*H);
        }

        public void load_maze()
        {
            if (MazesComboBox.Text.StartsWith("maze_"))
            {
                StreamReader MazeFile = new StreamReader(MazesPath + MazesComboBox.Text);
                String TempLine;
                W = Convert.ToInt16(MazeFile.ReadLine());
                H = Convert.ToInt16(MazeFile.ReadLine());
                Grid = new int[H, W];
                for (int i = 0; i < H; i++)
                {
                    TempLine = MazeFile.ReadLine();
                    for (int j = 0; j < W; j++)
                    {
                        Grid[i, j] = int.Parse(TempLine[j].ToString());
                    }
                }
                startX = Convert.ToInt16(MazeFile.ReadLine());
                startY = Convert.ToInt16(MazeFile.ReadLine());
                endX = Convert.ToInt16(MazeFile.ReadLine());
                endY = Convert.ToInt16(MazeFile.ReadLine());
                MazeFile.Close();
                human = new Player();
                human.X = startX;
                human.Y = startY;
                Invalidate();
            }
        }

        private void LoadButton_Click(object sender, EventArgs e)
        {

            if (File.Exists(MazesPath + MazesComboBox.Text))
            {
                LoadTileSet(TilesetsPath + TilesetsBox.SelectedItem.ToString() + @"\");
                load_maze();
                CoverControls();
                Steps = 0;
            }
            
        }
        int helpscore = 0;
        public void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            
            if (e.KeyCode == Keys.H)
                helpscore += 50; //штраф за использование подсказки
            if (InGame)
            {
                switch (e.KeyCode)
                {
                    case Keys.Tab:
                        Scaled = !Scaled;
                        break;
                    case Keys.W:
                        human.Move("Up", this);
                        break;
                    case Keys.S:
                        human.Move("Down", this);
                        break;
                    case Keys.A:
                        human.Move("Left", this);
                        break;
                    case Keys.D:
                        human.Move("Right", this);
                        break;
                    case Keys.H:
                        HelpMe();
                        break;
                    case Keys.Q:
                        //ShowEndMessage();
                        Application.Exit();
                        break;
                }
                Invalidate();
            }
            
        }

        public void ShowEndMessage()
        {
            int SummScore = 0;

            SummScore += W * H * 10 - Steps - helpscore;
            EndGameDialog myEndGameForm = new EndGameDialog(SummScore);
            MusicPlayer = new System.Media.SoundPlayer(VictoryMusic);
            MusicPlayer.PlayLooping();
            myEndGameForm.ShowDialog(this);
            if (myEndGameForm.DialogResult == System.Windows.Forms.DialogResult.No)
            {
                Application.Exit();
            }
            else
            {
                //НОВАЯ ИГРА С БОЛЬШИМ ЛАБИРИНТОМ
                HeightNumeric.Value += 2;
                WidthNumeric.Value += 2;
                button1_Click(this, EventArgs.Empty);
            }
        }

        public void HelpMe()
        {
            if (Lee(human.X, human.Y, endX, endY) == true)
            {
                int i = 1;
                if (Px[i] > human.X && Py[i] == human.Y)
                {
                    human.Move("Right", this);
                }
                if (Px[i] < human.X && Py[i] == human.Y)
                {
                    human.Move("Left", this);
                }
                if (Py[i] > human.Y && Px[i] == human.X)
                {
                    human.Move("Down", this);
                }
                if (Py[i] < human.Y && Px[i] == human.X)
                {
                    human.Move("Up", this);
                }
            }
        }

        private void GoToExit()
        {
            if (Lee(human.X, human.Y, endX, endY) == true)
            {
                for (int i = 0; i < Len + 1; i++)
                {
                    if (Px[i] > human.X && Py[i] == human.Y)
                    {
                        human.Move("Right", this);
                    }
                    if (Px[i] < human.X && Py[i] == human.Y)
                    {
                        human.Move("Left", this);
                    }
                    if (Py[i] > human.Y && Px[i] == human.X)
                    {
                        human.Move("Down", this);
                    }
                    if (Py[i] < human.Y && Px[i] == human.X)
                    {
                        human.Move("Up", this);
                    }
                    
                    System.Threading.Thread.Sleep(100);
                    
                }
            }
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (InGame && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                
            }
        }

        private void CoverControls()
        {
            MazesComboBox.Visible = false;
            LoadButton.Visible = false;
            WidthNumeric.Visible = false;
            HeightNumeric.Visible = false;
            GenerateButton.Visible = false;
            ExitButton.Visible = false;
            HelpLabel.Visible = false;
            TilesetsBox.Visible = false;
            this.Focus();
            InGame = true;
        }

        class Cell
        {
            public bool Set;

            public bool Wall;

            public Cell Neighbour;

            public Cell()
            {
                Set = false;
                Wall = false;
                Neighbour = null;
            }

            public Cell(bool set, bool wall, Cell n)
            {
                Set = set;
                Wall = wall;
                Neighbour = n;
            }
        }

        struct Position
        {
            public int X;
            public int Y;

            public Position(int y, int x)
            {
                X = x;
                Y = y;
            }
        }

        private void Generate_Maze()
        {
            H = Convert.ToInt16(HeightNumeric.Value);
            W = Convert.ToInt16(WidthNumeric.Value);

            Cell[][] cells = new Cell[H][];

            Random rand = new Random();
            int start = rand.Next(1, H - 1);

            Position player = new Position(start, 0);

            Cell empty = new Cell(false, false, new Cell());

            for (int h = 0; h < H; h++)
            {
                cells[h] = new Cell[W];
                for (int w = 0; w < W; w++)
                {
                    if (h == 0 || h == H - 1 || w == 0 || w == W - 1 &&
                        (w != 0 || h != start))
                    {
                        cells[h][w] = new Cell(true, true, empty);
                    }
                    else
                    {
                        cells[h][w] = new Cell(false, true, empty);
                    }
                }
            }
            List<Position> p = new List<Position>();
            p.Add(player);
            int count = 0;
            while (p.Count > 0)
            {
                cells[p[p.Count - 1].Y][p[p.Count - 1].X].Wall = false;
                cells[p[p.Count - 1].Y][p[p.Count - 1].X].Set = true;
                count = 0;
                int[] directions = new int[4];
                if (p[p.Count - 1].X > 0 && !cells[p[p.Count - 1].Y][p[p.Count - 1].X - 1].Set &&
                    (cells[p[p.Count - 1].Y][p[p.Count - 1].X - 1].Neighbour.Equals(cells[p[p.Count - 1].Y][p[p.Count - 1].X]) ||
                    cells[p[p.Count - 1].Y][p[p.Count - 1].X - 1].Neighbour.Equals(empty)))
                {
                    directions[count] = 0;
                    cells[p[p.Count - 1].Y][p[p.Count - 1].X - 1].Neighbour = cells[p[p.Count - 1].Y][p[p.Count - 1].X];
                    count++;
                }
                if (p[p.Count - 1].X < W - 1 && !cells[p[p.Count - 1].Y][p[p.Count - 1].X + 1].Set &&
                    (cells[p[p.Count - 1].Y][p[p.Count - 1].X + 1].Neighbour.Equals(cells[p[p.Count - 1].Y][p[p.Count - 1].X]) ||
                    cells[p[p.Count - 1].Y][p[p.Count - 1].X + 1].Neighbour.Equals(empty)))
                {
                    directions[count] = 1;
                    cells[p[p.Count - 1].Y][p[p.Count - 1].X + 1].Neighbour = cells[p[p.Count - 1].Y][p[p.Count - 1].X];
                    count++;
                }
                if (p[p.Count - 1].Y > 0 && !cells[p[p.Count - 1].Y - 1][p[p.Count - 1].X].Set &&
                    (cells[p[p.Count - 1].Y - 1][p[p.Count - 1].X].Neighbour.Equals(cells[p[p.Count - 1].Y][p[p.Count - 1].X]) ||
                    cells[p[p.Count - 1].Y - 1][p[p.Count - 1].X].Neighbour.Equals(empty)))
                {
                    directions[count] = 2;
                    cells[p[p.Count - 1].Y - 1][p[p.Count - 1].X].Neighbour = cells[p[p.Count - 1].Y][p[p.Count - 1].X];
                    count++;
                }
                if (p[p.Count - 1].Y < H - 1 && !cells[p[p.Count - 1].Y + 1][p[p.Count - 1].X].Set &&
                    (cells[p[p.Count - 1].Y + 1][p[p.Count - 1].X].Neighbour.Equals(cells[p[p.Count - 1].Y][p[p.Count - 1].X]) ||
                    cells[p[p.Count - 1].Y + 1][p[p.Count - 1].X].Neighbour.Equals(empty)))
                {
                    directions[count] = 3;
                    cells[p[p.Count - 1].Y + 1][p[p.Count - 1].X].Neighbour = cells[p[p.Count - 1].Y][p[p.Count - 1].X];
                    count++;
                }
                if (count == 0)
                {
                    if (p[p.Count - 1].X > 0)
                        cells[p[p.Count - 1].Y][p[p.Count - 1].X - 1].Set = true;
                    if (p[p.Count - 1].X < W - 1)
                        cells[p[p.Count - 1].Y][p[p.Count - 1].X + 1].Set = true;
                    if (p[p.Count - 1].Y > 0)
                        cells[p[p.Count - 1].Y - 1][p[p.Count - 1].X].Set = true;
                    if (p[p.Count - 1].Y < H - 1)
                        cells[p[p.Count - 1].Y + 1][p[p.Count - 1].X].Set = true;
                    p.RemoveAt(p.Count - 1);
                }
                else
                {
                    int next = rand.Next(count);
                    switch (directions[next])
                    {
                        case 0:
                            p.Add(new Position(p[p.Count - 1].Y, p[p.Count - 1].X - 1));
                            break;
                        case 1:
                            p.Add(new Position(p[p.Count - 1].Y, p[p.Count - 1].X + 1));
                            break;
                        case 2:
                            p.Add(new Position(p[p.Count - 1].Y - 1, p[p.Count - 1].X));
                            break;
                        case 3:
                            p.Add(new Position(p[p.Count - 1].Y + 1, p[p.Count - 1].X));
                            break;
                    }
                }
            }
            count = 0;
            int[] pos = new int[H];
            for (int i = 0; i < H; i++)
            {
                if (!cells[i][W - 2].Wall)
                {
                    pos[count] = i;
                    count++;
                }
            }
            int end = pos[rand.Next(count)];
            cells[end][W - 1].Wall = false;
            Grid = new int [H,W];
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    if (cells[i][j].Wall == true)
                    {
                        Grid[i, j] = 1;
                    }
                    else
                    {
                        Grid[i, j] = 0;
                    }
                }
            }
            StreamWriter MazeFile = new StreamWriter(MazesPath + "maze_random_" + rand.Next().ToString() + ".txt");
            MazeFile.WriteLine(W.ToString());
            MazeFile.WriteLine(H.ToString());
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    if (Grid[i, j] == 1)
                    {
                        MazeFile.Write("1");
                    }
                    else
                    {
                        MazeFile.Write("0");
                    }
                }
                MazeFile.Write("\n");
            }
            MazeFile.WriteLine("0");
            MazeFile.WriteLine(start.ToString());
            MazeFile.WriteLine((W - 1).ToString());
            MazeFile.WriteLine(end.ToString());
            MazeFile.Close();
            
            //DrawMaze();
            human = new Player();
            human.X = 0;
            human.Y = start;
            //human.PlayerImage.Location = new Point(0, start * 50);
            //Controls.Add(human.PlayerImage);
            endX = W - 1;
            endY = end;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.BackColor = Color.Black;
            LoadTileSet(TilesetsPath + TilesetsBox.SelectedItem.ToString() + @"\");
            Generate_Maze();
            CoverControls();
            MusicPlayer = new System.Media.SoundPlayer(GameMusic);
            MusicPlayer.PlayLooping();
            Steps = 0;
            Invalidate();
        }

        /*
         * ДАЛЬШЕ КОД ПО СКРОЛЛИНГУ КАРТЫ МЫШЬЮ
         */
        int Mousex, Mousey;
        bool MoveView = false;
        Point OldCoords;

        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                Cursor.Current = Cursors.Hand;
                MoveView = true;
                OldCoords = ScreenViewPosition;
                Mousex = e.X;
                Mousey = e.Y;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (MoveView)
            {
                MoveView = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (MoveView && !Scaled)
            {
                ScreenViewPosition.X = OldCoords.X + e.X - Mousex;
                ScreenViewPosition.Y = OldCoords.Y + e.Y - Mousey;
                Invalidate();
            }
        }

        public void Form1_Load(object sender, EventArgs e)
        {
            this.BackgroundImage = new Bitmap("Background.png");
            this.BackgroundImageLayout = ImageLayout.Zoom;
            GenerateButton.Location = new Point(this.Width / 2 - 75, this.Height / 2 - 25);
            LoadButton.Location = new Point(GenerateButton.Left, GenerateButton.Top + 75);
            ExitButton.Location = new Point(LoadButton.Left, LoadButton.Top + 75);
            MazesComboBox.Location = new Point(LoadButton.Left + 175, LoadButton.Top);
            TilesetsBox.Location = new Point(MazesComboBox.Left, MazesComboBox.Top + 75);
            WidthNumeric.Location = new Point(GenerateButton.Left + 175, GenerateButton.Top);
            HelpLabel.Location = new Point(GenerateButton.Left - 195, GenerateButton.Top);
            HeightNumeric.Location = new Point(WidthNumeric.Left + 75, WidthNumeric.Top);
            SoundOn = true;
            MusicPlayer = new System.Media.SoundPlayer(MenuMusic);
            MusicPlayer.PlayLooping();
        }

        private void ExitButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void HelpLabel_Click(object sender, EventArgs e)
        {

        }

        private void TilesetsBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
