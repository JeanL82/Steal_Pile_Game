using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StealPileSimple
{
    public enum Rank
    {
        A = 1, Two = 2, Three = 3, Four = 4, Five = 5, Six = 6,
        Seven = 7, Eight = 8, Nine = 9, Ten = 10,
        J = 11, Q = 12, K = 13
    }

    public enum Suit { Spades, Hearts, Clubs, Diamonds }

    public class Card
    {
        public Rank Rank { get; }
        public Suit Suit { get; }

        public Card(Rank r, Suit s)
        {
            Rank = r;
            Suit = s;
        }

        public override string ToString() => $"{Rank} of {Suit}";
    }

    public class Player
    {
        public string Name { get; }
        public List<Card> Hand { get; } = new();
        public List<Card> Pile { get; } = new();

        public Player(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Player" : name;
        }
    }

    static class Ui
    {
        public static void EnsureUtf8() => Console.OutputEncoding = Encoding.UTF8;

        public static void Separator(int n = 40)
            => Console.WriteLine(new string('#', n));

        public static string CardToGlyph(Card c)
        {
            // Parte del RANK (valor)
            string r;
            if (c.Rank == Rank.A) r = "A";
            else if (c.Rank == Rank.J) r = "J";
            else if (c.Rank == Rank.Q) r = "Q";
            else if (c.Rank == Rank.K) r = "K";
            else r = ((int)c.Rank).ToString(); // 2..10

            // Parte del SUIT (palo)
            string s;
            if (c.Suit == Suit.Hearts) s = "♥";
            else if (c.Suit == Suit.Diamonds) s = "♦";
            else if (c.Suit == Suit.Clubs) s = "♣";
            else if (c.Suit == Suit.Spades) s = "♠";
            else s = "?";

            return r + s;
        }

        private static string[] MakeBox(string content, int width)
        {
            width = Math.Max(width, 6);
            int inner = width - 2;

            if (content.Length > inner)
                content = content[..inner];

            string top = "┌" + new string('─', inner) + "┐";
            int pad = inner - content.Length;
            int leftPad = pad / 2;
            int rightPad = pad - leftPad;
            string mid = "│" + new string(' ', leftPad) + content + new string(' ', rightPad) + "│";
            string bot = "└" + new string('─', inner) + "┘";
            return new[] { top, mid, bot };
        }

        public static void Row(
            string leftContent,
            string? rightContent = null,
            int leftWidth = 44,
            int rightWidth = 12,
            int gap = 2)
        {
            var L = MakeBox(leftContent, leftWidth);

            if (rightContent == null)
            {
                Console.WriteLine(L[0]);
                Console.WriteLine(L[1]);
                Console.WriteLine(L[2]);
                return;
            }

            var R = MakeBox(rightContent, rightWidth);

            Console.WriteLine(L[0] + new string(' ', gap) + R[0]);
            Console.WriteLine(L[1] + new string(' ', gap) + R[1]);
            Console.WriteLine(L[2] + new string(' ', gap) + R[2]);
        }

        public static string HandToText(IEnumerable<Card> cards)
            => string.Join("  ", cards.Select(CardToGlyph));

        public static string PileToText(List<Card> pile)
        {
            if (pile.Count == 0) return "";
            var top = pile[pile.Count - 1];
            return $"Top {CardToGlyph(top)} ({pile.Count})";
        }
    }

    public class Game
    {
        private readonly Player p1;
        private readonly Player p2;
        private readonly List<Card> deck = new();
        private readonly List<Card> center = new();
        private readonly Random rng = new();

        public Game(string name1, string name2)
        {
            p1 = new Player(string.IsNullOrWhiteSpace(name1) ? "Player 1" : name1);
            p2 = new Player(string.IsNullOrWhiteSpace(name2) ? "Player 2" : name2);
        }

        public void Run()
        {
            Ui.EnsureUtf8();

            InitDeck();
            DealInitial();
            Player current = rng.Next(2) == 0 ? p1 : p2;

            while (!IsGameOver())
            {
                Console.Clear();
                PrintBoard();
                Turn(current);
                current = (current == p1) ? p2 : p1;
            }

            Console.Clear();
            PrintBoard();
            ShowWinner();
            Console.WriteLine("\nPresiona ENTER para salir...");
            Console.ReadLine();
        }

        private void InitDeck()
        {
            deck.Clear();
            foreach (Suit s in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank r in Enum.GetValues(typeof(Rank)))
                {
                    deck.Add(new Card(r, s));
                }
            }
            Shuffle(deck);
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        // Puede devolver null → Card?
        private bool TryDraw(out Card? card)
        {
            if (deck.Count == 0)
            {
                card = null;
                return false;
            }

            card = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            return true;
        }

        private Card? Draw()
        {
            if (deck.Count == 0) return null;
            Card c = deck[deck.Count - 1];
            deck.RemoveAt(deck.Count - 1);
            return c;
        }

        private void DealInitial()
        {
            // 4 para cada jugador
            for (int i = 0; i < 4; i++)
            {
                if (TryDraw(out var c1) && c1 != null) p1.Hand.Add(c1);
                if (TryDraw(out var c2) && c2 != null) p2.Hand.Add(c2);
            }

            // 4 al centro
            for (int i = 0; i < 4; i++)
            {
                if (TryDraw(out var c) && c != null) center.Add(c);
            }
        }

        private void PrintBoard()
        {
            Console.WriteLine($"{p1.Name}");
            Ui.Row(Ui.HandToText(p1.Hand), Ui.PileToText(p1.Pile));
            Ui.Separator();

            Ui.Row(Ui.HandToText(center));
            Ui.Separator();

            Ui.Row(Ui.HandToText(p2.Hand), Ui.PileToText(p2.Pile));
            Console.WriteLine($"\n{p2.Name}");
            Console.WriteLine($"\nMazo: {deck.Count} cartas");
        }

        private void Turn(Player player)
        {
            // Si no tiene cartas y queda mazo, roba 1
            if (player.Hand.Count == 0 && deck.Count > 0)
            {
                Card? extra = Draw();
                if (extra != null)
                    player.Hand.Add(extra);
            }

            if (player.Hand.Count == 0)
            {
                Pause($"{player.Name} no tiene cartas. Se pasa turno...");
                return;
            }

            Console.WriteLine();
            int idx = ReadIndex(
                $"{player.Name}, elige carta (1..{player.Hand.Count}): ",
                1,
                player.Hand.Count
            );
            idx--; // convertir a índice 0-based

            var played = player.Hand[idx];
            player.Hand.RemoveAt(idx);

            var rival = (player == p1) ? p2 : p1;

            // 1) Intentar robar el pile del rival
            Card? rivalTop = (rival.Pile.Count > 0) ? rival.Pile[rival.Pile.Count - 1] : null;
            if (rivalTop != null && rivalTop.Rank == played.Rank)
            {
                player.Pile.AddRange(rival.Pile);
                rival.Pile.Clear();
                player.Pile.Add(played);
                Pause($"¡ROBO! {player.Name} roba el pile de {rival.Name} con {Ui.CardToGlyph(played)}.");
            }
            else
            {
                // 2) Intentar capturar del centro
                int ci = center.FindIndex(c => c.Rank == played.Rank);
                if (ci >= 0)
                {
                    var captured = center[ci];
                    center.RemoveAt(ci);
                    player.Pile.Add(captured);
                    player.Pile.Add(played);
                    Pause($"{player.Name} captura {Ui.CardToGlyph(captured)} con {Ui.CardToGlyph(played)}.");
                }
                else
                {
                    // 3) Dejar carta en el centro
                    center.Add(played);
                    Pause($"{player.Name} deja {Ui.CardToGlyph(played)} en el centro.");
                }
            }

            // Robar al final del turno
            Card? d = Draw();
            if (d != null)
            {
                player.Hand.Add(d);
                Pause($"{player.Name} roba 1 carta.");
            }
        }

        private bool IsGameOver()
            => deck.Count == 0 && p1.Hand.Count == 0 && p2.Hand.Count == 0;

        private void ShowWinner()
        {
            Console.WriteLine("=== FIN DEL JUEGO ===");
            Console.WriteLine($"{p1.Name} Pile: {p1.Pile.Count} cartas");
            Console.WriteLine($"{p2.Name} Pile: {p2.Pile.Count} cartas");

            if (p1.Pile.Count > p2.Pile.Count)
                Console.WriteLine($"Ganador: {p1.Name}");
            else if (p2.Pile.Count > p1.Pile.Count)
                Console.WriteLine($"Ganador: {p2.Name}");
            else
                Console.WriteLine("¡Empate!");
        }

        private static int ReadIndex(string prompt, int min, int max)
        {
            while (true)
            {
                Console.Write(prompt);
                string? s = Console.ReadLine();

                if (int.TryParse(s, out int v) && v >= min && v <= max)
                    return v;

                Console.WriteLine("Entrada inválida.");
            }
        }

        private static void Pause(string msg)
        {
            Console.WriteLine(msg);
            Console.Write("Continuar (ENTER)...");
            Console.ReadLine();
        }
    }
}