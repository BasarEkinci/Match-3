using System.Collections.Generic;

namespace Match3.Model.Gravity
{
    public interface IGravityResolver
    {
        void Resolve(Board board, List<TileMove> moves, List<TileSpawn> spawns);
    }
}
