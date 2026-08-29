using System.Collections.Generic;
using Match3.Model;
using Match3.Model.Enums;
using Match3.Model.Special;
using NUnit.Framework;

namespace Match3.Tests.EditMode
{
    public sealed class SpecialTileFactoryTests
    {
        private const TileColor GroupColor = TileColor.Blue;

        [Test]
        public void TripleCreatesNoSpecialTile()
        {
            Assert.AreEqual(SpecialTileType.None, SpecialTileFactory.Resolve(MatchShape.Line3));
        }

        [Test]
        public void HorizontalQuadCreatesHorizontalRocket()
        {
            Assert.AreEqual(
                SpecialTileType.HorizontalRocket,
                SpecialTileFactory.Resolve(MatchShape.Line4Horizontal));
        }

        [Test]
        public void VerticalQuadCreatesVerticalRocket()
        {
            Assert.AreEqual(
                SpecialTileType.VerticalRocket,
                SpecialTileFactory.Resolve(MatchShape.Line4Vertical));
        }

        [Test]
        public void CornerCreatesBomb()
        {
            Assert.AreEqual(SpecialTileType.Bomb, SpecialTileFactory.Resolve(MatchShape.Corner));
        }

        [Test]
        public void FiveInRowCreatesColorBomb()
        {
            Assert.AreEqual(SpecialTileType.ColorBomb, SpecialTileFactory.Resolve(MatchShape.Line5));
        }

        [Test]
        public void OriginPrefersSwappedCell()
        {
            MatchGroup group = CreateRow(0, 4);

            GridPosition origin = SpecialTileFactory.ResolveOrigin(
                group,
                new GridPosition(3, 0),
                new GridPosition(3, 1));

            Assert.AreEqual(new GridPosition(3, 0), origin);
        }

        [Test]
        public void OriginFallsBackToGroupCentreWhenSwapIsOutside()
        {
            MatchGroup group = CreateRow(0, 5);

            GridPosition origin = SpecialTileFactory.ResolveOrigin(
                group,
                new GridPosition(7, 7),
                new GridPosition(6, 7));

            Assert.AreEqual(new GridPosition(2, 0), origin);
        }

        private static MatchGroup CreateRow(int y, int length)
        {
            List<GridPosition> positions = new List<GridPosition>();
            for (int x = 0; x < length; x++)
            {
                positions.Add(new GridPosition(x, y));
            }

            return new MatchGroup(positions, GroupColor, MatchShape.Line5);
        }
    }
}
