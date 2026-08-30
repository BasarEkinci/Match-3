using System.Text;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Persistence;
using Match3.Model.Settings;
using UnityEngine;

namespace Match3.Data
{
    public sealed class PlayerPrefsSaveRepository : ISaveRepository
    {
        private const string BoardKey = "Match3.Board";
        private const string ScoreKey = "Match3.Score";
        private const string WidthKey = "Match3.Board.Width";
        private const string HeightKey = "Match3.Board.Height";
        private const int MissingSize = 0;
        private const int NoScore = 0;
        private const int CharsPerCell = 2;
        private const char FirstDigit = '0';

        private readonly IBoardSettings m_Settings;
        private readonly StringBuilder m_Builder = new StringBuilder();

        public PlayerPrefsSaveRepository(IBoardSettings settings)
        {
            m_Settings = settings;
        }

        public bool HasSave =>
            PlayerPrefs.GetInt(WidthKey, MissingSize) == m_Settings.Width &&
            PlayerPrefs.GetInt(HeightKey, MissingSize) == m_Settings.Height &&
            PlayerPrefs.GetString(BoardKey, string.Empty).Length == CellCount * CharsPerCell;

        private int CellCount => m_Settings.Width * m_Settings.Height;

        public void LoadBoard(Board board)
        {
            string cells = PlayerPrefs.GetString(BoardKey, string.Empty);
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    int offset = (((y * board.Width) + x) * CharsPerCell);
                    board.Set(
                        new GridPosition(x, y),
                        new Tile(
                            (TileColor)(cells[offset] - FirstDigit),
                            (SpecialTileType)(cells[offset + 1] - FirstDigit)));
                }
            }
        }

        public int LoadScore() => PlayerPrefs.GetInt(ScoreKey, NoScore);

        public void Save(Board board, int score)
        {
            m_Builder.Clear();
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    board.TryGet(new GridPosition(x, y), out Tile tile);
                    m_Builder.Append((char)(FirstDigit + (int)tile.Color));
                    m_Builder.Append((char)(FirstDigit + (int)tile.Special));
                }
            }

            PlayerPrefs.SetString(BoardKey, m_Builder.ToString());
            PlayerPrefs.SetInt(ScoreKey, score);
            PlayerPrefs.SetInt(WidthKey, board.Width);
            PlayerPrefs.SetInt(HeightKey, board.Height);
            PlayerPrefs.Save();
        }
    }
}
