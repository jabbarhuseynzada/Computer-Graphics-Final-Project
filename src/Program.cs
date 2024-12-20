using Raylib_cs;
using System.Numerics;

class PowerUp
{
    public Vector2 Position;
    public required string Type { get; set; }  // "Health", "Speed", "Shield", "RapidFire"
    public float Duration { get; set; }
    public Color Color { get; set; }
}

class Enemy
{
    public Vector2 Position;
    public required string Type { get; set; }  // "Basic", "Fast", "Tank", "Shooter"
    public float Speed { get; set; }
    public int Health { get; set; }
    public Color Color { get; set; }
    public float ShootTimer { get; set; }
}

class Bullet
{
    public Vector2 Position;
    public Vector2 Direction { get; set; }
    public float Speed { get; set; }
    public bool IsPlayerBullet { get; set; }
}

class Player
{
    public Vector2 Position;
    public float Speed { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public bool HasShield { get; set; }
    public float ShieldDuration { get; set; }
    public bool HasRapidFire { get; set; }
    public float RapidFireDuration { get; set; }
    public float ShootCooldown { get; set; }
    public float CurrentCooldown { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        const int screenWidth = 800;
        const int screenHeight = 600;
        Raylib.InitWindow(screenWidth, screenHeight, "Computer Graphics Project by Jabbar, Rufat & Rashid");
        Raylib.SetTargetFPS(60);

        // Initialize game objects
        Player player = new Player
        {
            Position = new Vector2(screenWidth / 2, screenHeight - 50),
            Speed = 200f,
            Health = 100,
            MaxHealth = 100,
            ShootCooldown = 0.5f,
            CurrentCooldown = 0f
        };

        List<Bullet> bullets = new List<Bullet>();
        List<Enemy> enemies = new List<Enemy>();
        List<PowerUp> powerUps = new List<PowerUp>();

        Random random = new Random();
        int score = 0;
        float gameTime = 0f;
        bool gameOver = false;
        int wave = 1;
        float waveTimer = 0f;
        const float waveDuration = 30f;

        while (!Raylib.WindowShouldClose())
        {
            float deltaTime = Raylib.GetFrameTime();

            if (!gameOver)
            {
                gameTime += deltaTime;
                waveTimer += deltaTime;

                // Wave Management
                if (waveTimer >= waveDuration)
                {
                    wave++;
                    waveTimer = 0f;
                    SpawnWave(enemies, wave, screenWidth, random);
                }

                // Update Player
                UpdatePlayer(player, deltaTime, screenWidth, screenHeight);

                // Shooting
                player.CurrentCooldown -= deltaTime;
                if (Raylib.IsKeyDown(KeyboardKey.Space) && player.CurrentCooldown <= 0)
                {
                    float actualCooldown = player.HasRapidFire ? player.ShootCooldown / 3 : player.ShootCooldown;
                    player.CurrentCooldown = actualCooldown;

                    bullets.Add(new Bullet
                    {
                        Position = player.Position + new Vector2(20, 0),
                        Direction = new Vector2(0, -1),
                        Speed = 400f,
                        IsPlayerBullet = true
                    });
                }

                // Update PowerUps
                UpdatePowerUps(player, powerUps, deltaTime);

                // Random PowerUp Spawning
                if (random.NextDouble() < 0.001)
                {
                    SpawnPowerUp(powerUps, screenWidth, screenHeight, random);
                }

                // Update Enemies
                UpdateEnemies(enemies, bullets, player, deltaTime, screenWidth, screenHeight, random);

                // Update Bullets
                UpdateBullets(bullets, screenHeight);

                // Check Collisions
                CheckCollisions(player, enemies, bullets, powerUps, ref score, ref gameOver);
            }
            static void DrawGameOver(int score, int wave, int screenWidth, int screenHeight)
            {
                string gameOverText = "GAME OVER";
                string scoreText = $"Final Score: {score}";
                string waveText = $"Waves Survived: {wave}";
                string restartText = "Press R to Restart";

                // Calculate text positions for centered alignment
                int gameOverTextWidth = Raylib.MeasureText(gameOverText, 60);
                int scoreTextWidth = Raylib.MeasureText(scoreText, 30);
                int waveTextWidth = Raylib.MeasureText(waveText, 30);
                int restartTextWidth = Raylib.MeasureText(restartText, 20);

                // Draw game over screen text
                Raylib.DrawText(gameOverText, (screenWidth - gameOverTextWidth) / 2, screenHeight / 3, 60, Color.Red);
                Raylib.DrawText(scoreText, (screenWidth - scoreTextWidth) / 2, screenHeight / 2, 30, Color.White);
                Raylib.DrawText(waveText, (screenWidth - waveTextWidth) / 2, (screenHeight / 2) + 40, 30, Color.White);
                Raylib.DrawText(restartText, (screenWidth - restartTextWidth) / 2, (screenHeight / 2) + 100, 20, Color.Gray);
            }

            static void ResetGame(ref Player player, List<Enemy> enemies, List<Bullet> bullets, List<PowerUp> powerUps,
                ref int score, ref float gameTime, ref int wave, ref float waveTimer, ref bool gameOver, int screenWidth)
            {
                // Reset player
                player.Position = new Vector2(screenWidth / 2, 550);
                player.Health = player.MaxHealth;
                player.Speed = 200f;
                player.HasShield = false;
                player.HasRapidFire = false;
                player.ShieldDuration = 0f;
                player.RapidFireDuration = 0f;
                player.CurrentCooldown = 0f;

                // Clear lists
                enemies.Clear();
                bullets.Clear();
                powerUps.Clear();

                // Reset game state
                score = 0;
                gameTime = 0f;
                wave = 1;
                waveTimer = 0f;
                gameOver = false;
            }

            static void DrawGame(Player player, List<Enemy> enemies, List<Bullet> bullets, List<PowerUp> powerUps, int score, int wave, float waveTimer, float waveDuration, int screenWidth)
            {
                // Draw player
                Color playerColor = player.HasShield ? new Color(173, 216, 230, 255) : Color.Blue;
                Raylib.DrawRectangle((int)player.Position.X, (int)player.Position.Y, 40, 40, playerColor);

                // Draw enemies
                foreach (var enemy in enemies)
                {
                    Raylib.DrawRectangle((int)enemy.Position.X, (int)enemy.Position.Y, 40, 40, enemy.Color);
                }

                // Draw bullets
                foreach (var bullet in bullets)
                {
                    Color bulletColor = bullet.IsPlayerBullet ? Color.Yellow : Color.Red;
                    Raylib.DrawRectangle((int)bullet.Position.X, (int)bullet.Position.Y, 5, 10, bulletColor);
                }

                // Draw power-ups
                foreach (var powerUp in powerUps)
                {
                    Raylib.DrawRectangle((int)powerUp.Position.X, (int)powerUp.Position.Y, 30, 30, powerUp.Color);
                }

                // Draw HUD
                Raylib.DrawText($"Score: {score}", 10, 10, 20, Color.White);
                Raylib.DrawText($"Health: {player.Health}", 10, 40, 20, Color.White);
                Raylib.DrawText($"Wave: {wave}", 10, 70, 20, Color.White);

                // Draw wave timer
                float waveTimeLeft = waveDuration - waveTimer;
                Raylib.DrawText($"Next Wave: {waveTimeLeft:F1}s", screenWidth - 200, 10, 20, Color.White);

                // Draw active power-ups
                if (player.HasShield)
                {
                    Raylib.DrawText($"Shield: {player.ShieldDuration:F1}s", 10, 100, 20, Color.Blue);
                }
                if (player.HasRapidFire)
                {
                    Raylib.DrawText($"Rapid Fire: {player.RapidFireDuration:F1}s", 10, 130, 20, Color.Orange);
                }
            }

            // Drawing
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            if (!gameOver)
            {
                DrawGame(player, enemies, bullets, powerUps, score, wave, waveTimer, waveDuration, screenWidth);
            }
            else
            {
                DrawGameOver(score, wave, screenWidth, screenHeight);

                if (Raylib.IsKeyPressed(KeyboardKey.R))
                {
                    ResetGame(ref player, enemies, bullets, powerUps, ref score, ref gameTime, ref wave, ref waveTimer, ref gameOver, screenWidth);
                }
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }

    static void SpawnWave(List<Enemy> enemies, int wave, int screenWidth, Random random)
    {
        int baseEnemies = 5 + wave * 2;

        for (int i = 1; i < baseEnemies; i++)
        {
            string type = random.Next(100) switch
            {
                < 50 => "Basic",
                < 75 => "Fast",
                < 90 => "Tank",
                _ => "Shooter"
            };

            Enemy enemy = new Enemy
            {
                Position = new Vector2(random.Next(screenWidth - 40), -50),
                Type = type,
                Speed = type switch
                {
                    "Basic" => 100f,
                    "Fast" => 150f,
                    "Tank" => 50f,
                    "Shooter" => 75f,
                    _ => 100f
                },
                Health = type switch
                {
                    "Basic" => 1,
                    "Fast" => 1,
                    "Tank" => 3,
                    "Shooter" => 2,
                    _ => 1
                },
                Color = type switch
                {
                    "Basic" => Color.Red,
                    "Fast" => Color.Green,
                    "Tank" => Color.Purple,
                    "Shooter" => Color.Orange,
                    _ => Color.Red
                },
                ShootTimer = 0f
            };

            enemies.Add(enemy);
        }
    }

    static void UpdatePlayer(Player player, float deltaTime, int screenWidth, int screenHeight)
    {
        if (Raylib.IsKeyDown(KeyboardKey.W)) player.Position.Y -= player.Speed * deltaTime;
        if (Raylib.IsKeyDown(KeyboardKey.S)) player.Position.Y += player.Speed * deltaTime;
        if (Raylib.IsKeyDown(KeyboardKey.A)) player.Position.X -= player.Speed * deltaTime;
        if (Raylib.IsKeyDown(KeyboardKey.D)) player.Position.X += player.Speed * deltaTime;

        // Clamp player position to screen bounds
        player.Position.X = Math.Clamp(player.Position.X, 0, screenWidth - 40);
        player.Position.Y = Math.Clamp(player.Position.Y, 0, screenHeight - 40);

        if (player.HasShield)
        {
            player.ShieldDuration -= deltaTime;
            if (player.ShieldDuration <= 0) player.HasShield = false;
        }

        if (player.HasRapidFire)
        {
            player.RapidFireDuration -= deltaTime;
            if (player.RapidFireDuration <= 0) player.HasRapidFire = false;
        }
    }

    static void UpdatePowerUps(Player player, List<PowerUp> powerUps, float deltaTime)
    {
        for (int i = powerUps.Count - 1; i >= 0; i--)
        {
            powerUps[i].Duration -= deltaTime;
            if (powerUps[i].Duration <= 0)
            {
                powerUps.RemoveAt(i);
            }
        }
    }

    static void SpawnPowerUp(List<PowerUp> powerUps, int screenWidth, int screenHeight, Random random)
    {
        string[] types = { "Health", "Speed", "Shield", "RapidFire" };
        string type = types[random.Next(types.Length)];

        PowerUp powerUp = new PowerUp
        {
            Position = new Vector2(random.Next(screenWidth - 30), random.Next(screenHeight - 30)),
            Type = type,
            Duration = 10f,
            Color = type switch
            {
                "Health" => Color.Green,
                "Speed" => Color.Yellow,
                "Shield" => Color.Blue,
                "RapidFire" => Color.Orange,
                _ => Color.White
            }
        };

        powerUps.Add(powerUp);
    }

    static void UpdateEnemies(List<Enemy> enemies, List<Bullet> bullets, Player player, float deltaTime, int screenWidth, int screenHeight, Random random)
    {
        foreach (var enemy in enemies)
        {
            Vector2 direction = Vector2.Normalize(player.Position - enemy.Position);
            enemy.Position += direction * enemy.Speed * deltaTime;

            if (enemy.Type == "Shooter")
            {
                enemy.ShootTimer += deltaTime;
                if (enemy.ShootTimer >= 2f)
                {
                    enemy.ShootTimer = 0f;
                    bullets.Add(new Bullet
                    {
                        Position = enemy.Position,
                        Direction = direction,
                        Speed = 200f,
                        IsPlayerBullet = false
                    });
                }
            }
        }
    }

    static void UpdateBullets(List<Bullet> bullets, int screenHeight)
    {
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            bullets[i].Position += bullets[i].Direction * bullets[i].Speed * Raylib.GetFrameTime();

            if (bullets[i].Position.Y < -10 || bullets[i].Position.Y > screenHeight + 10)
            {
                bullets.RemoveAt(i);
            }
        }
    }

    static bool CheckCollisionRecs(Vector2 pos1, Vector2 size1, Vector2 pos2, Vector2 size2)
    {
        Rectangle rec1 = new Rectangle(pos1.X, pos1.Y, size1.X, size1.Y);
        Rectangle rec2 = new Rectangle(pos2.X, pos2.Y, size2.X, size2.Y);
        return Raylib.CheckCollisionRecs(rec1, rec2);
    }

    static void CheckCollisions(Player player, List<Enemy> enemies, List<Bullet> bullets, List<PowerUp> powerUps, ref int score, ref bool gameOver)
    {
        // Player-Enemy Collisions
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (CheckCollisionRecs(player.Position, new Vector2(40, 40), enemies[i].Position, new Vector2(40, 40)))
            {
                if (!player.HasShield)
                {
                    player.Health -= 20;
                    if (player.Health <= 0)
                    {
                        gameOver = true;
                    }
                }
                enemies.RemoveAt(i);
            }
        }

        // Bullet Collisions
        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            if (bullets[i].IsPlayerBullet)
            {
                // Check bullet collision with enemies
                for (int j = enemies.Count - 1; j >= 0; j--)
                {
                    if (CheckCollisionRecs(bullets[i].Position, new Vector2(5, 10), enemies[j].Position, new Vector2(40, 40)))
                    {
                        enemies[j].Health--;
                        bullets.RemoveAt(i);

                        if (enemies[j].Health <= 0)
                        {
                            enemies.RemoveAt(j);
                            score += 100;
                        }
                        break;
                    }
                }
            }
            else
            {
                // Check enemy bullet collision with player
                if (CheckCollisionRecs(bullets[i].Position, new Vector2(5, 10), player.Position, new Vector2(40, 40)))
                {
                    if (!player.HasShield)
                    {
                        player.Health -= 10;
                        if (player.Health <= 0)
                        {
                            gameOver = true;
                        }
                    }
                    bullets.RemoveAt(i);
                }
            }
        }

        // PowerUp Collisions
        for (int i = powerUps.Count - 1; i >= 0; i--)
        {
            if (CheckCollisionRecs(player.Position, new Vector2(40, 40), powerUps[i].Position, new Vector2(30, 30)))
            {
                ApplyPowerUp(player, powerUps[i]);
                powerUps.RemoveAt(i);
            }
        }
    }

    static void ApplyPowerUp(Player player, PowerUp powerUp)
    {
        switch (powerUp.Type)
        {
            case "Health":
                player.Health = Math.Min(player.Health + 30, player.MaxHealth);
                break;
            case "Speed":
                player.Speed += 50;
                break;
            case "Shield":
                player.HasShield = true;
                player.ShieldDuration = 5f;
                break;
            case "RapidFire":
                player.HasRapidFire = true;
                player.RapidFireDuration = 5f;
                break;
        }
    }

    static void DrawGame(Player player, List<Enemy> enemies, List<Bullet> bullets, List<PowerUp> powerUps, int score, int wave, float waveTimer, float waveDuration, int screenWidth)
    {
        // Draw player
        Color playerColor = player.HasShield ? new Color(173, 216, 230, 255) : Color.Blue;
        Raylib.DrawRectangle((int)player.Position.X, (int)player.Position.Y, 40, 40, playerColor);

        // Draw enemies
        foreach (var enemy in enemies)
        {
            Raylib.DrawRectangle((int)enemy.Position.X, (int)enemy.Position.Y, 40, 40, enemy.Color);
        }

        // Draw bullets
        foreach (var bullet in bullets)
        {
            Color bulletColor = bullet.IsPlayerBullet ? Color.Yellow : Color.Red;
            Raylib.DrawRectangle((int)bullet.Position.X, (int)bullet.Position.Y, 5, 10, bulletColor);
        }

        // Draw power-ups
        foreach (var powerUp in powerUps)
        {
            Raylib.DrawRectangle((int)powerUp.Position.X, (int)powerUp.Position.Y, 30, 30, powerUp.Color);
        }

        // Draw HUD
        Raylib.DrawText($"Score: {score}", 10, 10, 20, Color.White);
        DrawGame(player, enemies, bullets, powerUps, score, wave, waveTimer, waveDuration, screenWidth);
    }
}