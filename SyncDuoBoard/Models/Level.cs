using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncDuoBoard.Models
{
    public class Level
    {
        public string levelName { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public Position startPosA { get; set; }
        public Position startPosB { get; set; }
        public Position finishPosA { get; set; }
        public Position finishPosB { get; set; }
        public List<Position> enemyPositionsA { get; set; }
        public List<Position> enemyPositionsB { get; set; }
        public List<int> mazeLayoutA { get; set; }
        public List<int> mazeLayoutB { get; set; }
        public Level Clone()
        {
            return new Level
            {
                levelName = this.levelName,
                width = this.width,
                height = this.height,
                startPosA = new Position { x = this.startPosA.x, y = this.startPosA.y },
                startPosB = new Position { x = this.startPosB.x, y = this.startPosB.y },
                finishPosA = new Position { x = this.finishPosA.x, y = this.finishPosA.y },
                finishPosB = new Position { x = this.finishPosB.x, y = this.finishPosB.y },
                enemyPositionsA = this.enemyPositionsA.Select(pos => new Position { x = pos.x, y = pos.y }).ToList(),
                enemyPositionsB = this.enemyPositionsB.Select(pos => new Position { x = pos.x, y = pos.y }).ToList(),
                mazeLayoutA = new List<int>(this.mazeLayoutA),
                mazeLayoutB = new List<int>(this.mazeLayoutB)
            };

        }
    }
}
