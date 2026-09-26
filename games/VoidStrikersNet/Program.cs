using System;

namespace VoidStrikersNet;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("=== VOID STRIKERS: .NET SPACE SHOOTER ENGINE ===");
        Console.WriteLine("Initializing photon cannons & shields...");

        int score = 0;
        int wave = 1;
        int playerHealth = 100;

        while (wave <= 5 && playerHealth > 0)
        {
            Console.WriteLine($"\n--- WAVE {wave} INCOMING ---");
            int alienCount = wave * 3;
            for (int i = 1; i <= alienCount; i++)
            {
                Console.WriteLine($"[ENGAGEMENT] Alien fighter #{i} targeted. Firing lasers...");
                score += 150;
                System.Threading.Thread.Sleep(200);
            }
            Console.WriteLine($"[WAVE {wave} CLEARED] Score: {score} | Player Health: {playerHealth}%");
            wave++;
        }

        Console.WriteLine($"\n=== MISSION COMPLETE ===");
        Console.WriteLine($"Final Score: {score}. Victory achieved across all sectors!");
    }
}
