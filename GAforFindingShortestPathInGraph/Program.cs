using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAforFindingShortestPathInGraph
{
    class Program
    {
        static int countIndivids, countComp, countGenes, firstComp, lastComp;

        static void Main(string[] args)
        {
            countComp = 10;
            Console.WriteLine("Кол-во компьютеров: " + countComp);
            Random rand = new Random();

            int[,] matrWeidths = new int[countComp, countComp];
            for (int i = 0; i < countComp; i++)
                for (int j = i + 1; j < countComp; j++)
                    matrWeidths[i, j] = rand.Next(1, 99);
            for (int i = 0; i < countComp; i++)
                for (int j = 0; j < i; j++)
                    matrWeidths[i, j] = matrWeidths[j, i];

            Console.WriteLine("Длина пути от одного компьютера к другому: ");
            Console.Write("   | ");
            for (int i = 0; i < countComp; i++)
            {
                if (i < 9)
                    Console.Write("{0}   ", i + 1);
                else
                    Console.Write("{0}  ", i + 1);
            }
            Console.WriteLine();
            Console.Write("---");
            for (int i = 0; i < countComp; i++)
                Console.Write("----");
            Console.WriteLine();
            for (int i = 0; i < countComp; i++)
            {
                if (i < 9)
                    Console.Write("{0}  | ", i + 1);
                else
                    Console.Write("{0} | ", i + 1);
                for (int j = 0; j < countComp; j++)
                {
                    if (matrWeidths[i, j] > 9)
                        Console.Write("{0}  ", matrWeidths[i, j]);
                    else
                        Console.Write("{0}   ", matrWeidths[i, j]);
                }
                Console.WriteLine();
            }

            firstComp = rand.Next(1, countComp);
            lastComp = rand.Next(1, countComp);
            if (firstComp == lastComp)
                firstComp = rand.Next(1, countComp);
            Console.WriteLine("Компьютер-отправитель: {0}, компьютер-получатель: {1}", firstComp, lastComp);

            countIndivids = 12;
            Console.WriteLine("Кол-во особей в популяции: " + countIndivids);

            countGenes = 6;
            Console.WriteLine("Кол-во генов у особи: " + countGenes);

            int[,] individuals = new int[countIndivids, countGenes];  //особи
            for (int i = 0; i < countIndivids; i++)
                for (int j = 0; j < countGenes; j++)
                    individuals[i, j] = rand.Next(0, countComp);

            int[,] allFitnesses = new int[countIndivids, 2];  //пригодность каждой особи и её номер
            Console.WriteLine();
            Console.WriteLine("Поколение 1:");
            for (int i = 0; i < countIndivids; i++)
            {
                int fitness = 0;
                fitness += matrWeidths[firstComp - 1, individuals[i, 0]];
                Console.Write("Особь {0}, её гены: ", i + 1);
                for (int j = 0; j < countGenes; j++)
                {
                    if (individuals[i, j] < 9)
                        Console.Write("{0},  ", individuals[i, j] + 1);
                    else
                        Console.Write("{0}, ", individuals[i, j] + 1);
                    if (j > 0)
                        fitness += matrWeidths[individuals[i, j - 1], individuals[i, j]];
                }
                fitness += matrWeidths[individuals[i, countGenes - 1], lastComp - 1];
                Console.WriteLine(" пригодность: " + fitness);
                allFitnesses[i, 0] = i;
                allFitnesses[i, 1] = fitness;
            }
            Console.WriteLine();

            int iterations = 0;
            do
            {
                iterations++;
                List<int[]> newIndividuals = new List<int[]>();
                do
                {
                    int[] parent_1 = new int[countGenes];  //родители
                    int[] parent_2 = new int[countGenes];
                    //выбор родителей методом инбридинг
                    Inbreeding(individuals, allFitnesses, ref parent_1, ref parent_2);

                    int[] descendant_1 = new int[countGenes];  //потомоки
                    int[] descendant_2 = new int[countGenes];
                    //скрещивание с пом. дискретной рекомбинации
                    CrossingDiscreteRecombination(parent_1, parent_2, ref descendant_1, ref descendant_2);

                    newIndividuals.Add(descendant_1);
                    newIndividuals.Add(descendant_2);
                } while (newIndividuals.Count < countIndivids);  //формируем новую популяцию

                MutationExchange(ref newIndividuals);  //мутация обменом соседних генов

                for (int i = 0; i < countIndivids; i++)  //объединение родителей и потомков
                {
                    newIndividuals.Add(new int[countGenes]);
                    for (int j = 0; j < countGenes; j++)
                        newIndividuals.Last()[j] = individuals[i, j];
                }

                TruncationSelection(matrWeidths, ref newIndividuals);  //отбор особей в новое поколение методом усечения

                Console.WriteLine("Поколение {0}:", iterations + 1);
                OutputPopulation(matrWeidths, ref allFitnesses, newIndividuals);

                int counter = 0;
                for (int i = 1; i < countIndivids; i++)  //подсчёт кол-ва особей с одинаковой пригодностью для остановки алгоритма
                    if (allFitnesses[i, 1] == allFitnesses[0, 1])
                        counter++;
                if (counter >= countIndivids * 0.75)
                    break;

                for (int i = 0; i < countIndivids; i++)
                    for (int j = 0; j < countGenes; j++)
                        individuals[i, j] = newIndividuals[i][j];
            } while (iterations < 50);

            Console.WriteLine("Кол-во поколений: " + (iterations + 1));
            Console.Write("Найденный путь: " + firstComp);
            for (int i = 0; i < countGenes; i++)
                Console.Write(" -> " + (individuals[0, i] + 1));
            Console.WriteLine(" -> {0}, его длина: " + allFitnesses[0, 1], lastComp);
            Console.WriteLine("Длина прямого пути от {0} компьютера к {1}: " + matrWeidths[firstComp - 1, lastComp - 1], firstComp, lastComp);
            Console.ReadLine();
        }

        static void TruncationSelection(int[,] matrWeidths, ref List<int[]> individuals)  //отбор усечением
        {
            Random rand = new Random();
            double T = 0.4;  //порог для отбора
            int fitness = 0;
            int[,] allFitnesses = new int[individuals.Count, 2];  //пригодность каждой особи и её номер
            for (int i = 0; i < individuals.Count; i++)  //вычисление пригодности каждой особи
            {
                fitness += matrWeidths[firstComp - 1, individuals[i][0]];
                for (int j = 1; j < countGenes; j++)
                    fitness += matrWeidths[individuals[i][j - 1], individuals[i][j]];
                fitness += matrWeidths[individuals[i][countGenes - 1], lastComp - 1];
                allFitnesses[i, 0] = i;
                allFitnesses[i, 1] = fitness;
                fitness = 0;
            }

            int[] temp = new int[individuals.Count];  //сортировка особей в порядке убывания их пригодности
            for (int i = 0; i < individuals.Count; i++)
                temp[i] = allFitnesses[i, 1];
            Array.Sort(temp);
            for (int i = 0; i < individuals.Count; i++)
                for (int j = 0; j < individuals.Count; j++)
                    if (temp[i] == allFitnesses[j, 1])
                    {
                        Swap(ref allFitnesses[i, 1], ref allFitnesses[j, 1]);
                        Swap(ref allFitnesses[i, 0], ref allFitnesses[j, 0]);
                        break;
                    }

            int countNewIndividuals = (int)(individuals.Count * T);  //кол-во особей, прошедших через отбор
            List<int[]> newIndividuals = new List<int[]>();
            int numberNewIndividuals = 0;
            do  //отбор особей в новую популяцию
            {
                numberNewIndividuals = rand.Next(0, countNewIndividuals - 1);
                newIndividuals.Add(new int[countGenes]);
                for (int i = 0; i < countGenes; i++)
                    newIndividuals.Last()[i] = individuals[allFitnesses[numberNewIndividuals, 0]][i];
            } while (newIndividuals.Count < countIndivids);
            individuals.Clear();
            individuals = newIndividuals;
        }

        static void Inbreeding(int[,] individuals, int[,] allFitnesses, ref int[] parent_1, ref int[] parent_2)  //выбор родителей методом имбридинг
        {
            Random rand = new Random();
            int numberParent_1 = rand.Next(0, countIndivids - 1);  //выбор первого родителя          
            for (int i = 0; i < countGenes; i++)
                parent_1[i] = individuals[allFitnesses[numberParent_1, 0], i];

            double[] euclideanDistance = new double[countIndivids];
            double minEuclideanDistance = double.MaxValue;
            int numberParent_2 = 0;
            for (int i = 0; i < countIndivids; i++)  //вычисление Евклидова расстояния для выбора второго родителя
            {
                for (int j = 0; j < countGenes; j++)
                    euclideanDistance[i] += Math.Pow(parent_1[j] - individuals[allFitnesses[i, 0], j], 2);
                euclideanDistance[i] = Math.Sqrt(euclideanDistance[i]);
                if (euclideanDistance[i] < minEuclideanDistance && euclideanDistance[i] != 0)
                {
                    minEuclideanDistance = euclideanDistance[i];
                    numberParent_2 = i;
                }
            }
            for (int i = 0; i < countGenes; i++)
                parent_2[i] = individuals[allFitnesses[numberParent_2, 0], i];
        }

        static void CrossingDiscreteRecombination(int[] parent_1, int[] parent_2, ref int[] descendant_1, ref int[] descendant_2)  //скрещивание методом дискретной рекомбинации
        {
            Random rand = new Random();
            int[,] maskForDiscreteRecombination = new int[2, countGenes];  //маска для замены генов
            for (int i = 0; i < 2; i++)    //выбираем номера особи для замены генов
                for (int j = 0; j < countGenes; j++)
                    maskForDiscreteRecombination[i, j] = rand.Next(0, 2);

            for (int i = 0; i < countGenes; i++)
            {
                if (maskForDiscreteRecombination[0, i] == 1)  //замена генов для первого потомка
                    descendant_1[i] = parent_2[i];
                else
                    descendant_1[i] = parent_1[i];

                if (maskForDiscreteRecombination[1, i] == 0)   //замена генов для второго потомка
                    descendant_2[i] = parent_1[i];
                else
                    descendant_2[i] = parent_2[i];
            }
        }

        static void MutationExchange(ref List<int[]> newIndividuals)  //мутация методом обмена соседних генов
        {
            Random rand = new Random();
            double T = 0.1;  //порог мутации
            double[] mutationProbability = new double[newIndividuals.Count];
            for (int i = 0; i < newIndividuals.Count; i++)
            {
                mutationProbability[i] = ((double)rand.Next(1, 100) / 100);  //случайно выбирается вероятность мутации для каждой особи
                if (mutationProbability[i] <= T)  //если вероятность мутации особи меньше порога
                {
                    int numberGeneMutations = rand.Next(1, countGenes - 1);  //номер гена для мутации                    
                    if (newIndividuals[i][numberGeneMutations - 1] == newIndividuals[i][numberGeneMutations + 1])
                        numberGeneMutations = rand.Next(0, countGenes - 1);
                    if (numberGeneMutations == 0)
                        numberGeneMutations++;
                    Swap(ref newIndividuals[i][numberGeneMutations - 1], ref newIndividuals[i][numberGeneMutations + 1]);
                }
            }
        }

        static void OutputPopulation(int[,] matrWeidths, ref int[,] allFitnesses, List<int[]> newIndividuals)
        {
            int fitness = 0;
            for (int i = 0; i < countIndivids; i++)
            {
                fitness += matrWeidths[firstComp - 1, newIndividuals[i][0]];
                Console.Write("Особь {0}, её гены: ", i + 1);
                for (int j = 0; j < countGenes; j++)
                {
                    if (newIndividuals[i][j] < 9)
                        Console.Write("{0},  ", newIndividuals[i][j] + 1);
                    else
                        Console.Write("{0}, ", newIndividuals[i][j] + 1);
                    if (j > 0)
                        fitness += matrWeidths[newIndividuals[i][j - 1], newIndividuals[i][j]];
                }
                fitness += matrWeidths[newIndividuals[i][countGenes - 1], lastComp - 1];
                Console.WriteLine(" пригодность: " + fitness);
                allFitnesses[i, 0] = i;
                allFitnesses[i, 1] = fitness;
                fitness = 0;
            }
            Console.WriteLine();
        }

        static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }
    }
}
