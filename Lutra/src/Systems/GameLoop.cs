using System.Diagnostics;
using System.Runtime.CompilerServices;
using Lutra.Utility.Profiling;

namespace Lutra
{
    public class GameLoop
    {
        #region Public Properties

        public bool Active;
        public bool FixedTimeStep;

        public double TargetFrameRate { get; private set; }
        public TimeSpan TargetElapsedTime { get; private set; }

        public TimeSpan TotalGameTime { get; private set; }
        public TimeSpan ElapsedGameTime { get; private set; }

        #endregion

        #region Private Variables

        private readonly Game game;
        private bool initialized = false;
        private bool suppressRender = false;

        private Stopwatch gameTimer;
        private TimeSpan accumulatedElapsedTime;
        private long previousTicks = 0;

        private static readonly TimeSpan MaxElapsedTime = TimeSpan.FromMilliseconds(500);
        private const double TicksPerSecond = TimeSpan.TicksPerSecond;

        #endregion

        #region Constructor

        public GameLoop(Game gameInstance, double targetFrameRate)
        {
            game = gameInstance;

            SetTargetFrameRate(targetFrameRate);

            Active = false;
            FixedTimeStep = true;

            TotalGameTime = TimeSpan.Zero;
            ElapsedGameTime = TimeSpan.Zero;
        }

        #endregion

        #region Public Methods

        public void SetTargetFrameRate(double targetFrameRate)
        {
            TargetFrameRate = targetFrameRate;
            TargetElapsedTime = TimeSpan.FromTicks((long)Math.Round(TicksPerSecond / targetFrameRate));
        }

        public void SetFixedTimestep(bool fixedStep)
        {
            FixedTimeStep = fixedStep;
        }

        public void Exit()
        {
            Active = false;
            suppressRender = true;
        }

        public void SuppressDraw()
        {
            suppressRender = true;
        }

        public void Run()
        {
            if (!initialized)
            {
                game.Initialize();
                initialized = true;
            }

            Active = true;

            gameTimer = Stopwatch.StartNew();

            while (Active)
            {
                Profiler.StartFrame();
                Tick();
                Profiler.EndFrame();
            }

            game.ShutDown();
        }

        public void Tick()
        {
            AccumulateTime();

            if (FixedTimeStep && accumulatedElapsedTime < TargetElapsedTime)
            {
                while (accumulatedElapsedTime < TargetElapsedTime)
                {
                    System.Threading.Thread.Sleep(TargetElapsedTime - accumulatedElapsedTime);
                    AccumulateTime();
                }
            }

            if (accumulatedElapsedTime > MaxElapsedTime)
            {
                accumulatedElapsedTime = MaxElapsedTime;
            }

            if (FixedTimeStep)
            {
                ElapsedGameTime = TargetElapsedTime;
                int stepCount = 0;

                while (accumulatedElapsedTime >= TargetElapsedTime)
                {
                    TotalGameTime += TargetElapsedTime;
                    accumulatedElapsedTime -= TargetElapsedTime;
                    stepCount += 1;

                    game.Update();
                }

                ElapsedGameTime = TimeSpan.FromTicks(TargetElapsedTime.Ticks * stepCount);
            }
            else
            {
                ElapsedGameTime = accumulatedElapsedTime;
                TotalGameTime += ElapsedGameTime;

                accumulatedElapsedTime = TimeSpan.Zero;
                game.Update();
            }

            if (suppressRender)
            {
                suppressRender = false;
            }
            else
            {
                game.Render();
            }
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AccumulateTime()
        {
            long currentTicks = gameTimer.Elapsed.Ticks;
            accumulatedElapsedTime += TimeSpan.FromTicks(currentTicks - previousTicks);
            previousTicks = currentTicks;
        }
    }
}
