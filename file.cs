using Avalonia.Input;
using Digger.Architecture;

namespace Digger
{
    public class Terrain : ICreature
    {
        public string GetImageFileName()
        {
            return "Terrain.png";
        }

        public int GetDrawingPriority()
        {
            return 100;
        }

        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand
            {
                DeltaX = 0,
                DeltaY = 0,
                TransformTo = this
            };
        }

        public bool DeadInConflict(ICreature other)
        {
            return other is Player;
        }
    }

    public class Player : ICreature
    {
        public string GetImageFileName()
        {
            return "Digger.png";
        }

        public int GetDrawingPriority()
        {
            return 0;
        }

        public CreatureCommand Act(int x, int y)
        {
            var direction = GetInputDirection();
            var nextX = x + direction.Item1;
            var nextY = y + direction.Item2;
            
            if (!IsPositionInsideMap(nextX, nextY) || IsPositionBlockedBySack(nextX, nextY))
            {
                return new CreatureCommand();
            }

            return new CreatureCommand
            {
                DeltaX = direction.Item1,
                DeltaY = direction.Item2
            };
        }

        public bool DeadInConflict(ICreature other)
        {
            return other is Sack || other is Monster;
        }

        private (int, int) GetInputDirection()
        {
            var key = Game.KeyPressed;
            
            if (key == Key.Up)
                return (0, -1);
            else if (key == Key.Down)
                return (0, 1);
            else if (key == Key.Left)
                return (-1, 0);
            else if (key == Key.Right)
                return (1, 0);
            else
                return (0, 0);
        }

        private bool IsPositionInsideMap(int x, int y)
        {
            return x >= 0 && x < Game.MapWidth && 
                   y >= 0 && y < Game.MapHeight;
        }

        private bool IsPositionBlockedBySack(int x, int y)
        {
            var cell = Game.Map[x, y];
            return cell is Sack;
        }
    }

    public class Sack : ICreature
    {
        public int FlightTime = 0;
        
        public string GetImageFileName()
        {
            return "Sack.png";
        }

        public int GetDrawingPriority()
        {
            return 10;
        }

        public CreatureCommand Act(int x, int y)
        {
            var command = new CreatureCommand
            {
                DeltaX = 0,
                DeltaY = 1,
                TransformTo = this
            };

            var newX = x + command.DeltaX;
            var newY = y + command.DeltaY;

            if (CanFallTo(newX, newY))
            {
                FlightTime++;
            }
            else
            {
                if (FlightTime > 1)
                {
                    command.TransformTo = new Gold();
                }
                FlightTime = 0;
                command.DeltaY = 0;
            }

            return command;
        }

        public bool CanFallTo(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Game.MapWidth || y >= Game.MapHeight)
                return false;
                
            var cellContent = Game.Map[x, y];
            return cellContent == null || 
                  ((cellContent is Player || cellContent is Monster) && FlightTime > 0);
        }

        public bool DeadInConflict(ICreature other)
        {
            return false;
        }
    }

    public class Gold : ICreature
    {
        public string GetImageFileName()
        {
            return "Gold.png";
        }

        public int GetDrawingPriority()
        {
            return 10;
        }

        public CreatureCommand Act(int x, int y)
        {
            return new CreatureCommand
            {
                DeltaX = 0,
                DeltaY = 0,
                TransformTo = this
            };
        }

        public bool DeadInConflict(ICreature other)
        {
            if (other is Player)
            {
                Game.Scores += 10;
            }
            return true;
        }
    }

    public class Monster : ICreature
    {
        public string GetImageFileName()
        {
            return "Monster.png";
        }

        public int GetDrawingPriority()
        {
            return 20;
        }

        public CreatureCommand Act(int x, int y)
        {
            var (playerX, playerY) = FindPlayerOnMap();
            
            if (playerX == -1)
            {
                return new CreatureCommand
                {
                    DeltaX = 0,
                    DeltaY = 0,
                    TransformTo = this
                };
            }

            var moveX = 0;
            var moveY = 0;
            
            if (x < playerX && CanMoveTo(x + 1, y))
                moveX = 1;
            else if (x > playerX && CanMoveTo(x - 1, y))
                moveX = -1;
            else if (y < playerY && CanMoveTo(x, y + 1))
                moveY = 1;
            else if (y > playerY && CanMoveTo(x, y - 1))
                moveY = -1;

            return new CreatureCommand
            {
                DeltaX = moveX,
                DeltaY = moveY,
                TransformTo = this
            };
        }

        private (int, int) FindPlayerOnMap()
        {
            for (var i = 0; i < Game.MapWidth; i++)
            {
                for (var j = 0; j < Game.MapHeight; j++)
                {
                    if (Game.Map[i, j] is Player)
                    {
                        return (i, j);
                    }
                }
            }
            return (-1, -1);
        }

        private bool CanMoveTo(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Game.MapWidth || y >= Game.MapHeight)
                return false;
                
            var cellContent = Game.Map[x, y];
            return cellContent == null || 
                  cellContent is Gold || 
                  cellContent is Player;
        }

        public bool DeadInConflict(ICreature other)
        {
            if (other is Monster)
                return true;
                
            if (other is Sack sack)
                return sack.FlightTime > 0;
                
            return false;
        }
    }
}
