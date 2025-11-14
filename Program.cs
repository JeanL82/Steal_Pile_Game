using System;

namespace StealPileSimple
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.Write("Nombre del jugador 1: ");
            string? input1 = Console.ReadLine();
            string name1 = string.IsNullOrWhiteSpace(input1) ? "Player 1" : input1;

            Console.Write("Nombre del jugador 2: ");
            string? input2 = Console.ReadLine();
            string name2 = string.IsNullOrWhiteSpace(input2) ? "Player 2" : input2;

            Game game = new Game(name1, name2);
            game.Run();
        }
    }
}