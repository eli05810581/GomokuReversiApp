
using GomokuReversiApp;
using Xunit;

namespace GomokuReversiApp.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Board_ShouldHaveCorrectDimensions()
        {
            Board board = new Board(10, 10);

            Assert.Equal(10, board.Rows);
            Assert.Equal(10, board.Cols);
        }

        [Fact]
        public void Board_ShouldRejectOutOfBoundsPositions()
        {
            Board board = new Board(8, 8);

            Assert.True(board.IsWithinBounds(1, 1));
            Assert.True(board.IsWithinBounds(8, 8));
            Assert.False(board.IsWithinBounds(0, 1));
            Assert.False(board.IsWithinBounds(9, 1));
        }

        [Fact]
        public void Board_ShouldIdentifyFourCorners()
        {
            Board board = new Board(8, 8);

            Assert.True(board.IsCorner(1, 1));
            Assert.True(board.IsCorner(1, 8));
            Assert.True(board.IsCorner(8, 1));
            Assert.True(board.IsCorner(8, 8));

            Assert.False(board.IsCorner(4, 4));
        }

        [Fact]
        public void Board_ShouldCountPiecesCorrectly()
        {
            Board board = new Board(8, 8);

            board.SetCell(3, 3, new Disc(1));
            board.SetCell(4, 4, new Disc(1));
            board.SetCell(5, 5, new Disc(2));

            Assert.Equal(2, board.CountPieces(1));
            Assert.Equal(1, board.CountPieces(2));
        }

        [Fact]
        public void Board_ShouldRestoreSavedState()
        {
            Board original = new Board(10, 10);

            original.SetCell(3, 3, new OrdinaryStone(1));
            original.SetCell(4, 4, new OrdinaryStone(2));
            original.SetCell(5, 5, new HeavyStone(1));

            string[] savedRows = original.CaptureRows();

            Board restored = new Board(10, 10);
            restored.RestoreRows(savedRows);

            Assert.Equal(1, restored.GetOwner(3, 3));
            Assert.Equal(2, restored.GetOwner(4, 4));
            Assert.IsType<HeavyStone>(restored.GetCell(5, 5));

            Assert.Equal(savedRows, restored.CaptureRows());
        }
    }
}
