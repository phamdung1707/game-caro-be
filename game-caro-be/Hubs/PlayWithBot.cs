using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace game_caro_be.Hubs
{
    public class PlayWithBot
    {
        public int[,] map;
        public bool[,] isClicked;
        public int Rows = 8;
        public int Columns = 10;
        public Random random = new Random();

        public bool[,] checkClicked(int[,] map)
        {
            isClicked = new bool[Rows, Columns];

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    isClicked[i, j] = false;
                    if (map[i, j] != 0)
                    {
                        isClicked[i, j] = true;
                    }
                }
            }
            return isClicked;
        }

        public PlayWithBot(int[,] map)
        {
            this.map = map;
            this.isClicked = checkClicked(map);
        }

        public PlayWithBot()
        {
        }

        // Alpha-beta algorithm
        public bool cutRow(int indexCol, int indexRow)
        {
            for (int i = 1; i < 4; i++)
            {
                // left to right
                if ((indexCol + i) < Rows && !isClicked[indexCol + i, indexRow])
                {
                    return false;
                }

                // right to left
                if ((indexCol - i) >= 0 && !isClicked[indexCol - i, indexRow])
                {
                    return false;
                }
            }

            return true;
        }

        public bool cutCol(int indexCol, int indexRow)
        {
            for (int i = 1; i < 4; i++)
            {
                // bot to top
                if ((indexRow + i) < Columns && !isClicked[indexCol, indexRow + i])
                {
                    return false;
                }

                // top to bot
                if ((indexRow - i) >= 0 && !isClicked[indexCol, indexRow - i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool cutLeftTop(int indexCol, int indexRow)
        {
            for (int i = 1; i < 4; i++)
            {
                //top-left to bot-right
                if ((indexCol + i) < Rows && (indexRow + i) < Columns && !isClicked[indexCol + i, indexRow + i])
                {
                    return false;
                }

                //bot-right to top-left
                if ((indexCol - i) >= 0 && (indexRow - i) >= 0 && !isClicked[indexCol - i, indexRow - i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool cutRightBot(int indexCol, int indexRow)
        {
            for (int i = 1; i < 4; i++)
            {
                //top-right to bot-left
                if ((indexCol + i) < Rows && (indexRow - i) >= 0 && !isClicked[indexCol + i, indexRow - i])
                {
                    return false;
                }

                //bot-left to top-right
                if ((indexCol - i) >= 0 && (indexRow + i) < Columns && !isClicked[indexCol - i, indexRow + i])
                {
                    return false;
                }
            }

            return true;
        }

        public bool cutAlphaBeta(int indexCol, int indexRow)
        {
            return cutCol(indexCol, indexRow) || cutRow(indexCol, indexRow) || cutLeftTop(indexCol, indexRow) || cutRightBot(indexCol, indexRow);
        }

        public int pointAttackCol(int indexCol, int indexRow)
        {
            int pointAttack = 0;
            for (int i = 1; i <= 4; i++)
            {
                if ((indexRow + i) < Columns && map[indexCol, indexRow + i] == 2)
                {
                    pointAttack = pointAttack + 1;
                    if (i == 1)
                    {
                        pointAttack = pointAttack + 1;
                    }
                }

                if ((indexRow - i) >= 0 && map[indexCol, indexRow - i] == 2)
                {
                    pointAttack += 1;
                    if (i == 1)
                    {
                        pointAttack += 1;
                    }
                }
            }
            return pointAttack;
        }

        public int pointAttackRow(int indexCol, int indexRow)
        {
            int pointAttack = 0;
            for (int i = 1; i <= 4; i++)
            {
                if ((indexCol + i) < Rows && map[indexCol + i, indexRow] == 2)
                {
                    pointAttack += 1;
                    if (i == 1)
                    {
                        pointAttack += 1;
                    }


                }

                if ((indexCol - i) >= 0 && map[indexCol - i, indexRow] == 2)
                {
                    pointAttack += 1;
                    if (i == 1)
                    {
                        pointAttack += 1;
                    }

                }
            }

            return pointAttack;
        }

        public int pointAttackTopLeft(int indexCol, int indexRow)
        {
            int pointAttack = 0;
            for (int i = 1; i <= 4; i++)
            {
                if ((indexCol + i) < Rows && (indexRow + i) < Columns && map[indexCol + i, indexRow + i] == 2)
                {
                    pointAttack += 1;
                    if (i == 1)
                    {
                        pointAttack += 1;
                    }

                }

                if ((indexCol - i) >= 0 && (indexRow - i) >= 0 && map[indexCol - i, indexRow - i] == 2)
                {
                    pointAttack += 1;
                    if (i == 1)
                    {
                        pointAttack += 1;
                    }

                }
            }
            return pointAttack;
        }

        public int pointAttackTopRight(int indexCol, int indexRow)
        {
            int pointAttacck = 0;
            for (int i = 1; i <= 4; i++)
            {
                if ((indexCol + i) < Rows && (indexRow - i) >= 0 && map[indexCol + i, indexRow - i] == 2)
                {
                    pointAttacck += 1;
                    if (i == 1)
                    {
                        pointAttacck += 1;
                    }
                }

                if ((indexCol - i) >= 0 && (indexRow + i) < Columns && map[indexCol - i, indexRow + i] == 2)
                {
                    pointAttacck += 1;
                    if (i == 1)
                    {
                        pointAttacck += 1;
                    }
                }

            }
            return pointAttacck;
        }

        public int pointDefendCol(int indexCol, int indexRow)
        {
            int pointDefend = 0;
            for (int i = 1; i <= 4; i++)
            {
                if ((indexRow + i) < Columns && map[indexCol, indexRow + i] == 1)
                {
                    pointDefend += 1;
                    if (i == 1)
                    {
                        pointDefend += 1;
                    }
                }

                if ((indexRow - i) >= 0 && map[indexCol, indexRow - i] == 1)
                {
                    pointDefend += 1;
                    if (i == 1)
                    {
                        pointDefend += 1;
                    }
                }
            }
            return pointDefend;
        }

        public int pointDefendRow(int indexCol, int indexRow)
        {
            int pointDefend = 0;
            for (int i = 1; i <= 4; i++)
            {
                if ((indexCol + i) < Rows
                        && map[indexCol + i, indexRow] == 1)
                {
                    pointDefend += 1;
                    if (i == 1)
                    {
                        pointDefend += 1;
                    }
                }

                if ((indexCol - i) >= 0 && map[indexCol - i, indexRow] == 1)
                {
                    pointDefend += 1;
                    if (i == 1)
                    {
                        pointDefend += 1;
                    }
                }
            }
            return pointDefend;
        }

        public int pointDefendTopLeft(int indexCol, int indexRow)
        {
            int pointDefend = 0;
            for (int i = 1; i <= 4; i++)
            {
                if ((indexCol + i) < Rows && (indexRow + i) < Columns && map[indexCol + i, indexRow + i] == 1)
                {
                    pointDefend += 1;
                    if (i == 1)
                    {
                        pointDefend += 1;
                    }
                }

                if ((indexCol - i) >= 0 && (indexRow - i) >= 0 && map[indexCol - i, indexRow - i] == 1)
                {
                    pointDefend += 1;
                    if (i == 1)
                    {
                        pointDefend += 1;
                    }
                }
            }
            return pointDefend;
        }

        public int pointDefendTopRight(int indexCol, int indexRow)
        {
            int pointDefend = 0;
            for (int i = 1; i <= 4; i++)
            {
                if ((indexCol + i) < Rows && (indexRow - i) >= 0 && map[indexCol + i, indexRow - i] == 1)
                {
                    pointDefend += 1;
                    if (i == 1)
                    {
                        pointDefend += 1;
                    }

                }

                if ((indexCol - i) >= 0 && (indexRow + i) < Columns && map[indexCol - i, indexRow + i] == 1)
                {
                    pointDefend += 1;
                    if (i == 1)
                    {
                        pointDefend += 1;
                    }

                }
            }
            return pointDefend;
        }

        public int pointAttack(int indexCol, int indexRow)
        {
            int maxpoint = pointAttackCol(indexCol, indexRow);
            if (pointAttackTopRight(indexCol, indexRow) > maxpoint)
            {
                maxpoint = pointAttackTopRight(indexCol, indexRow);
            }

            if (pointAttackRow(indexCol, indexRow) > maxpoint)
            {
                maxpoint = pointAttackRow(indexCol, indexRow);
            }

            if (pointAttackTopLeft(indexCol, indexRow) > maxpoint)
            {
                maxpoint = pointAttackTopLeft(indexCol, indexRow);
            }

            return maxpoint;

        }

        public int pointDefend(int indexCol, int indexRow)
        {
            int maxpoint = pointDefendCol(indexCol, indexRow);
            if (pointDefendRow(indexCol, indexRow) > maxpoint)
            {
                maxpoint = pointDefendRow(indexCol, indexRow);
            }

            if (maxpoint < pointDefendTopLeft(indexCol, indexRow))
            {
                maxpoint = pointDefendTopLeft(indexCol, indexRow);
            }

            if (pointDefendTopRight(indexCol, indexRow) > maxpoint)
            {
                maxpoint = pointDefendTopRight(indexCol, indexRow);
            }

            return maxpoint;
        }

        public string getData(string data, bool isTurnOne)
        {
            if (isTurnOne)
            {
                return getDataTurnOne(data);
            }

            this.map = StringToArray(data);
            checkClicked(map);

            int maxPoint = 0;
            int secondMaxPoint = -1;
            int indexMaxCol = -1;
            int indexMaxRow = -1;
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (!isClicked[i, j])
                    {
                        int pointAk = pointAttack(i, j);
                        int pointDef = pointDefend(i, j);
                        int max = Math.Max(pointAk, pointDef);

                        if (max > maxPoint)
                        {
                            maxPoint = max;
                        }
                    }
                }
            }

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (!isClicked[i, j]
                            && maxPoint == Math.Max(pointAttack(i, j), pointDefend(i, j)))
                    {
                        int secondPoint = Math.Min(pointAttack(i, j), pointDefend(i, j));

                        if (secondPoint > secondMaxPoint)
                        {
                            secondMaxPoint = secondPoint;
                            indexMaxCol = i;
                            indexMaxRow = j;
                        }
                    }
                }
            }

            isClicked[indexMaxCol, indexMaxRow] = true;
            map[indexMaxCol, indexMaxRow] = 2;

            return ArrayToString(map);
        }

        public string getDataTurnOne(string data)
        {
            int[,] array = StringToArray(data);

            int indexI = 0;
            int indexJ = 0;

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if (array[i, j] != 0)
                    {
                        indexI = i;
                        indexJ = j;
                        break;
                    }
                }
            }

            List<IndexRandom> vs = new List<IndexRandom>();

            for (int i = 0; i < array.GetLength(0); i++)
            {
                for (int j = 0; j < array.GetLength(1); j++)
                {
                    if ((i == indexI && (j == indexJ + 1 || j == indexJ - 1)) 
                        || ((j == indexJ && (i == indexI + 1 || i == indexI - 1))) 
                        || (i == indexI - 1 && (j == indexJ - 1 || j == indexJ + 1))
                        || (i == indexI + 1 && (j == indexJ - 1 || j == indexJ + 1)))
                    {
                        vs.Add(new IndexRandom(i, j));
                    }
                }
            }

            int indexRandom = random.Next(0, vs.Count);

            array[vs[indexRandom].i, vs[indexRandom].j] = 2;

            return ArrayToString(array);
        }

        public string ArrayToString(int[,] array)
        {
            string data = "";

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    data += array[i, j].ToString();
                }
            }

            return data;
        }

        public int[,] StringToArray(string data)
        {
            int[,] array = new int[8, 10];

            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    array[i, j] = int.Parse(data.Substring(i * 10 + j, 1));
                }
            }

            return array;
        }

        public class IndexRandom
        {
            public IndexRandom(int i, int j)
            {
                this.i = i;
                this.j = j;
            }

            public int i { get; set; }
            public int j { get; set; }
        }
    }
}
