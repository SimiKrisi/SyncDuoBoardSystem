using Newtonsoft.Json;
using SyncDuoBoard.Models;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography.X509Certificates;
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

namespace SyncDuoBoard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
       
        private LevelsData levelsData;
        private int OldLevelIndex;
        private Level EditingLevel;
        public MainWindow()
        {
            InitializeComponent();
            LoadBoardsFromJson("levels.json");
            DisplayBoards(BoardsListContainer);

        }
        public void LoadBoardsFromJson(string filename)
        {
            try
            {
                if (!File.Exists(filename))
                {
                    MessageBox.Show($"File '{filename}' not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string levelsJsonContent = File.ReadAllText(filename);
                levelsData = JsonConvert.DeserializeObject<LevelsData>(levelsJsonContent);

                if (levelsData != null && levelsData.levels != null)
                {
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading boards from JSON: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DisplayBoards(StackPanel container)
        {
            container.Children.Clear();

            foreach (var level in levelsData.levels)
            {
                StackPanel BoardsListItem = new StackPanel 
                { 
                    Width = this.Width-40,
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(),
                };
                StackPanel mazeA = new StackPanel
                {
                    Height =100,
                };
                StackPanel mazeB = new StackPanel
                {
                    Height = 100,
                };
                GenerateComplexBoard(level, mazeA, mazeB, (int)mazeA.Height);
                BoardsListItem.Children.Add(mazeA);
                BoardsListItem.Children.Add(mazeB);
                TextBlock details = new TextBlock
                {
                    Text = level.levelName + "     |      " + level.width + "*" + level.height + "      |      EnemiesA: " + level.enemyPositionsA.Count() + "; EnemiesB: " + level.enemyPositionsB.Count(),
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                };

                BoardsListItem.Children.Add(details);
                Button editLevelButton = new Button
                {
                    Content = "Edit",
                    Margin = new Thickness(5),
                    Padding = new Thickness(10),
                    Height = 40,
                };
                editLevelButton.Click += (sender, e) => LoadLeveltoEdit(level);
                BoardsListItem.Children.Add(editLevelButton);
                container.Children.Add(BoardsListItem);
            }
        }

        private void LoadLeveltoEdit(Level level)
        {
            MyTabControl.SelectedIndex = 1;
            OldLevelIndex = levelsData.levels.IndexOf(level);
            EditingLevel = level.Clone();
            LoadLevelDetails(EditingLevel);
        }
        private void LoadLevelDetails(Level level)
        {
            BoardID_Input.Text = level.levelName.Split('_')[1];
            BoardSize_Input.Text = level.width.ToString();
            StartPosAX_Input.Text = level.startPosA.x.ToString();
            StartPosAY_Input.Text = level.startPosA.y.ToString();
            StartPosBX_Input.Text = level.startPosB.x.ToString();
            StartPosBY_Input.Text = level.startPosB.y.ToString();
            FinishPosAX_Input.Text = level.finishPosA.x.ToString();
            FinishPosAY_Input.Text = level.finishPosA.y.ToString();
            FinishPosBX_Input.Text = level.finishPosB.x.ToString();
            FinishPosBY_Input.Text = level.finishPosB.y.ToString();
            EnemyPosAList_Input.Text = string.Join("", level.enemyPositionsA.Select(pos => $"({pos.x};{pos.y})"));
            EnemyPosBList_Input.Text = string.Join("", level.enemyPositionsB.Select(pos => $"({pos.x};{pos.y})"));
            MazeLayoutA_Input.Text = "";
            MazeLayoutB_Input.Text = "";
            for (int i = 0; i < level.width*level.height; i++)
            {
                MazeLayoutA_Input.Text += level.mazeLayoutA[i].ToString()+";";
                MazeLayoutB_Input.Text += level.mazeLayoutB[i].ToString() + ";";
                if (i%level.width==level.width-1)
                {
                    MazeLayoutA_Input.Text += "\n";
                    MazeLayoutB_Input.Text += "\n";
                }
            }
            

            GenerateComplexBoard(level, BoardAContainer, BoardBContainer, (int)BoardAContainer.ActualHeight);
        }
        private void GenerateComplexBoard(Level level, StackPanel boardAContainer, StackPanel boardBContainer, int containerHeight)
        {
            Debug.WriteLine($"Generating boards for level: {level.levelName}");
            GenerateSingleBoard(level.width, level.height, level.mazeLayoutA, level.startPosA, level.finishPosA, level.enemyPositionsA, boardAContainer, containerHeight);
            GenerateSingleBoard(level.width, level.height, level.mazeLayoutB, level.startPosB, level.finishPosB, level.enemyPositionsB, boardBContainer, containerHeight);
           
        }
        private void GenerateSingleBoard(int width, int height, List<int> mazeLayout, Position startPos, Position finishPos, List<Position> enemyPositions, StackPanel boardContainer, int containerHeight)
        {
            boardContainer.Children.Clear();
            for (int y = 0; y < height; y++)
            {
                StackPanel row = new StackPanel { Orientation = Orientation.Horizontal };
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    int cellsize = (int)containerHeight/height;
                    var cellcolor = Brushes.Green;
                    if (mazeLayout[index]==0)
                    {
                        cellcolor = Brushes.LightGray;
                    }
                    Border cell = new Border
                    {
                        Width = cellsize,
                        Height = cellsize,
                        Background = cellcolor,
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5)
                    };
                    if (startPos.x == x && startPos.y == y)
                    {
                        cell.Background = Brushes.LightSkyBlue;
                    }
                    else if (finishPos.x == x && finishPos.y == y)
                    {
                        cell.Background = Brushes.White;

                    }
                    else if (enemyPositions.Any(pos => pos.x == x && pos.y == y))
                    {
                        cell.Background = Brushes.Red;
                    }
                    row.Children.Add(cell);
                }
                
                boardContainer.Children.Add(row);
            }
        }
        private bool isNumber(string input)
        {
            int result;
            return int.TryParse(input, out result);
        }
        private void BoardID_Input_LostFocus(object sender, RoutedEventArgs e)
        {
            string input = BoardID_Input.Text;
            if (isNumber(input)&& int.Parse(input)>0)
            {
                EditingLevel.levelName = "Level_" + BoardID_Input.Text;
            }
            else
            {
                BoardID_Input.Text = levelsData.levels[OldLevelIndex].levelName.Split('_')[1];
            }

        }

        private void BoardSize_Input_LostFocus(object sender, RoutedEventArgs e)
        {
            string input = BoardSize_Input.Text;
            if (isNumber(input)&&int.Parse(input)>5)
            {
                int newSize = int.Parse(input);
                int oldSize = EditingLevel.height;
                EditingLevel.width = newSize;
                EditingLevel.height = newSize;
                EditingLevel.mazeLayoutA =ExtendMazeLayout(EditingLevel.mazeLayoutA, newSize, oldSize);
                EditingLevel.mazeLayoutB = ExtendMazeLayout(EditingLevel.mazeLayoutB, newSize, oldSize);
                EditingLevel.startPosA = RepositionEntity(newSize, EditingLevel.mazeLayoutA, EditingLevel.startPosA);
                EditingLevel.startPosB = RepositionEntity(newSize, EditingLevel.mazeLayoutB, EditingLevel.startPosB);
                EditingLevel.finishPosA = RepositionEntity(newSize, EditingLevel.mazeLayoutA, EditingLevel.finishPosA);
                EditingLevel.finishPosB = RepositionEntity(newSize, EditingLevel.mazeLayoutB, EditingLevel.finishPosB);
                for (int i = 0; i < EditingLevel.enemyPositionsA.Count(); i++)
                {
                    EditingLevel.enemyPositionsA[i] = RepositionEntity(newSize, EditingLevel.mazeLayoutA, EditingLevel.enemyPositionsA[i]);
                   
                }
                for (int i = 0; i < EditingLevel.enemyPositionsB.Count(); i++)
                {
                    EditingLevel.enemyPositionsB[i] = RepositionEntity(newSize, EditingLevel.mazeLayoutB, EditingLevel.enemyPositionsB[i]);
                    
                }
                LoadLevelDetails(EditingLevel);
            }
            else
            {
                BoardSize_Input.Text = levelsData.levels[OldLevelIndex].width.ToString();
            }
            
        }
        private Position RepositionEntity(int newSize, List<int> layout, Position oldPosition)
        {
            Position newPos = new Position { x = Math.Min(oldPosition.x, newSize - 2), y = Math.Min(oldPosition.y, newSize - 2) };
            layout[newPos.y * newSize + newPos.x] = 0;
            
            return newPos;
        }
        private List<int> ExtendMazeLayout( List<int> Layout,int newSize, int oldSize)
        {
            List<List<int>> newMazeLayoutA = new List<List<int>>();//készítünk egy új layout-ot, és az feltöltjük teljesen 1-esekkel.

            for (int i = 0; i < newSize; i++)
            {
                newMazeLayoutA.Add(new List<int>());
                for (int j = 0; j < newSize; j++)
                {

                    newMazeLayoutA[i].Add(1);
                }
            }
            int smalerSize = Math.Min(newSize, oldSize);
            for (int i = 0; i < smalerSize-1; i++)
            {
                for (int j = 0; j < smalerSize-1; j++)
                {
                    newMazeLayoutA[i][j] = Layout[i * oldSize + j];
                    
                }
                
            }
            return newMazeLayoutA.SelectMany(row => row).ToList();
        }
        private void StartPosAX_Input_LostFocus(object sender, RoutedEventArgs e)
        {
            string input = StartPosAX_Input.Text;
            if (isNumber(input)&&int.Parse(input)>0&&int.Parse(input)<EditingLevel.width-1)
            {
                int newX = int.Parse(input);
            }
        }

        private void StartPosAY_Input_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void StartPosBX_Input_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void StartPosBY_Input_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void FinishPosAX_Input_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void FinishPosAY_Input_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void FinishPosBX_Input_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void FinishPosBY_Input_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void EnemyPosAList_Input_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void EnemyPosBList_Input_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void MazeLayoutA_Input_LostFocus(object sender, RoutedEventArgs e)
        {

        }

        private void MazeLayoutB_Input_LostFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}
