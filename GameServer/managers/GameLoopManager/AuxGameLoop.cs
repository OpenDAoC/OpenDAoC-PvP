using System;
using System.Reflection;
using System.Threading;
using log4net;

namespace DOL.GS
{
    //The AuxGameLoop is for Timers that do not need to be "realtime" and part of the main Game Loop. 
    //This is for things like quit timers, player init, world init, etc where we dont really care how long it takes as long as it doesnt affect the main Game Loop.
    public static class AuxGameLoop
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static long GameLoopTime=0;
        private const string PerfCounterName = "AuxGameLoop";
        private static Thread m_GameLoopThread;

        //GameLoop tick timer -> will adjust based on the performance
        private static long _tickDueTime = 50;
        private static Timer _timerRef;
        
        //Max player count is 4000
        public static GamePlayer[] players = new GamePlayer[4000];
        private static int _lastPlayerIndex = 0;

        public static String currentServiceTick;

        public static long TickRate { get { return _tickDueTime; } }

        public static bool Init()
        {
            m_GameLoopThread = new Thread(new ThreadStart(GameLoopThreadStart));
            m_GameLoopThread.Priority = ThreadPriority.AboveNormal;
            m_GameLoopThread.Name = "AuxGameLoop";
            m_GameLoopThread.IsBackground = true;
            m_GameLoopThread.Start();
            
            return true;
        }

        public static void Exit()
        {
            m_GameLoopThread.Interrupt();
            m_GameLoopThread = null;
        }

        private static void GameLoopThreadStart()
        {
            _timerRef = new Timer(Tick, null, 0, Timeout.Infinite);
        }

        private static void Tick(object obj)
        {
            // ECS.Debug.Diagnostics.StartPerfCounter(PerfCounterName);
            
            //Make sure the tick < gameLoopTick
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();

            AuxTimerService.Tick(GameLoopTime);


            //Always tick last!
           
            // ECS.Debug.Diagnostics.Tick();
            

            // ECS.Debug.Diagnostics.StopPerfCounter(PerfCounterName);

            GameLoopTime = GameTimer.GetTickCount();

            stopwatch.Stop();
            var elapsed = (float)stopwatch.Elapsed.TotalMilliseconds;
            if(elapsed > 25)
                log.Warn($"Long AuxGameLoop Time: {elapsed}ms");

            //We need to delay our next threading time to the default tick time. If this is > 0, we delay the next tick until its met to maintain consistent tick rate
            var diff = (int) (_tickDueTime - elapsed);
            if (diff <= 0)
            {
                //Console.WriteLine($"Tick rate unable to keep up with load! Elapsed: {elapsed}");
                _timerRef.Change(0, Timeout.Infinite);
                return;
            }

            _timerRef.Change(diff, Timeout.Infinite);
        }
    }
}